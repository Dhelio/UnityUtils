using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;
using Castrimaris.ScriptableObjects;

namespace Castrimaris.DynamicBakedLightmaps.Editor {


    public class PlayTools {

        [MenuItem("Tools/Play/Play From Starting Scene (HOST)")]
        public static void PlayFromStartingSceneAsHost() {
            SetupInitializationNetworkMode(InitializationParameters.NetworkModes.HOST);
            LoadAndPlayFirstScene();
        }

        [MenuItem("Tools/Play/Play From Starting Scene (LOCAL SERVER)")]
        public static void PlayFromStartingSceneAsLocalServer() {
            SetupInitializationNetworkMode(InitializationParameters.NetworkModes.SERVER_LOCAL);
            LoadAndPlayFirstScene();
        }

        private static void SetupInitializationNetworkMode(InitializationParameters.NetworkModes NetworkMode) {
            //Get ScriptableObject for Initialization Parameters
            var initializationParametersGUID = AssetDatabase.FindAssets($"t:{nameof(InitializationParameters)}").First();
            var initializationParametersAssetPath = AssetDatabase.GUIDToAssetPath(initializationParametersGUID);
            var initializationParameters = AssetDatabase.LoadAssetAtPath(initializationParametersAssetPath, typeof(InitializationParameters)) as InitializationParameters;

            //Force chosen Network Mode
            initializationParameters.ForceNetworkMode(NetworkMode);
        }

        private static void LoadAndPlayFirstScene() {
            //Get current scene and save it.
            var activeScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(activeScene);

            //Get first scene in build settings and play it
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes.First(scene => scene.enabled).path);
            EditorApplication.EnterPlaymode();
        }

    }


}

