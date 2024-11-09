using Castrimaris.Attributes;
using Castrimaris.Core;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Castrimaris.IO.Contracts;
using Castrimaris.IO.GoogleDirections;
using Castrimaris.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Castrimaris.IO.GoogleStreetView {

    /// <summary>
    /// Manager class for the StreetView services from Google.
    /// </summary>
    [HelpURL("https://developers.google.com/maps/documentation/streetview/?hl=it")]
    public class StreetView : SingletonMonoBehaviour<StreetView> {

        #region Private Variables
#if UNITY_ANDROID && !UNITY_EDITOR
        private static readonly string CACHE_PATH = "file://data/data/" + Application.identifier.ToString() + "/cache/";
#else
        private static readonly string CACHE_PATH = Application.streamingAssetsPath + "/cache/";
#endif

        private static APIKey apiKey = null;
        #endregion

        #region Static Methods
        /// <summary>
        /// Generic view request / response mechanism
        /// </summary>
        /// <param name="viewRequest">Generic request for the view</param>
        /// <returns>The request view or null</returns>
        /// <exception cref="StreetViewException">Thrown if there are problems downloading the image.</exception>
        public static async Task<Texture2D> RequestView(IViewRequest viewRequest) {
            //Retrieve API Key
            VerifyAPIKey();

            //Prepare WebReq
            var request = await viewRequest.Build(apiKey.Key);
            var response = UnityWebRequestTexture.GetTexture(request);

            //Send WebReq
            response.SendWebRequest();

            //Wait for WebReq to end
            while (!response.isDone) {
                await Task.Delay(100);
            }

            //Check Result
            switch (response.result) {
                case UnityWebRequest.Result.ConnectionError:
                    throw new StreetViewException($"Connection error while trying to connect to server. Error: {response.error}. Request: {request}");
                case UnityWebRequest.Result.ProtocolError:
                    throw new StreetViewException($"Fatal error while trying to download image. Error: {response.error}, Request: {request}");
                case UnityWebRequest.Result.Success:
                    Log.D($"Successfully downloaded image from server. Request: {request}");
                    break;
                default:
                    throw new StreetViewException($"No such result!");
            }

            return DownloadHandlerTexture.GetContent(response);
        }

        /// <summary>
        /// Request a single equirectangular view
        /// </summary>
        /// <param name="viewRequest"></param>
        /// <returns>The request view or null</returns>
        public static async Task<Texture2D> RequestEquirectangularView(EquirectangularViewRequest viewRequest) => await RequestView(viewRequest);

        /// <summary>
        /// Returns a texture representing a full equirectangular view from Google's Web APIs, then caches the resulting image. If the image is cached, then returns that instead.
        /// </summary>
        /// <param name="viewRequest">The request data</param>
        /// <returns>The full texture of the equirectangular view or a texture that says no data found.</returns>
        public static async Task<Texture2D> RequestFullEquirectangularView(EquirectangularViewRequest viewRequest) {
            VerifyAPIKey();

            //Check if the requested view is cached and return that instead, so we avoid hundreds of request for the view.
            if (CheckCacheForPano(viewRequest.PanoId)) {
                Log.D($"Found cached image for Panorama {viewRequest.PanoId}, returning cached image...");
                return await GetCachedPano(viewRequest.PanoId);
            }

            //Some additional view data
            viewRequest.Zoom = 4; //TODO you should be able to choose between multiple zoom types, but X and Y coordinates should be constrained this way
            int xCount = 16;
            int yCount = 8;
            var views = new List<Texture2D>();

            //Download the singular views
            try {
                for (int x = 0; x < xCount; x++) {
                    for (int y = 0; y < yCount; y++) {
                        viewRequest.X = x;
                        viewRequest.Y = y;
                        var result = await RequestEquirectangularView(viewRequest);
                        views.Add(result);
                    }
                }
            } catch (StreetViewException) {
                return null;
            }

            //Build the unified view
            var width = views[0].width;
            var height = views[0].height;
            Log.D($"Equirectangular textures size={width}x{height}, xCount={xCount}, yCount={yCount}, creating texture of size={width * xCount}x{height * yCount}");
            var unifiedViewTexture = new Texture2D(width * xCount, height * yCount);
            for (int x = 0, index = 0; x < xCount; x++) {
                for (int y = yCount - 1; y >= 0; y--, index++) {
                    Log.D($"Applying pixels to {x},{y}, starting coordinates {x * width},{y * height}, size {width},{height}");
                    var pixels = views[index].GetPixels32();
                    unifiedViewTexture.SetPixels32(x * width, y * height, width, height, pixels);
                }
            }

            Log.D($"Full equirectangular image composed. Applying texture...");
            unifiedViewTexture.Apply();

            //Cache the unified view
            Log.D($"Caching texture for future use at path {CACHE_PATH + viewRequest.PanoId}");
            var bytes = unifiedViewTexture.EncodeToJPG();
            File.WriteAllBytes(CACHE_PATH + viewRequest.PanoId, bytes);

            return unifiedViewTexture;
        }

        /// <summary>
        /// Requests a specific Panorama Id from a Metadata request
        /// </summary>
        /// <param name="metadataRequest">The metadata reuqest</param>
        /// <returns>The string of the Panorama Id</returns>
        public static async Task<string> RequestPanoId(MetadataRequest metadataRequest) => (await RequestMetadata(metadataRequest)).pano_id;

        /// <summary>
        /// Requests metadata from Google's APIs
        /// </summary>
        /// <param name="metadataRequest">The metadata request</param>
        /// <returns>The metadata response</returns>
        /// <exception cref="StreetViewException">Thrown if there are problems connecting or downloading from the server</exception>
        public static async Task<MetadataResponse> RequestMetadata(MetadataRequest metadataRequest) {
            VerifyAPIKey();

            //Prepare WebReq
            var request = await metadataRequest.Build(apiKey.Key);
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
                    throw new StreetViewException($"Connection error while trying to connect to server. Error: {response.error}. Request: {request}");
                case UnityWebRequest.Result.ProtocolError:
                    throw new StreetViewException($"Fatal error while trying to download metadata. Error: {response.error}, Request: {request}");
                case UnityWebRequest.Result.Success:
                    Log.D($"Successfully downloaded metadata from server. Request: {request}");
                    break;
                default:
                    throw new StreetViewException($"No such result!");
            }

            //Serialize the downloaded JSON into a class
            var metadataResponse = JsonUtility.FromJson<MetadataResponse>(response.downloadHandler.text);

            return metadataResponse;
        }

        #endregion


        #region Public Methods

        public async Task<Texture2D> RequestStaticView(StaticViewRequest viewRequest) => await RequestView(viewRequest);

        #endregion

        #region Unity Overrides

        protected override void Awake() {
            base.Awake();

            //Create cache directory TODO maybe in Android is different?
            if (!Directory.Exists(CACHE_PATH))
                Directory.CreateDirectory(CACHE_PATH);

            VerifyAPIKey();
        }

        #endregion

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

        /// <summary>
        /// Checks if the requested panorama is cached locally.
        /// </summary>
        /// <param name="PanoId">The panorama id</param>
        /// <returns>True if it's cached, false otherwise</returns>
        private static bool CheckCacheForPano(string PanoId) {
            if (File.Exists(CACHE_PATH + PanoId)) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to retrieve a cached panorama. Strongly suggested to check first with <see cref="CheckCacheForPano(string)"/> before calling this.
        /// </summary>
        /// <param name="PanoId">The panorama id</param>
        /// <returns>The texture of the cached panorama</returns>
        private static async Task<Texture2D> GetCachedPano(string PanoId) {
            //Windows
            //var texture = new Texture2D(2, 2);
            //texture.LoadImage(File.ReadAllBytes(CACHE_PATH + PanoId));
            //texture.Apply();
            //return texture;
            var textureReq = UnityWebRequestTexture.GetTexture(CACHE_PATH + PanoId);
            textureReq.SendWebRequest();
            while (!textureReq.isDone)
                await Task.Delay(70);
            return DownloadHandlerTexture.GetContent(textureReq);
        }
        #endregion

        #region Editor Tools

#if UNITY_EDITOR
        [Header("Editor Tools")]
        [SerializeField] private DirectionsContainer directionsContainer;
        [SerializeField] private string baseFileName = "Direction_";
        [SerializeField] private string baseFolder = Application.dataPath+"Resources/StreetView";

        [ExposeInInspector]
        private async Task SaveToResources() {
            var baseFileName = this.baseFileName;
            if (this.baseFileName.IsNullOrEmpty()) {
                baseFileName = directionsContainer.name + "_";
            }
            int index = 0;
            var steps = directionsContainer.Steps;
            steps.Add(directionsContainer.Steps.Last());
            for (int i = 0; i < steps.Count; i++) {
                var step = steps[i];

                var locationName = step.HtmlInstructions;
                locationName = FormatStepString(locationName, step.TravelMode);

                var location = (i == steps.Count-1) ? step.EndLocation : step.StartLocation; //We add the endlocation
                var request = new EquirectangularViewRequest(location.Latitude, location.Longitude);
                var response = await RequestFullEquirectangularView(request);
                if (response == null) {
                    Log.E($"Fatal error while trying to download image for location {location}. Skipping.");
                    continue;
                }
                var bytes = response.EncodeToPNG();
                var path = $"{baseFolder}/{baseFileName}{index}_step{i}_{locationName}.png";
                Log.D($@"Trying to save view at path: {path}");
                var invalidChars = Path.GetInvalidPathChars();
                foreach(var invalidChar in invalidChars)
                    path.Replace(invalidChar.ToString(), String.Empty);
                if (!Directory.Exists(baseFolder))
                    Directory.CreateDirectory(baseFolder);
                File.WriteAllBytes(path, bytes);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                index++;
            }
            await Task.CompletedTask;
            Log.D($"<color=green>DONE!</color>");
        }

        [ExposeInInspector]
        private async void SaveFromAllResourcesDirectionContainers() {
            var containers = Resources.LoadAll<DirectionsContainer>("");
            var folder = string.Copy(baseFolder);
            foreach (var container in containers) {
                var baseFileName = container.name + "_";
                int index = 0;
                var steps = container.Steps;
                steps.Add(container.Steps.Last());
                for (int i = 0; i < steps.Count; i++) {
                    var step = steps[i];

                    var locationName = step.HtmlInstructions;
                    locationName = FormatStepString(locationName, step.TravelMode);

                    var location = (i == steps.Count - 1) ? step.EndLocation : step.StartLocation; //We add the endlocation
                    var request = new EquirectangularViewRequest(location.Latitude, location.Longitude);
                    var response = await RequestFullEquirectangularView(request);
                    if (response == null) {
                        Log.E($"Fatal error while trying to download image for location {location}. Skipping.");
                        continue;
                    }
                    var bytes = response.EncodeToPNG();
                    var path = $"{folder}/{baseFileName}{index}_step{i}_{locationName}.png";
                    Log.D($@"Trying to save view at path: {path}");
                    var invalidChars = Path.GetInvalidPathChars();
                    foreach (var invalidChar in invalidChars)
                        path.Replace(invalidChar.ToString(), String.Empty);
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                    File.WriteAllBytes(path, bytes);

                    index++;
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

#endif

        private string FormatStepString(string step, string travelMode) {
            var result = step;
            result = result.Replace("\"", "");
            result = result.Replace(@"\ ", "");
            result = result.Replace(@"/ ", "");
            travelMode = travelMode.ToLower();
            switch (travelMode) {
                case "driving":
                    result = result.Split("<b>").Last();
                    result = result.Split("</b>").First();
                    break;
                default:
                    break;
            }

            result = result.Replace("Cammina fino a ", "");
            result = result.Replace("Treno verso ", "");
            result = result.Replace("Autobus verso ", "");
            result = result.Trim();
            result = result.Split(",").First();

            return result;
        }

        #endregion
    }

}
