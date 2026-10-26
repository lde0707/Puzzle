using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

namespace Puzzle.Editor
{
    public static class IosProjectSetup
    {
        [MenuItem("Puzzle/iOS/Apply Base Settings")]
        public static void ApplyBaseSettings()
        {
            // iOS-only setup: preserve Android settings, shared branding and scenes.
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.puzzlestudio.androidpuzzle");
            PlayerSettings.iOS.buildNumber = "1";
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneOnly;
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.SetArchitecture(NamedBuildTarget.iOS, 1); // ARM64
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.iOS, ManagedStrippingLevel.Minimal);
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.iOS, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { GraphicsDeviceType.Metal });
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            // Keep any existing real Team ID; the user can also select their team in Xcode.

            var qualityAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset")[0];
            var quality = new SerializedObject(qualityAsset);
            var defaults = quality.FindProperty("m_PerPlatformDefaultQuality");
            bool found = false;
            for (int i = 0; i < defaults.arraySize; i++)
            {
                var entry = defaults.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("first").stringValue != "iPhone") continue;
                entry.FindPropertyRelative("second").intValue = 1; // Existing Low / Mobile2D tier.
                found = true;
            }
            if (!found) throw new InvalidOperationException("iPhone quality default not found.");
            quality.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            ValidateProject();
        }

        [MenuItem("Puzzle/iOS/Validate Project")]
        public static void ValidateProject()
        {
            if (PlayerSettings.GetScriptingBackend(NamedBuildTarget.iOS) != ScriptingImplementation.IL2CPP ||
                PlayerSettings.GetArchitecture(NamedBuildTarget.iOS) != 1)
                throw new BuildFailedException("Expected IL2CPP and ARM64 for iOS device builds.");
            if (PlayerSettings.iOS.sdkVersion != iOSSdkVersion.DeviceSDK)
                throw new BuildFailedException("Select Device SDK for the iPhone export menus.");
            if (string.IsNullOrWhiteSpace(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS)))
                throw new BuildFailedException("Set an iOS Bundle Identifier in Player Settings.");
            if (!PlayerSettings.GetGraphicsAPIs(BuildTarget.iOS).Contains(GraphicsDeviceType.Metal))
                throw new BuildFailedException("Metal must be enabled for iOS.");
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).ToArray();
            if (scenes.Length == 0 || scenes.Any(s => !File.Exists(s.path)))
                throw new BuildFailedException("Add an existing enabled scene to the build scene list.");
            Debug.Log("iOS project settings validation passed (IL2CPP, ARM64, Metal, device SDK).");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
                Debug.LogWarning("Install iOS Build Support for this editor version using Unity Hub.");
            if (string.IsNullOrWhiteSpace(PlayerSettings.iOS.appleDeveloperTeamID))
                Debug.LogWarning("Apple Team ID is not configured. Select your signing team in Xcode before device installation or distribution.");
        }
    }
}
