using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Puzzle.Editor
{
    public static class AndroidBuild
    {
        [MenuItem("Puzzle/Android/Switch to Android")]
        public static void SwitchToAndroid()
        {
            RequireAndroidSupport();
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                throw new BuildFailedException("Could not switch to Android.");
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC2;
        }

        [MenuItem("Puzzle/Android/Build Development APK")]
        public static void BuildDevelopmentApk() => Build(false);

        [MenuItem("Puzzle/Android/Build Release AAB")]
        public static void BuildReleaseAab() => Build(true);

        private static void RequireAndroidSupport()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
                throw new BuildFailedException("In Unity Hub, install Android Build Support, Android SDK & NDK Tools, and OpenJDK for this editor version.");
        }

        private static void Build(bool release)
        {
            RequireAndroidSupport();
            // Switching platforms reloads scripts. Keep that operation separate from a build.
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                throw new BuildFailedException("Run Puzzle > Android > Switch to Android first. In batch mode, pass -buildTarget Android.");
            AndroidProjectSetup.ValidateProject();
            if (release && (!PlayerSettings.Android.useCustomKeystore ||
                            string.IsNullOrWhiteSpace(PlayerSettings.Android.keystoreName) ||
                            string.IsNullOrWhiteSpace(PlayerSettings.Android.keyaliasName)))
                throw new BuildFailedException("Configure your release keystore and key alias in Player > Publishing Settings before building a release AAB.");
            if (!Application.isBatchMode && !UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string output = release ? "Builds/Android/AndroidPuzzle.aab" : "Builds/Android/AndroidPuzzle-dev.apk";
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            bool previousBundle = EditorUserBuildSettings.buildAppBundle;
            bool previousExport = EditorUserBuildSettings.exportAsGoogleAndroidProject;
            bool previousSigning = PlayerSettings.Android.useCustomKeystore;
            var previousCompression = EditorUserBuildSettings.androidBuildSubtarget;
            try
            {
                EditorUserBuildSettings.buildAppBundle = release;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC2;
                if (!release) PlayerSettings.Android.useCustomKeystore = false;
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                    target = BuildTarget.Android,
                    locationPathName = output,
                    options = release ? BuildOptions.None : BuildOptions.Development
                });
                if (report.summary.result != BuildResult.Succeeded)
                    throw new BuildFailedException($"Android build failed: {report.summary.result} ({report.summary.totalErrors} errors).");
                Debug.Log($"Android build saved to {Path.GetFullPath(output)}");
            }
            finally
            {
                EditorUserBuildSettings.buildAppBundle = previousBundle;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = previousExport;
                EditorUserBuildSettings.androidBuildSubtarget = previousCompression;
                PlayerSettings.Android.useCustomKeystore = previousSigning;
            }
        }
    }
}
