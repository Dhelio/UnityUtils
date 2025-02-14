using Castrimaris.Core;
using Castrimaris.Core.Exceptions;
using Castrimaris.ScriptableObjects;
using OpenAI;
using UnityEngine;

namespace Castrimaris.IO {

    /// <summary>
    /// Singleton for Rage Againt The Pixel's OpenAI APIs wrapper.
    /// </summary>
    [DisallowMultipleComponent]
    [HelpURL("https://github.com/RageAgainstThePixel/com.openai.unity")]
    public partial class OpenAIApi : SingletonMonoBehaviour<OpenAIApi> {

        [Header("References")]
        [SerializeField] private APIKey apiKey;

        private OpenAIClient client;

        public OpenAIClient Client => client;

        protected override void Awake() {
            base.Awake();

            if (apiKey == null) throw new ReferenceMissingException(nameof(apiKey));

            client = new OpenAIClient(apiKey.Key);
        }
    }
}