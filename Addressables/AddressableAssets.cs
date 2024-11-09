using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UAddressables = UnityEngine.AddressableAssets.Addressables;
using System;
using UnityEditor;

namespace Castrimaris.Addressables {

    /// <summary>
    /// A support class for loading assets from Addressables at runtime.
    /// Automatically handles all operations relatively to <see cref="AsyncOperationHandle"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AddressableAssets<T> : IDisposable where T : UnityEngine.Object {

        public IList<T> Resources { get; private set; } = null;
        public AsyncOperationHandle<IList<T>> Handle { get; private set; }

        public AddressableAssets() {

        }

        public AddressableAssets(AsyncOperationHandle<IList<T>> Handle) {
            this.Handle = Handle;
        }

        public async Task LoadAssets(MergeModes MergeMode, params string[] Labels) {
            var locationsHandle = await LoadAssetsLocations(MergeMode, Labels.ToList());

            //Loading Addressables from editor doesn't work, so we load them from AssetDatabase instead
#if UNITY_EDITOR
            var result = await LoadAssetsFromAssetDatabase(locationsHandle.Result);
            Resources = result;
#else

            Handle = await LoadAssets(locationsHandle.Result);
            Resources = Handle.Result;
#endif
            UAddressables.Release(locationsHandle);
        }

        public async Task LoadAssets(MergeModes MergeMode, IList<string> Labels) {
            await LoadAssets(MergeMode, Labels.ToArray());
        }

        public void Release() {
            if (Handle.IsValid())
                UAddressables.Release(Handle);
        }

        private async Task<AsyncOperationHandle<IList<IResourceLocation>>> LoadAssetsLocations(MergeModes MergeMode, IList<string> labels) {
            var mergeMode = ConvertMergeMode(MergeMode);
            var handle = UAddressables.LoadResourceLocationsAsync(labels, mergeMode, typeof(T));
            await handle.Task;
            return handle;
        }

        private async Task<AsyncOperationHandle<IList<T>>> LoadAssets(IList<IResourceLocation> Locations) {
            var Handle = UAddressables.LoadAssetsAsync<T>(Locations, null);
            await Handle.Task;
            return Handle;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Utility method to load assets from AssetDatabase when using this component in Editor.
        /// </summary>
        /// <param name="Locations"></param>
        /// <returns></returns>
        private async Task<IList<T>> LoadAssetsFromAssetDatabase(IList<IResourceLocation> Locations) {
            var result = new List<T>();
            foreach (var location in Locations)
            {
                result.Add(AssetDatabase.LoadAssetAtPath<T>(location.ToString()));
            }
            return await Task.FromResult(result);
        }
#endif

        private UAddressables.MergeMode ConvertMergeMode(MergeModes MergeMode) {
            switch (MergeMode) {
                case MergeModes.Union: return UAddressables.MergeMode.Union;
                case MergeModes.Intersection: return UAddressables.MergeMode.Intersection;
                default: return UAddressables.MergeMode.None;
            }
        }

        public void Dispose() {
            Release();
        }
    }

}