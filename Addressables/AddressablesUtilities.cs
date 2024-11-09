using Castrimaris.Core.Monitoring;
using System.Collections.Generic;
using System.Threading.Tasks;
using UAddressables = UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Linq;

namespace Castrimaris.Addressables {

    /// <summary>
    /// Utility class containing static method for loading assets from Addressables
    /// </summary>
    public class AddressablesUtilities {

        public static bool IsInitialized = false;
        public static bool IsInitializing = false;

        public static async Task InitializeAsync() {
            if (IsInitialized || IsInitializing)
                return;

            Log.D("Starting initialization...");
            IsInitializing = true;
            var initializationHandle = UAddressables.Addressables.InitializeAsync();
            await initializationHandle.Task;
            IsInitialized = true;
            IsInitializing = false;
            Log.D("Done!");
        }

        #region Asynchronous Methods

        public static async Task Release<T>(T obj) {
            UAddressables.Addressables.Release(obj);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Load an asset asynchronously from <see cref="Addressables"/>.
        /// </summary>
        /// <typeparam name="T">Type of the asset to load</typeparam>
        /// <param name="Key">Key of the asset to load</param>
        /// <returns>The requested object of the requested type, if available, default otherwise</returns>
        public static async Task<T> LoadAsync<T>(string Key) {
            T result;
            var handle = UAddressables.Addressables.LoadAssetAsync<T>(Key);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded) {
                result = handle.Result;
            } else {
                HandleFailure<T>(out result);
            }

            UAddressables.Addressables.Release(handle);

            return result;
        }

        /// <summary>
        /// Loads several assets asynchronously from <see cref="Addressables"/>.
        /// </summary>
        /// <typeparam name="T">Type of the assets to load</typeparam>
        /// <param name="Keys">Keys of the asset to load</param>
        /// <returns>The requested objects of the requested type, if available, empty <see cref="IList{T}"/> otherwise</returns>
        public static async Task<IList<T>> LoadAsync<T>(string[] Keys) {
            var results = new List<T>();
            var handle = UAddressables.Addressables.LoadAssetsAsync<T>(Keys, (result) => results.Add(result));
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded) {
                //Ok
            } else {
                HandleFailure(out results);
            }

            UAddressables.Addressables.Release(handle);

            return results;
        }

        /// <summary>
        /// Loads several assets asynchronously from <see cref="Addressables"/>.
        /// </summary>
        /// <typeparam name="T">Type of the assets to load</typeparam>
        /// <param name="Labels">Labels to look among for the assets.</param>
        /// <param name="MergeMode"><see cref="MergeModes"/> With which to look for assets. <br />
        /// Default is <see cref="MergeModes.Intersection"/>, which looks for assets that have all the indicated labels.<br />
        /// <see cref="MergeModes.UseFirst"/> looks for assets with the first passed Label,<br />
        /// <see cref="MergeModes.Union"/> looks fora assets that have any of the passed Labels,<br />
        /// <see cref="MergeModes.None"/> same as UseFirst</param>
        /// <returns>The requested objects of the requested type, if available, empty <see cref="IList{T}"/> otherwise</returns>
        public static async Task<IList<T>> LoadAsync<T>(string[] Labels, MergeModes MergeMode = MergeModes.Intersection) {
            var locationsHandle = UAddressables.Addressables.LoadResourceLocationsAsync(Labels, (UAddressables.Addressables.MergeMode)MergeMode, typeof(T));
            await locationsHandle.Task;
            var locations = locationsHandle.Result;
            UAddressables.Addressables.Release(locationsHandle);
            return await LoadAsync<T>(locations);
        }

        /// <summary>
        /// Returns the count of how many assets are there for the indicated labels.
        /// </summary>
        /// <param name="mergeMode"><see cref="MergeModes"/> for counting the labels.</param>
        /// <param name="labels">The labels to check</param>
        /// <returns>The count of how many assets are there for the indicated labels.</returns>
        public static async Task<int> GetLoadCountAsync(MergeModes mergeMode = MergeModes.Intersection, params string[] labels) {
            var locationsHandle = UAddressables.Addressables.LoadResourceLocationsAsync(labels, (UAddressables.Addressables.MergeMode)mergeMode);
            await locationsHandle.Task;
            var result = locationsHandle.Result.Count;
            UAddressables.Addressables.Release(locationsHandle);
            return result;
        }

        /// <summary>
        /// Returns a list of all the addresses/keys of the assets for the indicated labels
        /// </summary>
        /// <param name="mergeMode"><see cref="MergeModes"/> for counting the labels.</param>
        /// <param name="labels">The labels to check</param>
        /// <returns>The names (aka keys) of all the assets found with the labels.</returns>
        public static async Task<List<string>> GetNamesAsync(MergeModes mergeMode = MergeModes.Intersection, params string[] labels) {
            var locationsHandle = UAddressables.Addressables.LoadResourceLocationsAsync(labels, (UAddressables.Addressables.MergeMode)mergeMode);
            await locationsHandle.Task;
            var result = new List<string>();
            foreach (var location in locationsHandle.Result) {
                result.Add(location.PrimaryKey);
            }
            return result;
        }

        #endregion

        #region Managed Methods

        /// <summary>
        /// TODO As of right now this method leaves dangling pointers; resolve!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="MergeMode"></param>
        /// <param name="Labels"></param>
        /// <returns></returns>
        public static async Task<IList<T>> ManagedLoadAsync<T>(MergeModes MergeMode = MergeModes.Intersection, params string[] Labels) {
            var locationsHandle = UAddressables.Addressables.LoadResourceLocationsAsync(Labels, (UAddressables.Addressables.MergeMode)MergeMode, typeof(T));
            await locationsHandle.Task;
            var locations = locationsHandle.Result;
            return await ManagedLoadAsync<T>(locations);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles failure while loading Addressable
        /// </summary>
        /// <typeparam name="T">Type of the asset</typeparam>
        /// <param name="Result">The asset itself</param>
        private static void HandleFailure<T>(out T Result) {
            //TODO
            Log.E($"Fatal error while trying to load Addressable!");
            Result = default;
        }

        /// <summary>
        /// Loads several assets asynchronously from <see cref="Addressables"/>.
        /// </summary>
        /// <typeparam name="T">Type of the assets to load</typeparam>
        /// <param name="Locations">An <see cref="IList{T}">/> with the <see cref="IResourceLocation"/> of the assets to load.</param>
        /// <returns>The requested objects of the requested type, if available, empty <see cref="IList{T}"/> otherwise</returns>
        private static async Task<IList<T>> LoadAsync<T>(IList<IResourceLocation> Locations) {
            var results = new List<T>();
            var handle = UAddressables.Addressables.LoadAssetsAsync<T>(Locations, null);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded) {
                results = handle.Result.ToList();
            } else {
                HandleFailure(out results);
            }

            UAddressables.Addressables.Release(handle);

            return results;
        }

        
        private static async Task<IList<T>> ManagedLoadAsync<T>(IList<IResourceLocation> Locations) {
            var results = new List<T>();
            var handle = UAddressables.Addressables.LoadAssetsAsync<T>(Locations, null);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded) {
                results = handle.Result.ToList();
            } else {
                HandleFailure(out results);
            }

            return results;
        }

        #endregion

    }

}