using Castrimaris.Core.Monitoring;
using Castrimaris.Network;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Castrimaris.HurricaneIntegration {

    /// <summary>
    /// Allows for a <see cref="HVRGrabbable"/> component to be used on the Network by clients.
    /// It works by making the transform and physics client authoritative.
    /// </summary>
    [RequireComponent(typeof(NetworkObject), typeof(ClientNetworkTransform))]
    [RequireComponent(typeof(Rigidbody), typeof(NetworkRigidbody))]
    [RequireComponent(typeof(HVRGrabbable))]
    [DisallowMultipleComponent]
    public class NetworkGrabbable : NetworkBehaviour {

        #region PRIVATE VARIABLES
        [Header("Network Variables")]
        [SerializeField] private NetworkVariable<ulong> owningClientId = new NetworkVariable<ulong>(ulong.MaxValue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private NetworkVariable<bool> isBeingHeld = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private HVRGrabbable grabbable = null;
        #endregion

        #region PROPERTIES
        public bool IsBeingHeld => isBeingHeld.Value;
        public VRGrabberEvent OnGrab => grabbable.Grabbed;
        public VRGrabberEvent OnRelease => grabbable.Released;
        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Requests ownership for grabbing the object from the server
        /// </summary>
        public void Grab(HVRGrabberBase grabber, HVRGrabbable grabbable) => RequestOwnershipServerRpc();

        public void Release(HVRGrabberBase grabber, HVRGrabbable grabbable) => ReleaseOwnershipServerRpc();

        public void ForceRelease() {
            if (!IsServer)
                return;

            ForceReleaseServerRpc();
        }

        public void DisableGrabbing() => grabbable.enabled = false;

        public override void OnNetworkSpawn() {
            if (!IsServer)
                return;

            owningClientId.Value = ulong.MaxValue;
            isBeingHeld.Value = false;
        }

        #endregion

        #region UNITY OVERRIDES

        private void Awake() {
            grabbable = GetComponent<HVRGrabbable>();
        }

        #endregion

        #region PRIVATE METHODS

        [ServerRpc(RequireOwnership = false)]
        private void RequestOwnershipServerRpc(ServerRpcParams serverParams = default) {
            if (isBeingHeld.Value) {
                Log.D($"Client {serverParams.Receive.SenderClientId} tried to use the item {this.gameObject.name}, but it is already being used by {owningClientId.Value}. Skipping interaction.");
                return;
            }

            owningClientId.Value = serverParams.Receive.SenderClientId;
            isBeingHeld.Value = true;
            NetworkObject.ChangeOwnership(owningClientId.Value);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ReleaseOwnershipServerRpc(ServerRpcParams serverParams = default) {
            if (serverParams.Receive.SenderClientId != owningClientId.Value) {
                Log.D($"Client {serverParams.Receive.SenderClientId} tried to release ownership on {this.gameObject.name}, but it's owned by {owningClientId.Value}. Skipping interaction.");
                return;
            }

            //Item may be held by two hands at the same time. In this case we shouldn't release ownership!
            var isItemTwoHanded = grabbable.HoldType == HurricaneVR.Framework.Shared.HVRHoldType.TwoHanded;
            var isAnyHandHeldingTheItem = grabbable.IsLeftHandGrabbed || grabbable.IsRightHandGrabbed;

            if (isItemTwoHanded && isAnyHandHeldingTheItem)
                return;

            owningClientId.Value = ulong.MaxValue;
            isBeingHeld.Value = false;
        }

        [ServerRpc(RequireOwnership = false)]
        private void ForceReleaseServerRpc() {
            owningClientId.Value = ulong.MaxValue;
            isBeingHeld.Value = false;
            grabbable.ForceRelease();
            ForceReleaseClientRpc();
        }

        [ClientRpc] private void ForceReleaseClientRpc() => grabbable.ForceRelease();

        #endregion
    }
}