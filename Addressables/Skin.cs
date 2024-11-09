using Castrimaris.Math;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UAddressables = UnityEngine.AddressableAssets.Addressables;

namespace Castrimaris.ScriptableObjects {

    [CreateAssetMenu(fileName = "Skin", menuName = "Castrimaris/ScriptableObjects/Skin")]
    public class Skin : AddressableObject<Material> {

        #region PRIVATE VARIABLES
        private const string TAG = nameof(Skin);
        #endregion

        #region PUBLIC METHODS

        public override Material Get(int Index) {
            //Sanity Checks
            if (!hasInitialized)
                Initialize();
            Index = FMath.Clamp(Index, 0, resourceLocations.Count);
            //Retrieve skin
            Debug.Log($"[{TAG}] - Loading skin with index {Index}");
            handle = UAddressables.LoadAsset<Material>(resourceLocations[Index]);
            handle.WaitForCompletion();
            var result = handle.Result;
            Debug.Log($"[{TAG}] - Forcing shader reloading by finding {result.shader.name}");
            result.shader = Shader.Find(result.shader.name);
            return result;
        }

        public override Task<Material> GetAsync(int Index) {
            throw new NotImplementedException();
        }

        #endregion

    }

}