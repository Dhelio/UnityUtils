using Castrimaris.Core.Extensions;
using Castrimaris.ScriptableObjects;
using UnityEditor;
using System;
using CastrimarisLayout = Castrimaris.Core.Editor.Layout;

namespace Castrimaris.Management {

    /// <summary>
    /// Custom inspector for the <see cref="InitializationParameters"/> scriptable object
    /// </summary>
    [CustomEditor(typeof(InitializationParameters))]
    public class InitializationParametersInspector : Editor {

        #region Private Variables

        private const string clientSceneFieldName = "clientTargetSceneName";
        private const string serverSceneFieldName = "serverTargetSceneName";

        private InitializationParameters targetReference;
        private string sceneNamesPath = string.Empty;
        private int serverSceneIndex = -1;
        private int clientSceneIndex = -1;
        private SceneNames sceneNames = null;

        #endregion

        #region Unity Overrides

        private void OnEnable() {
            CheckForDuplicateScriptableObject();

            targetReference = target as InitializationParameters;

            var sceneNames = AssetDatabase.FindAssets($"t:{nameof(SceneNames)}");
            sceneNamesPath = AssetDatabase.GUIDToAssetPath(sceneNames[0]);
            this.sceneNames = AssetDatabase.LoadAssetAtPath(sceneNamesPath, typeof(SceneNames)) as SceneNames;
            var clientSceneName = targetReference.GetFieldValue<string>(clientSceneFieldName);
            var serverSceneName = targetReference.GetFieldValue<string>(serverSceneFieldName);

            clientSceneIndex = Array.FindIndex(this.sceneNames.shortNames, shortName => shortName == clientSceneName);
            serverSceneIndex = Array.FindIndex(this.sceneNames.shortNames, shortName => shortName == serverSceneName);
        }

        public override void OnInspectorGUI() {

            clientSceneIndex = EditorGUILayout.Popup("Client Target Scene", clientSceneIndex, sceneNames.shortNames);
            serverSceneIndex = EditorGUILayout.Popup("Server Target Scene", serverSceneIndex, sceneNames.shortNames);

            if (clientSceneIndex >= 0)
                targetReference.SetFieldValue<string>(clientSceneFieldName, sceneNames.shortNames[clientSceneIndex]);
            if (serverSceneIndex >= 0)
                targetReference.SetFieldValue<string>(serverSceneFieldName, sceneNames.shortNames[serverSceneIndex]);

            base.OnInspectorGUI();

            EditorUtility.SetDirty(target);

            CastrimarisLayout.Space(2);
            CastrimarisLayout.BoldLabelField("Tools");
            CastrimarisLayout.Button("Autodetect Local IP", AutodetectIP);
            CastrimarisLayout.Button("Autoset for Release", Autoset, true);
            CastrimarisLayout.Button("Autoset for Local", Autoset, false);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if there is just one scriptable object of type <see cref="InitializationParameters"/>.
        /// </summary>
        private void CheckForDuplicateScriptableObject() {
            //This check is required because there are some functions that look for the SO in order to set it or get parameters from it; having two or more would make it difficult to understand which is the used one.
            var sceneNames = AssetDatabase.FindAssets($"t:{nameof(InitializationParameters)}");
            if (sceneNames.Length >= 2) {
                EditorUtility.DisplayDialog(
                    title: $"Duplicate {nameof(InitializationParameters)} ScriptableObject error",
                    message: $"More than one ScriptableObject of type {nameof(InitializationParameters)} has been found. Please, ensure that there is always just one. You can search it by typing 't:{nameof(InitializationParameters)}' in the Project Panel search bar.",
                    ok: "Understood.");
            }
        }

        /// <summary>
        /// Retrieves the public IP on the local machine
        /// </summary>
        private async void AutodetectIP() {
            var ip = (await new System.Net.Http.HttpClient().GetStringAsync("http://icanhazip.com")).Replace("\\r\\n", "").Replace("\\n", "").Trim();
            targetReference.SetFieldValue<string>("localServerAddress", ip); //HACK this is kinda hardcoded, if the name changes it breaks.
        }

        /// <summary>
        /// Sets the parameters for release or debug
        /// </summary>
        private void Autoset(bool isRelease) {
            targetReference.SetFieldValue<bool>("forceClientConnection", isRelease ? false : true);
            if (isRelease) {
                targetReference.ForceNetworkMode(InitializationParameters.NetworkModes.SERVER);
            } else {
                targetReference.ForceNetworkMode(InitializationParameters.NetworkModes.SERVER_LOCAL);
            }
        }

        #endregion
    }
}
