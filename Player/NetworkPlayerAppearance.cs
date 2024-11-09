using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Castrimaris.Addressables;
using Castrimaris.Player.Contracts;
using Castrimaris.Core.Extensions;
using Unity.Collections;
using Castrimaris.Core;
using Castrimaris.Core.Monitoring;
using System;

namespace Castrimaris.Player {

    /// <summary>
    /// Manager class for Player's appearance across the Network.
    /// </summary>
    public class NetworkPlayerAppearance : NetworkBehaviour, IPlayerAppearance {

        /// <summary>
        /// Custom data structure to serialize appearance data on the Network.
        /// </summary>
        [System.Serializable]
        public struct NetworkData : INetworkSerializeByMemcpy, System.IEquatable<NetworkData> {
            public FixedString128Bytes category;
            public FixedString128Bytes value;

            public bool Equals(NetworkData other) => (category.ToString() == other.category.ToString() && value.ToString() == other.value.ToString());

            public NetworkData(string category, string value) {
                this.category = category;
                this.value = value;
            }

            public void SetValue(string value) => this.value = value;
            public void SetValue(FixedString128Bytes value) => this.value = value;
        }

        [Header("References")]
        [SerializeField] private List<PlayerAppearanceData> data;
        [SerializeField] private NetworkList<NetworkData> syncedValues;

        private readonly IPlayerPrefs playerPrefs = new LocalPlayerPrefs();

        public override void OnNetworkSpawn() {
            if (IsOwner) {
                var skinnedMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
                skinnedMeshes.ForEach(skinnedMesh => skinnedMesh.updateWhenOffscreen = true);
            }

            syncedValues.OnListChanged += OnNetworkListChanged;
        }

        private void Reset() {
            RetrieveReferences();
        }

        private void Awake() {
            if (data == null)
                throw new System.NullReferenceException($"No reference found for {nameof(PlayerAppearanceData)}! Add it in the Editor.");
            var tmp = (from datum in data select new NetworkData(datum.CategoryName, datum.Value));
            syncedValues = new NetworkList<NetworkData>(tmp, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        }

        public void Load() {
            if (!IsOwner) {
                LoadFromList();
                return;
            }

            if (syncedValues.Count > 0)
                syncedValues.Clear();

            //Load value from player prefs and populate network list
            foreach (var datum in data) {
                var value = playerPrefs.GetString(datum.CategoryName, datum.Value);
                syncedValues.Add(new NetworkData(datum.CategoryName, value));
            }
        }

        public void Save() {
            if (!IsOwner)
                return;

            foreach (var syncedValue in syncedValues) {
                playerPrefs.SetString(syncedValue.category.ToString(), syncedValue.value.ToString());
            }
            
            playerPrefs.Save();
        }

        public void Set(AppearanceCategories category, string value) => Set(category.AsString(), value);

        public void Set(string category, string value) {
            if (!IsOwner)
                return;

            //Update synced values
            int i;
            for (i = 0; i < syncedValues.Count; i++) {
                if (syncedValues[i].category.ToString() == category) {
                    var networkData = new NetworkData(category, value);
                    syncedValues[i] = networkData;
                    break;
                }
            }
            if (i >= syncedValues.Count) { 
                Log.D($"Failed to find{nameof(NetworkData)} for {category}!");
                return;
            }

            Save();
        }

        private void OnNetworkListChanged(NetworkListEvent<NetworkData> listEvent) {
            var newValue = listEvent.Value;
            InnerSet(newValue.category, newValue.value);
        }

        private void LoadFromList() {
            Log.D($"Trying to load player aspect from synced values. Local synced values length: {syncedValues.Count}");
            foreach (var syncedValue in syncedValues) {
                Log.D($"Synced value category {syncedValue.category.ToString()}, value {syncedValue.value.ToString()}");
                InnerSet(syncedValue.category, syncedValue.value);
            }
        }

        private void InnerSet(FixedString128Bytes category, FixedString128Bytes value) => InnerSet(category.ToString(), value.ToString());

        private async void InnerSet(string category, string value) {
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
        private void RetrieveReferences() {
            data = GetComponentsInChildren<PlayerAppearanceData>().ToList();
        }
    }
}