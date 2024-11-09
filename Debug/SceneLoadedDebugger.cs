using UnityEngine;
using UnityEngine.SceneManagement;

namespace Castrimaris.Monitoring {

    public class SceneLoadedDebugger : MonoBehaviour {
        private void OnEnable() {
            SceneManager.sceneLoaded += LogSceneLoaded;
        }

        private void OnDisable() {
            SceneManager.sceneLoaded -= LogSceneLoaded;
        }

        private void LogSceneLoaded(Scene scene, LoadSceneMode loadSceneMode) {
            //Debug.Log("Scene loaded: " + scene.name);
        }
    }

}