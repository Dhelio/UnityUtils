using UnityEngine;
using UnityEngine.XR.Management;
using NUnit.Framework;
using System.Collections.Generic;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Castrimaris.Core.Utilities {

    public class Utilities {

        private const string QUEST_1_DEVICE_CODENAME = "miramar";
        private const string QUEST_2_DEVICE_CODENAME = "hollywood";
        private const string QUEST_3_DEVICE_CODENAME = "eureka";

        private static bool? isVrPlatform = null; //Cached value to make checks for VR platforms faster

        /// <summary>
        /// Retrieves all interfaces of a certain type. Might work also with common types.
        /// </summary>
        /// <typeparam name="T">The interface</typeparam>
        /// <returns>a list of interfaces of the requested type. If no types are found then the list will be empty.</returns>
        public static List<T> FindInterfacesOfType<T>() where T : class {
            var result = new List<T>();
            var gameObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var gameObject in gameObjects) {
                var components = gameObject.GetComponents<MonoBehaviour>();
                foreach (var component in components) {
                    if (component == null) continue; //Skip missing or invalid components
                    if (component is T) {
                        result.Add(component as T);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the running platform on Android OS. In Editor calls <see cref="GetEditorPlatform"/> instead.
        /// </summary>
        /// <returns></returns>
        public static RuntimePlatformTypes GetAndroidPlatform() {
            if (Application.isEditor)
                return GetEditorPlatform();

            var ajc = new AndroidJavaClass("android.os.Build");
            var deviceCodename = ajc.GetStatic<string>("DEVICE");
            switch (deviceCodename) {
                case QUEST_1_DEVICE_CODENAME:
                    return RuntimePlatformTypes.QUEST1;
                case QUEST_2_DEVICE_CODENAME:
                    return RuntimePlatformTypes.QUEST2;
                case QUEST_3_DEVICE_CODENAME:
                    return RuntimePlatformTypes.QUEST3;
                default:
                    return RuntimePlatformTypes.ANDROID;
            }
        }

        /// <summary>
        /// Custom RuntimePlatform definition. Extends the base <see cref="RuntimePlatform"/> from Unity with more granular devices definition.
        /// </summary>
        /// <returns>The current <see cref="RuntimePlatformTypes"/>. For Android returns also the precise platform if it's a quest device</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static RuntimePlatformTypes GetRuntimePlatform() {
            var runtimePlatform = RuntimePlatformTypes.UNKNOWN;
            switch (Application.platform) {
                case RuntimePlatform.Android:
                    runtimePlatform = GetAndroidPlatform();
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.OSXPlayer:
                    runtimePlatform = RuntimePlatformTypes.PC;
                    break;

                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
#if UNITY_EDITOR
                    return GetEditorPlatform();
#endif
                default:
                    throw new System.NotImplementedException($"Runtime platform is {Application.platform}, but no platform is defined for it!");
            }

            return runtimePlatform;
        }

        public static bool IsVrPlatform() {
            if (isVrPlatform != null)
                return isVrPlatform.Value;

            // Check if the XR General Settings instance is available
            if (XRGeneralSettings.Instance != null) {

                // Get the XR Manager Settings
                var xrManagerSettings = XRGeneralSettings.Instance.Manager;

                // Check if XR Manager Settings is not null and if the XR system is initialized and running
                if (xrManagerSettings != null) {

                    // Check if an XR Loader is currently active
                    isVrPlatform = xrManagerSettings.activeLoaders.Count > 0;
                    return isVrPlatform.Value;

                }
            }

            //TODO check if the absence of an instance of XRGeneralSettings effectively means that the application is not running on a VR platform, so to cache the bool value
            return false;
        }

        /// <summary>
        /// If VR is Enabled, return Quest 2; otherwise if the build target is Android return Android, otherwise return PC
        /// </summary>
        private static RuntimePlatformTypes GetEditorPlatform() {
#if UNITY_EDITOR
            if (IsVrPlatform())
                return RuntimePlatformTypes.QUEST2;
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                return RuntimePlatformTypes.ANDROID;
            else
                return RuntimePlatformTypes.PC;
#endif
            return RuntimePlatformTypes.UNKNOWN;
        }

    }

}