using UnityEditor;
using UnityEngine;

namespace Castrimaris.ScriptableObjects {

    [CustomEditor(typeof(SceneNames))]
    public class SceneNamesInspector : Editor {

        private void OnEnable() {
            CheckForDuplicateScriptableObjects();
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            var sceneNames = target as SceneNames;

            if (sceneNames.fullNames.Length <= 0 || GUILayout.Button("Get Active Scenes Names")) {
                sceneNames.RetrieveSceneNames();
                EditorUtility.SetDirty(target);
            }

        }

        private void CheckForDuplicateScriptableObjects() {
            var sceneNames = AssetDatabase.FindAssets($"t:{nameof(SceneNames)}");
            if (sceneNames.Length >= 2) {
                EditorUtility.DisplayDialog(
                    title: $"Duplicate {nameof(SceneNames)} ScriptableObject error",
                    message: $"More than one ScriptableObject of type {nameof(SceneNames)} has been found. Please, ensure that there is always just one. You can search it by typing 't:{nameof(SceneNames)}' in the Project Panel search bar.",
                    ok: "Understood.");
            }
        }
    }

}
