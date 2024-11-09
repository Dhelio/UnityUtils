using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using UnityEngine;

namespace Castrimaris.IO {

    //TODO
    [RequireComponent(typeof(OpenAIApi))]
    public class OpenAIAssistant : MonoBehaviour {

        [Header("Parameters")]
        [SerializeField] private string assistantId = null;


        private OpenAIApi apis;
        private void Awake() {
            if (assistantId.IsNullOrEmpty()) {
                throw new System.NullReferenceException($"Fatal error: {nameof(assistantId)} cannot be null or empty!");
            }

            apis = GetComponent<OpenAIApi>();


        }

    }

}