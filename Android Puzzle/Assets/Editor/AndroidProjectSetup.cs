using System;
using System.IO;
using System.Linq;
using Puzzle.UI;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Puzzle.Editor
{
    public static class AndroidProjectSetup
    {
        public const string MainScenePath = "Assets/Scenes/Main.unity";
        public const string MobilePipelinePath = "Assets/Settings/Mobile2D.asset";

        // Explicit initialization only: future user changes are never reset on editor startup/build.
        [MenuItem("Puzzle/Android/Apply Base Settings")]
        public static void ApplyBaseSettings()
        {
            PlayerSettings.companyName = "PuzzleStudio";
            PlayerSettings.productName = "데굴데굴";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.puzzlestudio.androidpuzzle");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.defaultScreenWidth = 1080;
            PlayerSettings.defaultScreenHeight = 1920;
            PlayerSettings.runInBackground = false;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.optimizedFramePacing = true;
            PlayerSettings.Android.renderOutsideSafeArea = true;
            PlayerSettings.Android.forceInternetPermission = false;
            PlayerSettings.Android.forceSDCardPermission = false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Minimal);
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
            EditorSettings.serializationMode = SerializationMode.ForceText;
            EditorSettings.projectGenerationRootNamespace = "Puzzle";

            if (!File.Exists(MobilePipelinePath))
            {
                if (!AssetDatabase.CopyAsset("Assets/Settings/UniversalRP.asset", MobilePipelinePath))
                    throw new InvalidOperationException("Could not create the mobile render pipeline.");
            }
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(MobilePipelinePath);
            pipeline.supportsHDR = false;
            pipeline.msaaSampleCount = 1;
            pipeline.renderScale = 1f;
            pipeline.supportsCameraDepthTexture = false;
            pipeline.supportsCameraOpaqueTexture = false;
            pipeline.useSRPBatcher = true;
            EditorUtility.SetDirty(pipeline);

            // Use the existing Low tier as the mobile tier, preserving desktop quality options.
            int previousQuality = QualitySettings.GetQualityLevel();
            QualitySettings.SetQualityLevel(1, false);
            QualitySettings.renderPipeline = pipeline;
            QualitySettings.vSyncCount = 0;
            QualitySettings.globalTextureMipmapLimit = 0;
            QualitySettings.SetQualityLevel(previousQuality, false);
            var qualityAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset")[0];
            var quality = new SerializedObject(qualityAsset);
            var defaults = quality.FindProperty("m_PerPlatformDefaultQuality");
            bool androidFound = false;
            for (int i = 0; i < defaults.arraySize; i++)
            {
                var entry = defaults.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("first").stringValue != "Android") continue;
                entry.FindPropertyRelative("second").intValue = 1;
                androidFound = true;
            }
            if (!androidFound) throw new InvalidOperationException("Android quality default not found.");
            quality.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log("Android base settings applied (portrait, API 26+, IL2CPP, ARM64, OpenGLES3).");
        }

        [MenuItem("Puzzle/Android/Create Starter Scene")]
        public static void CreateStarterScene()
        {
            if (File.Exists(MainScenePath))
            {
                Debug.Log("Main scene already exists; keeping its contents.");
                return;
            }
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            if (!AssetDatabase.CopyAsset("Assets/Scenes/SampleScene.unity", MainScenePath))
                throw new InvalidOperationException("Could not copy the 2D template scene.");
            var scene = EditorSceneManager.OpenScene(MainScenePath);
            var camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            camera.backgroundColor = new Color(0.08f, 0.10f, 0.16f);
            camera.allowHDR = false;

            var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.layer = LayerMask.NameToLayer("UI");
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var safeArea = new GameObject("SafeArea", typeof(RectTransform));
            safeArea.layer = canvasObject.layer;
            safeArea.transform.SetParent(canvasObject.transform, false);
            var rect = safeArea.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            safeArea.AddComponent<SafeAreaFitter>();
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            EditorSceneManager.SaveScene(scene);
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != MainScenePath).ToList();
            // The untouched template remains available as an asset, outside the build.
            scenes.RemoveAll(s => s.path == "Assets/Scenes/SampleScene.unity");
            scenes.Insert(0, new EditorBuildSettingsScene(MainScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("Main scene created with a 1080x1920 UI canvas, safe area, and touch input.");
        }

        public static void InitializeProject()
        {
            ApplyBaseSettings();
            CreateStarterScene();
            ValidateProject();
        }

        [MenuItem("Puzzle/Android/Validate Project")]
        public static void ValidateProject()
        {
            if (PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) != ScriptingImplementation.IL2CPP ||
                PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
                throw new BuildFailedException("Expected IL2CPP and ARM64 for Android.");
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).ToArray();
            if (scenes.Length == 0 || scenes.Any(s => !File.Exists(s.path)))
                throw new BuildFailedException("Add an existing enabled scene to the build scene list.");
            if (AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(MobilePipelinePath) == null)
                throw new BuildFailedException("Mobile2D render pipeline asset is missing.");
            Debug.Log("Android project validation passed.");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
                Debug.LogWarning("Android Build Support is missing. Install it with SDK/NDK Tools and OpenJDK using Unity Hub.");
        }
    }
}
