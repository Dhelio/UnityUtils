using Castrimaris.Core;
using Castrimaris.Core.Collections;
using Castrimaris.Core.Monitoring;
using Castrimaris.Core.Utilities;
using Castrimaris.Player;
using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.Management {

    /// <summary>
    /// Avatar selection system. Spawns a different prefab based on the client's executing platform.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(PlatformPlayerPicker))]
    [DisallowMultipleComponent]
    public class NetworkAvatarPicker : NetworkBehaviour {

        private PlatformPlayerPicker platformPlayerPicker;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();

            if (!IsServer)
                return;

            NetworkManager.Singleton.OnClientConnectedCallback += RequestAvatarChoiceClientRpc;

            if (IsHost) {
                //If it's host the OnClientConnectedCallback isn't called (because it's already connected!), so we're spawning the avatar here.
                var localClientId = NetworkManager.Singleton.LocalClientId;
                var runtimePlatform = Utilities.GetRuntimePlatform();
                var runtimePlatfromAsInt = (int)runtimePlatform;
                RequestPlayerSpawnServerRpc(localClientId, runtimePlatfromAsInt);
            }
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            if (!IsServer)
                return;
            NetworkManager.Singleton.OnClientConnectedCallback -= RequestAvatarChoiceClientRpc;
        }

        private void Awake() {
            platformPlayerPicker = GetComponent<PlatformPlayerPicker>();
        }

        [ClientRpc]
        private void RequestAvatarChoiceClientRpc(ulong clientId) {
            //Check if the local client is authorized for requesting a player spawn to the server
            if (NetworkManager.Singleton.LocalClientId != clientId)
                return;

            //Retrieve client RuntimePlatform
            var runtimePlatform = Utilities.GetRuntimePlatform();
            var runtimePlatfromAsInt = (int)runtimePlatform;

            //Request server spawn for clientId with the avatar relative to the runtimePlatform
            RequestPlayerSpawnServerRpc(clientId, runtimePlatfromAsInt);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestPlayerSpawnServerRpc(ulong clientId, int runtimePlatformAsInt) {
            //Conversion for obtaining the runtime platform
            var runtimePlatform = (RuntimePlatformTypes) runtimePlatformAsInt;

            Log.D($"Received request from Client {clientId} for avatar spawning on the platform {runtimePlatform}");

            //Retrieve the prefab
            var playerPrefab = platformPlayerPicker.GetPlayerPrefab(runtimePlatform);

            //Sanity check
            if (playerPrefab == null) {
                throw new System.NullReferenceException($"No reference set for {nameof(playerPrefab)}.");
            }

            //Get client base Network Object
            NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var networkClient);
            var clientBaseNetworkObject = networkClient.PlayerObject;

            Log.D($"Spawning prefab {playerPrefab.name} for client {clientId}");

            //Instantiate player & reparent it
            var player = GameObject.Instantiate(playerPrefab);
            player.name = "Player";
            var playerNGO = player.GetComponent<NetworkObject>();
            playerNGO.SpawnWithOwnership(clientId);
            playerNGO.ChangeOwnership(clientId);
            playerNGO.TrySetParent(clientBaseNetworkObject);

            //InitializeNullNetworkObjectReferences(clientBaseNetworkObject.NetworkObjectId); //Initialize references for the server
            //InitializeNullNetworkObjectReferencesClientRpc(clientBaseNetworkObject.NetworkObjectId); //Initialize references for the clients
        }

        //TODO?
        [ClientRpc]
        private void SetNameClientRpc(ulong TargetClient) {
            var clientId = NetworkManager.Singleton.LocalClientId;
            if (TargetClient != clientId)
                return;
        }

        private void InitializeNullNetworkObjectReferences(ulong networkObjectId) {
            var rootNetworkObect = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
            var references = rootNetworkObect.GetComponentsInChildren<Core.Utilities.NetworkObjectReference>(includeInactive: true);
            foreach (var reference in references) {
                if (reference.Reference == null) //TODO maybe this isn't correct
                    reference.Reference = rootNetworkObect;
            }
        }

        [ClientRpc]
        private void InitializeNullNetworkObjectReferencesClientRpc(ulong networkObjectId) {
            InitializeNullNetworkObjectReferences(networkObjectId);
        }
    }
}
