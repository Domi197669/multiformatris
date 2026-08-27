#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Multiformatris.Infrastructure.Build
{
    public class BuildManager
    {
        private const string ANDROID_BUILD_PATH = "build/Multiformatris.apk";

        private const string KEYSTORE_PATH = "multiformatris-release.keystore";
        private const string KEYSTORE_PASSWORD = "multiformatris123";
        private const string KEY_ALIAS = "multiformatris";
        private const string KEY_PASSWORD = "multiformatris123";

        public static void BuildAndroid()
        {
            SetAndroidSettings();
            SetAndroidSigning();
            SetAndroidIcon();

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

        [MenuItem("Build/Build Android APK")]
        public static void BuildAndroidMenu()
        {
            BuildAndroid();
        }

        [MenuItem("Build/Build Android AAB (Play Store)")]
        public static void BuildAndroidAAB()
        {
            SetAndroidSettings();
            SetAndroidSigning();
            SetAndroidIcon();
            EditorUserBuildSettings.buildAppBundle = true;

            string[] scenes = GetScenes();
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "build/Multiformatris.aab",
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
#pragma warning disable CS0618
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.multiformatris.game");
#pragma warning restore CS0618
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.bundleVersionCode = 1;

            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;

#pragma warning disable CS0618
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
#pragma warning restore CS0618
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.PreferExternal;

            PlayerSettings.defaultScreenWidth = 1080;
            PlayerSettings.defaultScreenHeight = 1920;
            PlayerSettings.defaultIsNativeResolution = true;

            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = true;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }

        private static void SetAndroidIcon()
        {
            string[] iconPaths = new string[]
            {
                "Assets/Icons/app_icon.png",
                "Assets/Icons/app_icon_192.png"
            };

            foreach (string path in iconPaths)
            {
                Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (icon != null)
                {
                    PlayerSettings.SetIcons(NamedBuildTarget.Android, new Texture2D[] { icon }, IconKind.Any);
                    PlayerSettings.SetIcons(NamedBuildTarget.Android, new Texture2D[] { icon }, IconKind.Application);
                    Debug.Log($"[BuildManager] Android launcher icon set from {path}");
                    return;
                }
            }

            Debug.LogWarning("[BuildManager] No icon found in Assets/Icons/");
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
#pragma warning disable CS0618
            Debug.Log($"Application ID: {PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)}");
#pragma warning restore CS0618

            if (System.IO.File.Exists(keystoreFullPath))
            {
                Debug.Log("Keystore found - Signing is configured!");
                Debug.Log("You can now build signed APK/AAB for Play Console");
            }
            else
            {
                Debug.LogError("Keystore NOT found!");
                Debug.LogError("Run: keytool -genkeypair -v -keystore multiformatris-release.keystore -alias multiformatris -keyalg RSA -keysize 2048 -validity 10000");
            }
        }
    }
}
#endif
