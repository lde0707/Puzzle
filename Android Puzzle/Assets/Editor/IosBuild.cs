using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Puzzle.Editor
{
    public static class IosBuild
    {
        [MenuItem("Puzzle/iOS/Switch to iOS")]
        public static void SwitchToIos()
        {
            RequireIosSupport();
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS))
                throw new BuildFailedException("Could not switch to iOS.");
        }

        [MenuItem("Puzzle/iOS/Export Development Xcode Project")]
        public static void ExportDevelopmentXcode() => Export(true);

        [MenuItem("Puzzle/iOS/Export Release Xcode Project")]
        public static void ExportReleaseXcode() => Export(false);

        private static void RequireIosSupport()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
                throw new BuildFailedException("Install iOS Build Support for this editor version using Unity Hub.");
        }

        private static void Export(bool development)
        {
            RequireIosSupport();
            // Platform switching reloads scripts, so it is a separate menu action.
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
                throw new BuildFailedException("Run Puzzle > iOS > Switch to iOS first. In batch mode, pass -buildTarget iOS.");
            IosProjectSetup.ValidateProject();
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            // Preserve previous exports, including manual Xcode changes.
            string kind = development ? "Development" : "Release";
            string output = Path.GetFullPath(Path.Combine("Builds", "iOS", kind, DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")));
            if (Directory.Exists(output))
                throw new BuildFailedException($"Export directory already exists: {output}");
            Directory.CreateDirectory(output);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                target = BuildTarget.iOS,
                locationPathName = output,
                options = development ? BuildOptions.Development : BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"iOS export failed: {report.summary.result} ({report.summary.totalErrors} errors).");
            Debug.Log($"Xcode project exported to {output}. Open it on a Mac, choose your signing team, then build or archive in Xcode.");
        }
    }
}
