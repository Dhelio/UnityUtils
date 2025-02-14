using UnityEngine;
using UnityEditor;

namespace Castrimaris.Core.Editor.Windows {

    /// <summary>
    /// Utility window to easily rename <see cref="GameObject"/>s
    /// </summary>
    public class RenamingWindow : EditorWindow {

        private string text;
        private int number;

        [MenuItem("Tools/Castrimaris/Windows/Renaming Window")]
        public static void OpenWindow() {
            var window = GetWindow<RenamingWindow>();
            window.titleContent = new GUIContent(nameof(RenamingWindow));
        }

        private void OnGUI() {
            number = EditorGUILayout.IntField("Num", number);
            text = EditorGUILayout.TextField("Text", text);
            if (GUILayout.Button("Remove prefix from selection")) {
                var gameObjects = Selection.gameObjects;
                foreach (var gameObject in gameObjects) {
                    if (gameObject.name.StartsWith(text)) {
                        gameObject.name = gameObject.name.Remove(0, text.Length);
                    }
                }
                Debug.Log($"Selection count: {gameObjects.Length} text: {text}");
            }
            if (GUILayout.Button("Remove last N chars from Selection")) {
                var gameObjects = Selection.gameObjects;
                foreach (var gameObject in gameObjects) {
                    gameObject.name = gameObject.name.Substring(0, gameObject.name.Length - number);
                }
            }
        }

    }

}
