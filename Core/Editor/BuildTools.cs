using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using System.Linq;
using System.IO;
using Castrimaris.Core.Monitoring;
using System;

namespace Castrimaris.Core.Editor {

    /// <summary>
    /// Miscellaneous build tools
    /// </summary>
    public class BuildTools {

        [Flags] //TODO simplify building process
        private enum Platforms { QUEST=1<<10, ANDROID=1<<11, LINUX_SERVER=1<<12 }

        #region PRIVATE VARIABLES

        public const string baseFileName = "Castrimaris_Metaverse_";
        public const string standaloneFolderName = "LinuxStandalone";
        public const string mobileAppendName = "Mobile.apk";
        public const string questAppendName = "Quest.apk";
        public const string linuxServerAppendName = "Server";

        #endregion

        #region PUBLIC METHODS

        [MenuItem("Tools/Build/_All Builds are Development with Script Debugging!_")]
        public static void Warning() => Log.D($"That button was just a warning/comment. Pressing it does nothing.");

        [MenuItem("Tools/Build/Build for Android")]
        public static void BuildAndroid() {
            string path = EditorUtility.SaveFolderPanel("Choose APKs location", "", baseFileName);
            if (string.IsNullOrEmpty(path)) return;
            MakeAndroidBuild(path, BuildOptions.Development | BuildOptions.AllowDebugging);
        }

        [MenuItem("Tools/Build/Build for Quest")]
        public static void BuildQuest() {
            string path = EditorUtility.SaveFolderPanel("Choose APKs location", "", baseFileName);
            if (string.IsNullOrEmpty(path)) return;
            MakeOculusBuild(path, BuildOptions.Development | BuildOptions.AllowDebugging);
        }

        [MenuItem("Tools/Build/Build for Android and Quest")]
        public static void BuildForAndroidPhoneAndQuest() {
            string path = EditorUtility.SaveFolderPanel("Choose APKs location", "", baseFileName);

            if (string.IsNullOrEmpty(path))
                return;

            VersioningEditor.SkipBuild = true;
            VersioningEditor.UpdateBuildNumber();
            MakeOculusBuild(path, BuildOptions.Development | BuildOptions.AllowDebugging);
            MakeAndroidBuild(path, BuildOptions.Development | BuildOptions.AllowDebugging);
            VersioningEditor.SkipBuild = false;
        }

        [MenuItem("Tools/Build/Build for Quest and Linux Server")]
        public static void BuildQuestAndLinuxServer() {
            string path = EditorUtility.SaveFolderPanel("Choose builds save location.", Directory.GetCurrentDirectory(), "");

            if (string.IsNullOrEmpty(path))
                return;

            VersioningEditor.SkipBuild = true;
            VersioningEditor.UpdateBuildNumber();
            MakeOculusBuild(path, BuildOptions.Development | BuildOptions.AllowDebugging);
            MakeLinuxServerBuild(path, BuildOptions.Development | BuildOptions.AllowDebugging);
            VersioningEditor.SkipBuild = false;
        }

        [MenuItem("Tools/Build/Build for Android, Quest and Linux Server")]
        public static void BuildAndroidQuestAndLinuxServer() {
            string path = EditorUtility.SaveFolderPanel("Choose builds save location.",Directory.GetCurrentDirectory(), "");

            if (string.IsNullOrEmpty(path))
                return;

            VersioningEditor.SkipBuild = true;
            VersioningEditor.UpdateBuildNumber();
            MakeOculusBuild(path, BuildOptions.Development | BuildOptions.AllowDebugging);
            MakeAndroidBuild(path, BuildOptions.Development | BuildOptions.AllowDebugging);
            MakeLinuxServerBuild(path, BuildOptions.Development | BuildOptions.AllowDebugging);
            VersioningEditor.SkipBuild = false;
        }

        [MenuItem("Tools/Build/Clean Build for Android")]
        public static void CleanBuildAndroid() {
            string path = EditorUtility.SaveFolderPanel("Choose APKs location", "", baseFileName);
            if (string.IsNullOrEmpty(path)) return;
            MakeAndroidBuild(path, BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.CleanBuildCache);
        }

        [MenuItem("Tools/Build/Clean Build for Quest")]
        public static void CleanBuildQuest() {
            string path = EditorUtility.SaveFolderPanel("Choose APKs location", "", baseFileName);
            if (string.IsNullOrEmpty(path)) return;
            MakeOculusBuild(path, BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.CleanBuildCache);
        }

        [MenuItem("Tools/Build/Clean Build for Android and Quest")]
        public static void CleanBuildForAndroidPhoneAndQuest() {
            string path = EditorUtility.SaveFolderPanel("Choose APKs location", "", baseFileName);

            if (string.IsNullOrEmpty(path))
                return;

            VersioningEditor.SkipBuild = true;
            VersioningEditor.UpdateBuildNumber();
            MakeOculusBuild(path, BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.CleanBuildCache);
            MakeAndroidBuild(path, BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.CleanBuildCache);
            VersioningEditor.SkipBuild = false;
        }

        [MenuItem("Tools/Build/Clean Build for Android, Quest and Linux Server")]
        public static void CleanBuildAndroidQuestAndLinuxServer() {
            string path = EditorUtility.SaveFolderPanel("Choose builds save location.", Directory.GetCurrentDirectory(), "");

            if (string.IsNullOrEmpty(path))
                return;

            VersioningEditor.SkipBuild = true;
            VersioningEditor.UpdateBuildNumber();
            MakeOculusBuild(path, BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.CleanBuildCache);
            MakeAndroidBuild(path, BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.CleanBuildCache);
            MakeLinuxServerBuild(path, BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.CleanBuildCache);
            VersioningEditor.SkipBuild = false;
        }

        [MenuItem("Tools/Switch Build Target/Linux")]
        public static void SwitchToLinux() {
            EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
            CheckBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
        }

        [MenuItem("Tools/Switch Build Target/Android")]
        public static void SwitchToAndroid() {
            EnableOculusPlugin(false);
            CheckBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }

        [MenuItem("Tools/Switch Build Target/Oculus")]
        public static void SwitchToOculus() {
            EnableOculusPlugin(true);
            CheckBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }
        #endregion

        #region PRIVATE METHODS

        public static string[] FindEnabledScenesPaths() {
            var selectedScenesPaths = (from scene in EditorBuildSettings.scenes
                                       where scene.enabled
                                       select scene.path).ToArray();
            return selectedScenesPaths;
        }

        public static void CheckBuildTarget(NamedBuildTarget namedBuildTarget, BuildTarget buildTarget) {
            if (EditorUserBuildSettings.activeBuildTarget != buildTarget) {
                var result = EditorUserBuildSettings.SwitchActiveBuildTarget(namedBuildTarget, buildTarget);
                if (!result) {
                    Log.E($"Error while trying to switch active build target!");
                }
            }
        }

        public static void CheckBuildTarget(BuildTargetGroup buildTargetGroup, BuildTarget buildTarget) {
            if (EditorUserBuildSettings.activeBuildTarget != buildTarget) {
                var result = EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
                if (!result) {
                    Log.E($"Error while trying to switch active build target!");
                }
            }
        }

        public static bool EnableOculusPlugin(bool IsEnabled) {
            bool result = false;
            if (IsEnabled) {
                Log.D("Enabling Oculus plugin...");
                var buildTargetSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
                var pluginSettings = buildTargetSettings.AssignedSettings;
                result = XRPackageMetadataStore.AssignLoader(pluginSettings, "Unity.XR.Oculus.OculusLoader", BuildTargetGroup.Android);
            } else {
                Log.D("Disabling Oculus plugin...");
                var buildTargetSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
                var pluginSettings = buildTargetSettings.AssignedSettings;
                result = XRPackageMetadataStore.RemoveLoader(pluginSettings, "Unity.XR.Oculus.OculusLoader", BuildTargetGroup.Android);
            }
            return result;
        }

        public static void MakeOculusBuild(string Path, BuildOptions Options = BuildOptions.None) {
            CheckBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            Log.D("Setting up Oculus build...");
            EnableOculusPlugin(true);

            var locationPathName = System.IO.Path.Combine(Path, baseFileName + questAppendName);

            Log.D("Building android oculus application to {locationPathName}");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions() {
                targetGroup = BuildTargetGroup.Android,
                target = BuildTarget.Android,
                locationPathName = locationPathName,
                scenes = FindEnabledScenesPaths()
            });
        }

        public static void MakeAndroidBuild(string Path, BuildOptions Options = BuildOptions.None) {
            CheckBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            Log.D("Setting up Android build...");
            EnableOculusPlugin(false);

            var locationPathName = System.IO.Path.Combine(Path, baseFileName + mobileAppendName);

            Log.D("Building android mobile application to {locationPathName}");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions() {
                targetGroup = BuildTargetGroup.Android,
                target = BuildTarget.Android,
                locationPathName = locationPathName,
                scenes = FindEnabledScenesPaths(),
                options = Options
            });
        }

        public static void MakeLinuxServerBuild(string Path, BuildOptions Options = BuildOptions.None, bool ShouldEnableOculusPlugin = true) {
            EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
            CheckBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);

            Log.D("Setting up Linux Server build...");
            EnableOculusPlugin(ShouldEnableOculusPlugin);

            var locationPathName = System.IO.Path.Combine(Path, standaloneFolderName, baseFileName + linuxServerAppendName);

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions() {
                subtarget = (int)StandaloneBuildSubtarget.Server,
                targetGroup = BuildTargetGroup.Standalone,
                target = BuildTarget.StandaloneLinux64,
                locationPathName = locationPathName,
                scenes = FindEnabledScenesPaths(),
                options = Options
            });
        }

        #endregion


    }

}