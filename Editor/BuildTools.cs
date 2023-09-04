using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace CastrimarisStudios.Core.Editor {

    /// <summary>
    /// Miscellaneous build tools
    /// </summary>
    public class BuildTools {

        #region PRIVATE VARIABLES

        private const string TAG = nameof(BuildTools);
        private const string baseFileName = "AppBaseName";
        private const string standaloneFolderName = "LinuxStandalone";
        private const string mobileAppendName = "Mobile.apk";
        private const string questAppendName = "Quest.apk";
        private const string linuxServerAppendName = "Server";

        #endregion

        #region PRIVATE METHODS

        private static string[] FindEnabledScenesPaths() {
            var selectedScenesPaths = (from scene in EditorBuildSettings.scenes
                                  where scene.enabled
                                  select scene.path).ToArray();
            return selectedScenesPaths;
        }

        private static void CheckBuildTarget(NamedBuildTarget namedBuildTarget, BuildTarget buildTarget) {
            if (EditorUserBuildSettings.activeBuildTarget != buildTarget) {
                var result = EditorUserBuildSettings.SwitchActiveBuildTarget(namedBuildTarget, buildTarget);
                if (!result) {
                    Debug.LogError($"[{TAG}] - Error while trying to switch active build target!");
                }
            }
        }

        private static void CheckBuildTarget(BuildTargetGroup buildTargetGroup, BuildTarget buildTarget) {
            if (EditorUserBuildSettings.activeBuildTarget != buildTarget) {
                var result = EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
                if (!result) {
                    Debug.LogError($"[{TAG}] - Error while trying to switch active build target!");
                }
            }
        }

        private static bool EnableOculusPlugin(bool IsEnabled) {
            bool result = false;
            if (IsEnabled) {
                Debug.Log($"[{TAG}] - Enabling Oculus plugin...");
                var buildTargetSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
                var pluginSettings = buildTargetSettings.AssignedSettings;
                result = XRPackageMetadataStore.AssignLoader(pluginSettings, "Unity.XR.Oculus.OculusLoader", BuildTargetGroup.Android);
            } else {
                Debug.Log($"[{TAG}] - Disabling Oculus plugin...");
                var buildTargetSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
                var pluginSettings = buildTargetSettings.AssignedSettings;
                result = XRPackageMetadataStore.RemoveLoader(pluginSettings, "Unity.XR.Oculus.OculusLoader", BuildTargetGroup.Android);
            }
            return result;
        }

        private static void MakeOculusBuild(string Path) {
            CheckBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            Debug.Log($"[{TAG}] - Setting up Oculus build...");
            EnableOculusPlugin(true);

            var locationPathName = System.IO.Path.Combine(Path, baseFileName + questAppendName);

            Debug.Log($"[{TAG}] - Building android oculus application to {locationPathName}");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions() {
                targetGroup = BuildTargetGroup.Android,
                target = BuildTarget.Android,
                locationPathName = locationPathName,
                scenes = FindEnabledScenesPaths()
            });
        }

        private static void MakeAndroidBuild(string Path) {
            CheckBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            Debug.Log($"[{TAG}] - Setting up Android build...");
            EnableOculusPlugin(false);

            var locationPathName = System.IO.Path.Combine(Path, baseFileName + mobileAppendName);

            Debug.Log($"[{TAG}] - Building android mobile application to {locationPathName}");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions() {
                targetGroup = BuildTargetGroup.Android,
                target = BuildTarget.Android,
                locationPathName = locationPathName,
                scenes = FindEnabledScenesPaths()
            });
        }

        private static void MakeLinuxServerBuild(string Path, bool ShouldEnableOculusPlugin = true) {
            EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
            CheckBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);

            Debug.Log($"[{TAG}] - Setting up Linux Server build...");
            EnableOculusPlugin(ShouldEnableOculusPlugin);

            var locationPathName = System.IO.Path.Combine(Path, standaloneFolderName, baseFileName +linuxServerAppendName);

    var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions() {
        subtarget = (int)StandaloneBuildSubtarget.Server,
        targetGroup = BuildTargetGroup.Standalone,
        target = BuildTarget.StandaloneLinux64,
        locationPathName = locationPathName,
        scenes = FindEnabledScenesPaths()
            });
        }

        #endregion

        #region PUBLIC METHODS

        [MenuItem("Tools/Build/Build for Android")]
        public static void BuildAndroid() {
            string path = EditorUtility.SaveFolderPanel("Choose APKs location", "", baseFileName);
            if (string.IsNullOrEmpty(path))
                return;
            MakeOculusBuild(path);
        }

        [MenuItem("Tools/Build/Build for Quest")]
        public static void BuildQuest() {
            string path = EditorUtility.SaveFolderPanel("Choose APKs location", "", baseFileName);
            if (string.IsNullOrEmpty(path))
                return;
            MakeOculusBuild(path);
        }

        [MenuItem("Tools/Build/Build for Android and Quest")]
        public static void BuildForAndroidPhoneAndQuest() {
            string path = EditorUtility.SaveFolderPanel("Choose APKs location", "", baseFileName);

            if (string.IsNullOrEmpty(path))
                return;

            VersioningEditor.SkipBuild = true;
            VersioningEditor.UpdateBuildNumber();
            MakeOculusBuild(path);
            MakeAndroidBuild(path);
            VersioningEditor.SkipBuild = false;
        }



        [MenuItem("Tools/Build/Build for Android, Quest and Linux Server")]
        public static void BuildAndroidAndLinuxStandalone() {
            string path = EditorUtility.SaveFolderPanel("Choose builds save location.",Directory.GetCurrentDirectory(), "");

            if (string.IsNullOrEmpty(path))
                return;

            VersioningEditor.SkipBuild = true;
            VersioningEditor.UpdateBuildNumber();
            MakeOculusBuild(path);
            MakeAndroidBuild(path);
            MakeLinuxServerBuild(path);
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

    }

}
