using Castrimaris.Addressables;
using Castrimaris.Attributes;
using Castrimaris.Core;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Castrimaris.Player.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Castrimaris.Player {

    /// <summary>
    /// Single-player version of the customization system.
    /// </summary>
    public class PlayerAppearance : MonoBehaviour, IPlayerAppearance {

        [Header("References")]
        [SerializeField] private List<PlayerAppearanceData> data;

        private void Reset() {
            RetrieveReferences();
        }

        private void Awake() {
            if (data == null)
                throw new System.NullReferenceException($"No reference found for {nameof(PlayerAppearanceData)}! Add it in the Editor.");
        }

        private async void Start() {
            await AddressablesUtilities.InitializeAsync();
        }

        [ExposeInInspector]
        public async void Load() {
            while (!AddressablesUtilities.IsInitialized)
                await Task.Delay(50); //TODO ugly, remove

            await Task.Delay(1500); //HACK apparently Addressables doesn't initialize right away when it says it is initialized, so we wait a bit before loading everything.

            foreach (var appearance in data) {
                //Load value from player prefs
                var value = PlayerPrefs.GetString(appearance.CategoryName, appearance.Value);
                Set(appearance.Category, value);
            }
        }

        [ExposeInInspector]
        public void Save() {
            foreach (var appearance in data) {
                //Save value in player prefs
                PlayerPrefs.SetString(appearance.CategoryName, appearance.Value);
            }
            PlayerPrefs.Save();
        }

        [ExposeInInspector]
        public void Set(AppearanceCategories category, string value) => Set(category.AsString(), value);

        public async void Set(string category, string value) {
            //Sanity check
            if (category.IsNullOrEmpty() || value.IsNullOrEmpty()) {
                Log.W($"Tried to call set with category {category} and value {value}, but one or both are invalid! Did you pass an empty string?");
                return;
            }

            //Get current skinnedmesh in children
            var oldPlayerAppearanceData = (from d in data where d.Category.AsString() == category select d).FirstOrDefault();
            var oldSkinnedMeshRenderer = oldPlayerAppearanceData.GetComponent<SkinnedMeshRenderer>();

            //If the request is null, just null the mesh and the value
            if (value.IsNullOrEmpty()) {
                oldSkinnedMeshRenderer.sharedMesh = null;
                oldPlayerAppearanceData.Value = null;
                return;
            }

            //Load new skinnedmesh
            var categoryAppearances = await AddressablesUtilities.ManagedLoadAsync<GameObject>(MergeModes.Intersection, category);
            var newSkinnedMeshRendererContainer = categoryAppearances.Where(obj => obj.name == value).FirstOrDefault(); //This implies that the Key/Address of the addressable MUST be the same as its name!

            //If the skinned mesh doesn't exist, null data
            if (newSkinnedMeshRendererContainer == null) {
                Log.W($"No skinned mesh for category {category} with value {value} has been found. Nulling skinned mesh data...");
                oldSkinnedMeshRenderer.sharedMesh = null;
                oldPlayerAppearanceData.Value = null;
                return;
            }

            //Otherwise, spawn the new skinned mesh
            newSkinnedMeshRendererContainer = GameObject.Instantiate(newSkinnedMeshRendererContainer);

            //Remap new skinnedmesh on old skinnedmesh
            var newSkinnedMeshRenderer = newSkinnedMeshRendererContainer.GetComponentInChildren<SkinnedMeshRenderer>();
            newSkinnedMeshRenderer.Remap(oldSkinnedMeshRenderer);

            //Reparent and reset position of new skinnedmesh
            var newSkinnedMeshTransform = newSkinnedMeshRenderer.transform;
            newSkinnedMeshTransform.parent = oldSkinnedMeshRenderer.transform.parent;
            newSkinnedMeshTransform.localPosition = Vector3.zero;
            newSkinnedMeshTransform.position = Vector3.zero;
            newSkinnedMeshTransform.localRotation = Quaternion.identity;
            newSkinnedMeshTransform.rotation = Quaternion.identity;

            //Copy layer from old skinned mesh into new skinned mesh
            newSkinnedMeshRenderer.gameObject.layer = oldSkinnedMeshRenderer.gameObject.layer;

            ////Save data of the new skinned mesh
            var newPlayerAppearanceData = newSkinnedMeshRenderer.GetComponent<PlayerAppearanceData>();
            data.Remove(oldPlayerAppearanceData);
            data.Add(newPlayerAppearanceData);

            //Destroy unused objects
            Log.D($"Destroying {oldPlayerAppearanceData.name} and {newSkinnedMeshRendererContainer.name}");
            Destroy(oldPlayerAppearanceData.gameObject);
            Destroy(newSkinnedMeshRendererContainer.gameObject);
        }

        [ContextMenu("Retrieve References")]
        private void RetrieveReferences() => data = GetComponentsInChildren<PlayerAppearanceData>().ToList();

    }
} 