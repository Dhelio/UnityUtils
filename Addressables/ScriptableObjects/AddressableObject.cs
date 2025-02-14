using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UAddressables = UnityEngine.AddressableAssets.Addressables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Castrimaris.ScriptableObjects {

    /// <summary>
    /// Class representing an object that can retrieve values from addressable assets.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AddressableObject<T> : ScriptableObject, IAddressableObject<T> {

        protected bool hasInitialized = false;
        protected AsyncOperationHandle<T> handle;
        protected IList<IResourceLocation> resourceLocations;

        [SerializeField] private string[] addressableLabels = { "Resource" };

        public string[] Labels {
            get { return addressableLabels; }
            set { addressableLabels = value; }
        }

        public int Count {
            get {
                if (resourceLocations == null)
                    Initialize(); //TODO remove. Count shouldn't launch a potentially expensive method under the hood.
                return resourceLocations.Count;
            }
        }

        public virtual void Initialize() {
            var handle = UAddressables.LoadResourceLocationsAsync(addressableLabels, UAddressables.MergeMode.Intersection, typeof(T));
            handle.WaitForCompletion();
            resourceLocations = handle.Result;
            UAddressables.Release(handle);
            hasInitialized = true;
        }

        public virtual async Task InitializeAsync() {
            var handle = UAddressables.LoadResourceLocationsAsync(addressableLabels, UAddressables.MergeMode.Intersection, typeof(T));
            await handle.Task;
            resourceLocations = handle.Result;
            UAddressables.Release(handle);
            hasInitialized = true;
        }

        public virtual T Get(int Index) {
            if (!hasInitialized) {
                Initialize();
            }

            Index = Mathf.Clamp(Index, 0, resourceLocations.Count);

            handle = UAddressables.LoadAssetAsync<T>(resourceLocations[Index]);
            handle.WaitForCompletion();
            var result = handle.Result;

            return result;
        }

        public virtual async Task<T> GetAsync(int Index) {
            if (!hasInitialized) {
                Initialize();
            }

            Index = Mathf.Clamp(Index, 0, resourceLocations.Count);
            handle = UAddressables.LoadAssetAsync<T>(resourceLocations[Index]);
            await handle.Task;

            var result = handle.Result;
            return result;
        }

        public virtual async Task<List<T>> GetAll() {
            if (!hasInitialized) {
                Initialize();
            }

            List<T> values = new List<T>();

            var opHandle = UAddressables.LoadAssetsAsync<T>(resourceLocations, element => values.Add(element));

            await opHandle.Task;

            UAddressables.Release(opHandle);

            return values;
        }

        public virtual void Dispose() {
            UAddressables.Release(handle);
        }

    }

}