using Assets.Temp; //TODO remove me
using Castrimaris.Core;
using Castrimaris.Core.Exceptions;
using Castrimaris.Core.Monitoring;
using Castrimaris.Core.Utilities;
using Castrimaris.IO.Contracts;
using Castrimaris.Player.Contracts;
using Castrimaris.UI;
using Castrimaris.UI.Contracts;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Player;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Castrimaris.Player {

    public class PlayerManager : NetworkBehaviour, IPlayer, INetworkPlayer {

        #region PRIVATE VARIABLES

        [Header("References")]
        [SerializeField] private InterfaceReference<ITeleporter> teleporter;
        [SerializeField] private InterfaceReference<IPlayerController> controller;
        [SerializeField] private InterfaceReference<IVoiceComms> voiceComms;
        [SerializeField] private InterfaceReference<IPlayerAppearance> appearance;

        private NetworkVariable<FixedString512Bytes> playerName = new NetworkVariable<FixedString512Bytes>("Player", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private PlayerInitializationHelper initializationHelper = null; //TODO replace with an IInitializer

        #endregion

        #region PROPERTIES
        public ulong Id => NetworkObjectId;

        public IPlayerAppearance Appearance => appearance.Interface;
        #endregion

        #region UNITY & NETWORK OVERRIDES

        private void OnApplicationQuit() {
            Logout();
        }

        public override async void OnNetworkSpawn() {
            base.OnNetworkSpawn();

            SetName();
            SpawnAdditionalSystems();
            SetupOwnership();
            await TeleportToSpawn();
            InitializeVoiceComms();
            appearance.Interface.Load();
            StartCoroutine(OutOfBoundsBehaviour());
            UIManager.Singleton.FadeOut();
        }

        public override void OnNetworkDespawn() {
            Log.D($"Player {this.gameObject.name} logged out.");
            base.OnNetworkDespawn();

            NetworkManager.Singleton.SceneManager.OnLoadComplete -= TeleportToSpawn;
        }

        private void Awake() {
            RetrieveStaticReferences();
            playerName.OnValueChanged += UpdateName;
            NetworkManager.Singleton.SceneManager.OnLoadComplete += TeleportToSpawn;

            if (teleporter.Interface == null) throw new ReferenceMissingException(nameof(teleporter));
            if (controller.Interface == null) throw new ReferenceMissingException(nameof(controller));
            if (voiceComms.Interface == null) throw new ReferenceMissingException(nameof(voiceComms));
            if (appearance.Interface == null) throw new ReferenceMissingException(nameof(appearance));
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Locks the player movement
        /// </summary>
        public void Lock() { } //TODO

        /// <summary>
        /// Unlocks the player movement
        /// </summary>
        public void Unlock() { } //TODO

        public void Login() => throw new NotImplementedException();

        public void Logout() { if (IsOwner) LogoutServerRpc(); }

        #endregion

        #region PRIVATE METHODS

        private IEnumerator OutOfBoundsBehaviour() {
            if (!IsOwner)
                yield break;

            var wfs = new WaitForSeconds(2);
            do {
                yield return wfs;
                if (controller.Interface.transform.position.y < 0.0f) {
                    Log.W($"Out of bounds! Respawning player...");
                    TeleportToSpawn();
                }
            } while (true);
        }

        [ServerRpc]
        private void LogoutServerRpc(ServerRpcParams serverRpcParams = default) {
            Log.D($"Received client disconnection request from client {serverRpcParams.Receive.SenderClientId}");
            NetworkManager.Singleton.DisconnectClient(serverRpcParams.Receive.SenderClientId);
            var objs = NetworkManager.Singleton.SpawnManager.SpawnedObjectsList;
            foreach (var obj in objs) {
                if (obj.OwnerClientId == serverRpcParams.Receive.SenderClientId)
                    obj.RemoveOwnership();
            }
        }

        private async void TeleportToSpawn(ulong clientId, string sceneName, LoadSceneMode loadSceneMode) {
            if (IsOwner && NetworkObject.OwnerClientId == clientId) //Since this method is a callback and it's called on every client once a client loads a scene, we check first if the loading client is the one running this callback
                await TeleportToSpawn();
        }

        private async Task TeleportToSpawn() {
            if (IsOwner) {
                try {
                    var spawn = GameObject.FindGameObjectWithTag("Respawn");
                    teleporter.Interface.ForceTeleport(spawn.transform.position, Quaternion.identity);
                    Log.D($"Spawn object found at {spawn.transform.position}. Moved player to {teleporter.Interface.transform.position}.");
                } catch {
                    var defaultPosition = Vector3.zero;
                    teleporter.Interface.ForceTeleport(defaultPosition, Quaternion.identity);
                    Log.W($"No Spawn object in scene. Spawning at default position {defaultPosition}");
                }
            }
            await Task.CompletedTask;
        }

        private void RetrieveStaticReferences() {
            //Retrieve reference for the initialization helper.
            if (!this.gameObject.TryGetComponent<PlayerInitializationHelper>(out initializationHelper)) {
                Log.D($"Tried to retrieve reference for {nameof(PlayerInitializationHelper)}, but non has benn found on the object {this.gameObject.name}");
            }
        }

        private void SetupHurricaneStaticReferences() {
            if (!Utilities.IsVrPlatform())
                return;

            //Additional Hurricane setup
            HVRManager.Instance.PlayerController = controller.Interface as HVRPlayerController;
        }

        private void SetName() {
            if (IsServer) {
                //TODO replace with naming from some more reliable source.
                string name = $"{Prefixes.Rand()} {Suffixes.Rand()}";
                Log.D($"Chose name {name} for client {NetworkObject.OwnerClientId}.");
                playerName.Value = name;
            }
            name = playerName.Value.ToString();
        }

        /// <summary>
        /// Removes unused components if the owner of this NetworkPlayerManager is not the local Player.
        /// </summary>
        private void SetupOwnership() {
            if ((IsClient && !IsOwner) || (IsServer && !IsHost)) {

                //This is not the "local" client, but a remote player. Initialize it accordingly.
                if (initializationHelper == null) {
                    Log.D($"No {nameof(PlayerInitializationHelper)} found. Skipping initialization.");
                    return;
                }

                initializationHelper.RemoveComponents();

            } else {
                //This is a local client; initialize this accordingly

                //Setup the player head meshes on the "CameraIgnore" layer, so that the Player camera won't see them
                var tags = GetComponentsInChildren<Tags>();
                var cameraIgnoreTags = from tag in tags
                                       where tag.Has("CameraIgnore")
                                       select tag;
                foreach (var cameraIgnoreTag in cameraIgnoreTags) {
                    cameraIgnoreTag.gameObject.layer = LayerMask.NameToLayer("CameraIgnore");
                }
            }
        }

        private async void InitializeVoiceComms() {
            if (voiceComms == null)
                return;

            await voiceComms.Interface.Initialize();
            await voiceComms.Interface.Login();
        }

        private void SpawnAdditionalSystems() {
            if (IsServer) {
                if (initializationHelper == null) {
                    Log.D($"No {nameof(PlayerInitializationHelper)} found. Skipping additional systems instantiation.");
                    return;
                }

                //Instantiate additional systems prefabs and reparent them to this gameobject's NetworkObject
                foreach (var systemPrefab in initializationHelper.AdditionalNonUISystemPrefabs) {
                    var system = GameObject.Instantiate(systemPrefab);
                    if (!system.TryGetComponent<NetworkObject>(out var systemNetworkObject)) //Skip non NetworkObject systems
                        continue;
                    systemNetworkObject.Spawn();
                    systemNetworkObject.TrySetParent(NetworkObject);
                }

                //Instantiate additional VR system, don't reparent them because there's no need
                if (Utilities.IsVrPlatform()) {
                    foreach (var systemPrefab in initializationHelper.AdditionalVRSystems) {
                        var system = GameObject.Instantiate(systemPrefab);
                    }
                }
            } else if (IsOwner) {

                //Instantiate UI controls for platforms TODO move this to some other method or structure it better
                var runtimePlatform = Utilities.GetRuntimePlatform();
                switch (runtimePlatform) {
                    case RuntimePlatformTypes.PC:
                    case RuntimePlatformTypes.ANDROID:
                        var ui = initializationHelper.MainUIAdditionalSystemPrefab;
                        ui = GameObject.Instantiate(ui);
                        ui.name = "User Controls";
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Callback for the <see cref="playerName"/> network variable. Updates the name of the Player GO when it updates and reassigns player ids around.
        /// </summary>
        private void UpdateName(FixedString512Bytes PreviousValue, FixedString512Bytes NewValue) {
            this.gameObject.name = NewValue.ToString();
        }

        #endregion

    }

}