using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;
using Castrimaris.Core.Extensions;

namespace Castrimaris.Core.Editor {

    /// <summary>
    /// Miscellaneous tools for Play mode
    /// </summary>
    public class PlayTools {

        private static string previousOpenScenePath = "";

        [MenuItem("Tools/Play/Play From Starting Scene")]
        public static void PlayFromStartingScene() {
            var activeScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(activeScene);
            previousOpenScenePath = activeScene.path;
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes.First(scene => scene.enabled).path);
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("Tools/Play/Stop and Return to Previous Scene")]
        public static void StopPlayAndReprise() {
            EditorApplication.ExitPlaymode();
            if (previousOpenScenePath.IsNullOrEmpty())
                return;
            EditorSceneManager.OpenScene(previousOpenScenePath);
        }

    }

}