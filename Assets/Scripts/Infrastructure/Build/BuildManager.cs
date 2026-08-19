#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Multiformatris.Infrastructure.Build
{
    public class BuildManager
    {
        private const string ANDROID_BUILD_PATH = "Builds/Android/Multiformatris.apk";
        private const string IOS_BUILD_PATH = "Builds/iOS";

        [MenuItem("Build/Build Android APK")]
        public static void BuildAndroid()
        {
            SetAndroidSettings();

            string[] scenes = GetScenes();
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = ANDROID_BUILD_PATH,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
                Debug.Log($"Android build succeeded: {summary.totalSize} bytes");
            else
                Debug.LogError($"Android build failed: {summary.result}");
        }

        [MenuItem("Build/Build iOS")]
        public static void BuildIOS()
        {
            SetIOSSettings();

            string[] scenes = GetScenes();
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = IOS_BUILD_PATH,
                target = BuildTarget.iOS,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
                Debug.Log($"iOS build succeeded: {summary.totalSize} bytes");
            else
                Debug.LogError($"iOS build failed: {summary.result}");
        }

        [MenuItem("Build/Build Android AAB (Play Console)")]
        public static void BuildAndroidAAB()
        {
            SetAndroidSettings();
            EditorUserBuildSettings.buildAppBundle = true;

            string[] scenes = GetScenes();
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Builds/Android/Multiformatris.aab",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            EditorUserBuildSettings.buildAppBundle = false;

            if (summary.result == BuildResult.Succeeded)
                Debug.Log($"Android AAB build succeeded: {summary.totalSize} bytes");
            else
                Debug.LogError($"Android AAB build failed: {summary.result}");
        }

        private static void SetAndroidSettings()
        {
            PlayerSettings.companyName = "Multiformatris";
            PlayerSettings.productName = "Multiformatris";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.multiformatris.game");
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.bundleVersionCode = 1;

            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingBackend.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.PreferExternal;

            PlayerSettings.defaultScreenWidth = 1080;
            PlayerSettings.defaultScreenHeight = 1920;
            PlayerSettings.defaultIsNativeResolution = false;

            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = true;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            PlayerSettings.SetUseDefaultApplicationIdentifier(BuildTargetGroup.Android, false);
            PlayerSettings.Android.keystoreName = "";
            PlayerSettings.Android.keyaliasName = "";
        }

        private static void SetIOSSettings()
        {
            PlayerSettings.companyName = "Multiformatris";
            PlayerSettings.productName = "Multiformatris";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.multiformatris.game");
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.iOS.buildNumber = "1";

            PlayerSettings.iOS.targetOSVersionString = "14.0";
            PlayerSettings.iOS.appleDeveloperTeamID = "";

            PlayerSettings.defaultScreenWidth = 1080;
            PlayerSettings.defaultScreenHeight = 1920;
            PlayerSettings.defaultIsNativeResolution = false;

            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = true;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }

        private static string[] GetScenes()
        {
            return new string[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Game.unity"
            };
        }

        [MenuItem("Build/Open Build Folder")]
        public static void OpenBuildFolder()
        {
            EditorUtility.RevealInFinder("Builds");
        }
    }
}
#endif
