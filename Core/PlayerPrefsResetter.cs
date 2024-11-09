using Castrimaris.Attributes;
using Castrimaris.Core.Monitoring;
using UnityEngine;

namespace Castrimaris.Core {

    public class PlayerPrefsResetter : MonoBehaviour {

        [Header("Parameters")]
        [Tooltip("The key to look for; if not found, the PlayerPrefs will be erased.")]
        [SerializeField] private string key = "Resetted";
        [SerializeField] private InitializationTypes initializationType = InitializationTypes.OnAwake;

        private void Awake() {
            if (initializationType != InitializationTypes.OnAwake)
                return;
            Initialize();
        }

        private void Start() {
            if (initializationType != InitializationTypes.OnStart)
                return;
            Initialize();
        }

        public void Initialize() {
            if (PlayerPrefs.GetString(key, null) == null) {
                Log.W($"No key '{key}' found! Resetting...");
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                PlayerPrefs.SetString(key, key);
            }
        }
    }
}
