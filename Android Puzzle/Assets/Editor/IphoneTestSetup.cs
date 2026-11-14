using System;
using System.IO;
using System.Linq;
using Puzzle.Development;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Editor
{
    public static class IphoneTestSetup
    {
        public const string TestScene = "Assets/Scenes/DeviceTouchTest.unity";

        [MenuItem("Puzzle/iPhone Test/Prepare Test Environment")]
        public static void Prepare()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.dataCaching = false;
            PlayerSettings.WebGL.template = "PROJECT:MobileTest";
            PlayerSettings.WebGL.initialMemorySize = 128;
            PlayerSettings.WebGL.maximumMemorySize = 512;
            var quality = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset")[0]);
            var defaults = quality.FindProperty("m_PerPlatformDefaultQuality");
            for (int i = 0; i < defaults.arraySize; i++)
            {
                var entry = defaults.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("first").stringValue == "WebGL")
                    entry.FindPropertyRelative("second").intValue = 1;
            }
            quality.ApplyModifiedPropertiesWithoutUndo();
            // USB Remote is optional. The actual iPhone must be selected after connecting it.
            EditorSettings.unityRemoteCompression = "JPEG";
            EditorSettings.unityRemoteResolution = "Normal";
            CreateTestScene();
            AssetDatabase.SaveAssets();
            Debug.Log("iPhone browser/Remote test environment prepared. Game build scenes were preserved.");
        }

        [MenuItem("Puzzle/iPhone Test/Open Touch Test Scene")]
        public static void OpenTestScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            CreateTestScene();
            EditorSceneManager.OpenScene(TestScene);
        }

        private static void CreateTestScene()
        {
            if (File.Exists(TestScene)) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!AssetDatabase.CopyAsset(AndroidProjectSetup.MainScenePath, TestScene))
                throw new InvalidOperationException("Could not copy Main for the isolated touch test scene.");
            var scene = EditorSceneManager.OpenScene(TestScene);
            var parent = GameObject.Find("Canvas/SafeArea").transform;
            Label(parent, "Title", "iPHONE TOUCH TEST", 58, 0.83f, 0.94f);
            Label(parent, "Instructions", "Tap and drag inside the panel.\nTry two fingers together.", 34, 0.72f, 0.82f);
            var status = Label(parent, "Input Status", "Waiting for input", 32, 0.59f, 0.70f);
            var panel = new GameObject("Touch Panel", typeof(RectTransform), typeof(Image), typeof(DeviceTouchProbe));
            panel.transform.SetParent(parent, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.06f, 0.18f);
            panelRect.anchorMax = new Vector2(0.94f, 0.57f);
            panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.15f, 0.23f, 0.36f);
            var marker = new GameObject("Touch Marker", typeof(RectTransform), typeof(Image));
            marker.transform.SetParent(panel.transform, false);
            marker.GetComponent<Image>().color = new Color(0.28f, 0.95f, 0.72f);
            marker.GetComponent<Image>().raycastTarget = false;
            marker.GetComponent<RectTransform>().sizeDelta = new Vector2(48, 48);
            var probe = panel.GetComponent<DeviceTouchProbe>();
            probe.statusLabel = status;
            probe.marker = marker.GetComponent<RectTransform>();
            probe.eventLabel = Label(parent, "Events", "Touch panel ready", 32, 0.10f, 0.17f);
            Label(parent, "Footer", "Development input check", 28, 0.03f, 0.09f);
            EditorSceneManager.SaveScene(scene);
            // This diagnostic scene is deliberately not added to the Android/iOS build list.
        }

        private static Text Label(Transform parent, string name, string message, int size, float bottom, float top)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.06f, bottom);
            rect.anchorMax = new Vector2(0.94f, top);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            text.text = message;
            return text;
        }

        [MenuItem("Puzzle/iPhone Test/Switch to Web")]
        public static void SwitchToWeb()
        {
            RequireWebSupport();
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new BuildFailedException("Could not switch to Web.");
        }

        [MenuItem("Puzzle/iPhone Test/Build Touch Test (Web)")]
        public static void BuildTouchTest() => Build(new[] { TestScene });

        [MenuItem("Puzzle/iPhone Test/Build Game (Web)")]
        public static void BuildGame() => Build(EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray());

        private static void RequireWebSupport()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new BuildFailedException("Install Web Build Support for this Unity version using Unity Hub.");
        }

        private static void Build(string[] scenes)
        {
            RequireWebSupport();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                throw new BuildFailedException("Run Switch to Web first, or use -buildTarget WebGL in batch mode.");
            if (scenes.Length == 0 || scenes.Any(s => !File.Exists(s)))
                throw new BuildFailedException("No valid scenes. Run Prepare Test Environment first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.WebGL,Il2CppCodeGeneration.OptimizeSize);
            UnityEditor.WebGL.UserBuildSettings.codeOptimization=UnityEditor.WebGL.WasmCodeOptimization.DiskSize;
            using var packedBrand=new PackedBrandBuild();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                target = BuildTarget.WebGL,
                locationPathName = "Builds/iPhoneWebTest",
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"Web test build failed: {report.summary.result}");
            Debug.Log("iPhone web test built successfully. Run Tools/Serve-IPhoneTest.ps1 from the repository.");
        }
    }
}
