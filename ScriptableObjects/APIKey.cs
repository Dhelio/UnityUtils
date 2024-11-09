using UnityEngine;

namespace Castrimaris.ScriptableObjects {

    /// <summary>
    /// Generic <see cref="ScriptableObject"/> that holds a string key to some kind of service.
    /// </summary>
    [CreateAssetMenu(fileName = "APIKey", menuName = "Castrimaris/ScriptableObjects/API Key")]
    public class APIKey : ScriptableObject {

        /// <summary>
        /// Enumerable with the known list of key types one can expect. Useful when loading automatically a key from a certain source.
        /// </summary>
        public enum KeyTypes {
            UNKNOWN = 0,
            OPENAI,
            AZURE_TTS,
            GOOGLE,
            OPENAI_ASSISTANT
        }

        [Header("Parameters")]
        [SerializeField] private string key;
        [SerializeField] private KeyTypes keyType;

        public string Key => key;
        public KeyTypes KeyType => keyType;
    }

}
