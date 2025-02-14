#if HVR_OCULUS

using Castrimaris.Core;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.HurricaneIntegration {

    /// <summary>
    /// Class for handling simple socketing operations.
    /// </summary>
    [RequireComponent(typeof(Tags))]
    [RequireComponent(typeof(NetworkGrabbable))]
    public class NetworkSimpleSocketable : NetworkBehaviour {

        #region PUBLIC VARIABLES
        public NetworkVariable<bool> IsSocketed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        #endregion

        #region PRIVATE VARIABLES
        [SerializeField] private bool startSocketed;
        [SerializeField] private NetworkSimpleSocket socket;

        private bool originalIsKinematicValue;
        private Tags localTags;
        private new Rigidbody rigidbody;
        private NetworkGrabbable grabbable;
        #endregion

        #region PROPERTIES
        public bool IsKinematic { get => rigidbody.isKinematic; set => rigidbody.isKinematic = value; }
        public bool IsBeingHeld => grabbable.IsBeingHeld;
        #endregion

        #region PUBLIC METHODS

        public void ForceRelease() => grabbable.ForceRelease();
        public void Disable() => grabbable.DisableGrabbing();

        #endregion

        #region UNITY OVERRIDES

        private void Awake() {
            localTags = GetComponent<Tags>();
            rigidbody = GetComponent<Rigidbody>();
            grabbable = GetComponent<NetworkGrabbable>();

            originalIsKinematicValue = rigidbody.isKinematic;
        }

        private void Start() {
            grabbable.OnGrab.AddListener((_, _) => { //HACK:  this is due to the NetworkSimpleSocket maybe making this object Kinematic.
                if (socket == null)
                    return;

                if (this.socket.SocketableShouldBeKinematic)
                    rigidbody.isKinematic = originalIsKinematicValue;
            });
        }

        private void OnTriggerEnter(Collider other) {
            if (!IsOwner)
                return;

            if (IsSocketed.Value)
                return;

            if (!other.TryGetComponent<NetworkSimpleSocket>(out var socket))
                return;

            if (!other.TryGetComponent<Tags>(out var tags))
                return;

            if (!tags.HasAny(localTags))
                return;

            SetSocketServerRpc(socket.NetworkObjectId);
            socket.PlaceSocket(this);
        }

        private void OnTriggerExit(Collider other) {
            if (!IsOwner)
                return;

            if (!IsSocketed.Value)
                return;

            if (!other.TryGetComponent<NetworkSimpleSocket>(out var socket))
                return;

            if (socket != this.socket)
                return;

            if (!other.TryGetComponent<Tags>(out var tags))
                return;

            if (!tags.HasAny(localTags))
                return;

            this.socket.RemoveSocket();
        }

        #endregion

        #region PRIVATE METHODS

        [ServerRpc(RequireOwnership = true)]
        private void SetSocketServerRpc(ulong socketNetworkId) {
            SetSocketClientRpc(socketNetworkId);
            InternalSetSocket(socketNetworkId);
        }

        [ClientRpc]
        private void SetSocketClientRpc(ulong socketNetworkId) => InternalSetSocket(socketNetworkId);

        private void InternalSetSocket(ulong socketNetworkId) => socket = NetworkManager.Singleton.SpawnManager.SpawnedObjects[socketNetworkId].GetComponent<NetworkSimpleSocket>();

        #endregion
    }
}

#endif