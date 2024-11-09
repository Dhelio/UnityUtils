using Castrimaris.Attributes;
using Castrimaris.Core;
using Castrimaris.Core.Collections;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Castrimaris.IO.Contracts;
using Castrimaris.IO.GoogleDataStructures;
using Castrimaris.ScriptableObjects;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Castrimaris.IO.GoogleDirections {

    /// <summary>
    /// Manager class for the Directions services from Google
    /// </summary>
    [HelpURL("https://developers.google.com/maps/documentation/directions/?hl=it")]
    public class Directions : SingletonMonoBehaviour<Directions> {

        #region Private Fields

        private static APIKey apiKey;

        #endregion Private Fields

        #region Public Methods

        public static async Task<DirectionsResponse> RequestDirections(IDirectionsRequest directionsRequest) {
            VerifyAPIKey();

            //Prepare WebReq
            var request = await directionsRequest.Build(apiKey.Key);
            var response = UnityWebRequest.Get(request);

            //Send WebReq
            response.SendWebRequest();

            //Wait for WebReq to end
            while (!response.isDone) {
                await Task.Delay(100);
            }

            //Check Result
            switch (response.result) {
                case UnityWebRequest.Result.ConnectionError:
                    throw new DirectionsException($"Connection error while trying to connect to server. Error: {response.error}. Request: {request}");
                case UnityWebRequest.Result.ProtocolError:
                    throw new DirectionsException($"Fatal error while trying to download metadata. Error: {response.error}, Request: {request}");
                case UnityWebRequest.Result.Success:
                    Log.D($"Successfully downloaded directions data from server. Request: {request}");
                    break;

                default:
                    throw new DirectionsException($"No such result!");
            }

            //Serialize the downloaded JSON into a class

            var directionsResponse = JsonConvert.DeserializeObject<DirectionsResponse>(response.downloadHandler.text); //we're using Netwonsoft because we have fields with attributes to define the correct serialization/deserialization process

            return directionsResponse;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Ensures that the APIKey is correctly loaded.
        /// </summary>
        private static void VerifyAPIKey() => apiKey = (apiKey == null) ? LoadAPIKeyFromResources() : apiKey;

        /// <summary>
        /// Loads the first API Key from Google that is found from Resources.
        /// </summary>
        private static APIKey LoadAPIKeyFromResources() {
            //Retrieve API Key
            var apiKeys = Resources.LoadAll<APIKey>("");
            var apiKey = (from apikey in apiKeys
                          where apikey.KeyType == APIKey.KeyTypes.GOOGLE
                          select apikey).First();

            return apiKey;
        }
        #endregion

        #region Editor Tools
#if UNITY_EDITOR

        [Header("Editor Tools")]
        [SerializeField] private Location origin;
        [SerializeField] private Location destination;
        [SerializeField] private TransitModes transitMode;
        [SerializeField] private Languages language;
        [SerializeField] private DateTime dateTime;
        [SerializeField] private SerializableDateTime departureTime;
        [SerializeField] private string scriptableName;
        [SerializeField] private string savePath = "Assets/Resources";

        [ExposeInInspector]
        private async void SaveToScriptable() {
            var request = new DirectionsRequest(
                origin: origin, 
                destination: destination, 
                transitMode: transitMode, 
                language: language,
                departureTime: departureTime.ToDateTime()
                );

            var response = await RequestDirections(request);

            var directions = ScriptableObject.CreateInstance<DirectionsContainer>();
            directions.Data = response;
            var savePath = (scriptableName.IsNullOrEmpty()) ? $"{this.savePath}/Directions.asset" : $"{this.savePath}/{scriptableName}.asset";
            if (!Directory.Exists(this.savePath))
                Directory.CreateDirectory(this.savePath);
            AssetDatabase.CreateAsset(directions, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Log.D($"Successfully saved asset {scriptableName} to {savePath}!");
        }
#endif
        #endregion
    }
}