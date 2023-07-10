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

    public class BuildTools {

        #region PRIVATE VARIABLES

        private const string TAG = nameof(BuildTools);
        private const string baseFileName = "BuildBaseName";
        private const string standaloneFolderName = "LinuxStandalone";
        private const string mobileAppendName = "Mobile.apk";
        private const string questAppendName = "Quest.apk";
        private const string linuxServerAppendName = "Server";
        private static readonly string[] scenes = { "" }; //TODO automate scene finding 

        #endregion

        #region PRIVATE METHODS

        private static void CheckBuildTarget(NamedBuildTarget namedBuildTarget, BuildTarget buildTarget) {
            if (EditorUserBuildSettings.activeBuildTarget != buildTarget) {
                var result = EditorUserBuildSettings.SwitchActiveBuildTarget(namedBuildTarget, buildTarget);
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
            CheckBuildTarget(NamedBuildTarget.Android, BuildTarget.Android);

            Debug.Log($"[{TAG}] - Setting up Oculus build...");
            EnableOculusPlugin(true);

            var locationPathName = System.IO.Path.Combine(Path, baseFileName + questAppendName);

            Debug.Log($"[{TAG}] - Building android oculus application to {locationPathName}");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions() {
                targetGroup = BuildTargetGroup.Android,
                target = BuildTarget.Android,
                locationPathName = locationPathName,
                scenes = scenes
            });
        }

        private static void MakeAndroidBuild(string Path) {
            CheckBuildTarget(NamedBuildTarget.Android, BuildTarget.Android);

            Debug.Log($"[{TAG}] - Setting up Android build...");
            EnableOculusPlugin(false);

            var locationPathName = System.IO.Path.Combine(Path, baseFileName + mobileAppendName);

            Debug.Log($"[{TAG}] - Building android mobile application to {locationPathName}");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions() {
                targetGroup = BuildTargetGroup.Android,
                target = BuildTarget.Android,
                locationPathName = locationPathName,
                scenes = scenes
            });
        }

        private static void MakeLinuxServerBuild(string Path, bool ShouldEnableOculusPlugin = true) {
            CheckBuildTarget(NamedBuildTarget.Server, BuildTarget.StandaloneLinux64);

            Debug.Log($"[{TAG}] - Setting up Linux Server build...");
            EnableOculusPlugin(ShouldEnableOculusPlugin);

            var locationPathName = System.IO.Path.Combine(Path, standaloneFolderName, baseFileName +linuxServerAppendName);

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions() {
                targetGroup = BuildTargetGroup.LinuxHeadlessSimulation,
                target = BuildTarget.LinuxHeadlessSimulation,
                locationPathName = locationPathName,
                scenes = scenes
            });
        }

        #endregion

        #region PUBLIC METHODS


        [MenuItem("Tools/Build Android and Quest")]
        public static void BuildForAndroidPhoneAndQuest() {
            string path = EditorUtility.SaveFolderPanel("Choose APKs location", "", baseFileName);

            Debug.Log($"[{TAG}] - Saving player builds to {path}...");

            MakeOculusBuild(path);
            MakeAndroidBuild(path);
        }

        [MenuItem("Tools/Build Android, Quest and Linux Server")]
        public static void BuildAndroidAndLinuxStandalone() {
            string path = EditorUtility.SaveFolderPanel("Choose builds save location.",Directory.GetCurrentDirectory(), "");

            Debug.Log($"[{TAG}] - Saving player builds to {path}...");

            MakeOculusBuild(path);
            MakeAndroidBuild(path);
            MakeLinuxServerBuild(path);
        }

        #endregion

    }

}
