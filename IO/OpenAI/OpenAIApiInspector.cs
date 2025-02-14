#if UNITY_EDITOR && OPENAI
using UnityEditor;
using Castrimaris.ScriptableObjects;
using System.Linq;
using System.Collections.Generic;
using Castrimaris.Core.Monitoring;
using Castrimaris.Attributes;
using OpenAI;

namespace Castrimaris.IO {
    public partial class OpenAIApi {
        private void Reset() {
            var apiKeyGuids = AssetDatabase.FindAssets($"t:{nameof(APIKey)}");
            List<APIKey> keys = new List<APIKey>();
            foreach (var guid in apiKeyGuids) {
                var apiKey = AssetDatabase.LoadAssetAtPath<APIKey>(AssetDatabase.GUIDToAssetPath(guid));
                if (apiKey.KeyType == APIKey.KeyTypes.OPENAI)
                    keys.Add(apiKey);
            }
            if (keys.Count > 1) {
                Log.W($"Multiple {nameof(APIKey)} found of type {APIKey.KeyTypes.OPENAI}, using first one.");
            } else if (keys.Count < 0) {
                Log.W($"No {nameof(APIKey)} found of type {APIKey.KeyTypes.OPENAI}. Please, create one.");
                this.apiKey = null;
                return;
            }
            this.apiKey = keys.First();
        }

        [ExposeInInspector]
        private async void ListModels() {
            var client = new OpenAIClient(apiKey.Key);
            var models = await client.ModelsEndpoint.GetModelsAsync();
            int i = 0;
            foreach (var model in models) {
                i++;
                Log.D($"Model {i} id: {model.Id}");
            }
        }
    }
}
#endif