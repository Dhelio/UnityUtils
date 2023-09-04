using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CastrimarisStudios.Editor {

    /// <summary>
    /// Useful class for auto-version build numbers.
    /// </summary>
    public class VersioningEditor : IPreprocessBuildWithReport {

        #region PROPERTIES
        public int callbackOrder { get { return 0; } } //Should never be called
        public static bool SkipBuild { get; set; }
        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Updates build number by assigning year,month and build count (e.g. for the third build made in September 2023 it would be "2023.09.003")
        /// </summary>
        public static void UpdateBuildNumber() {
            string[] versionSplit = PlayerSettings.bundleVersion.Split(".");
            int year = int.Parse(versionSplit[0]);
            int month = int.Parse(versionSplit[1]);
            int build = int.Parse(versionSplit[2]);

            build++;

            DateTime time = DateTime.Now;
            string datedVersionNumber = $"{time.Year}.{time.Month}.{build.ToString("000")}";
            PlayerSettings.bundleVersion = datedVersionNumber;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Built version {datedVersionNumber}");
        }

        public void OnPreprocessBuild(BuildReport report) {

            if (SkipBuild) {
                return;
            }

            UpdateBuildNumber();
        }

        #endregion

    }

}
