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

        private const string KEYSTORE_PATH = "multiformatris-release.keystore";
        private const string KEYSTORE_PASSWORD = "multiformatris123";
        private const string KEY_ALIAS = "multiformatris";
        private const string KEY_PASSWORD = "multiformatris123";

        [MenuItem("Build/Build Android APK")]
        public static void BuildAndroid()
        {
            SetAndroidSettings();
            SetAndroidSigning();

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
                Debug.Log($"Android APK build succeeded: {summary.totalSize} bytes");
            else
                Debug.LogError($"Android APK build failed: {summary.result}");
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
            SetAndroidSigning();
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
        }

        private static void SetAndroidSigning()
        {
            string keystoreFullPath = System.IO.Path.Combine(Application.dataPath, "..", KEYSTORE_PATH);

            if (!System.IO.File.Exists(keystoreFullPath))
            {
                Debug.LogError($"Keystore not found at: {keystoreFullPath}");
                Debug.LogError("Generate keystore with: keytool -genkeypair -v -keystore multiformatris-release.keystore -alias multiformatris -keyalg RSA -keysize 2048 -validity 10000");
                return;
            }

            PlayerSettings.Android.keystoreName = keystoreFullPath;
            PlayerSettings.Android.keystorePass = KEYSTORE_PASSWORD;
            PlayerSettings.Android.keyaliasName = KEY_ALIAS;
            PlayerSettings.Android.keyaliasPass = KEY_PASSWORD;

            Debug.Log($"Android signing configured: {KEY_ALIAS}");
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

        [MenuItem("Build/Verify Signing Setup")]
        public static void VerifySigningSetup()
        {
            string keystoreFullPath = System.IO.Path.Combine(Application.dataPath, "..", KEYSTORE_PATH);

            Debug.Log("=== Android Signing Verification ===");
            Debug.Log($"Keystore path: {keystoreFullPath}");
            Debug.Log($"Keystore exists: {System.IO.File.Exists(keystoreFullPath)}");
            Debug.Log($"Key alias: {KEY_ALIAS}");
            Debug.Log($"Application ID: {PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)}");

            if (System.IO.File.Exists(keystoreFullPath))
            {
                Debug.Log("✓ Keystore found - Signing is configured!");
                Debug.Log("✓ You can now build signed APK/AAB for Play Console");
            }
            else
            {
                Debug.LogError("✗ Keystore NOT found!");
                Debug.LogError("Run: keytool -genkeypair -v -keystore multiformatris-release.keystore -alias multiformatris -keyalg RSA -keysize 2048 -validity 10000");
            }
        }
    }
}
#endif
