using Castrimaris.Attributes;
using Castrimaris.Core;
using Castrimaris.Core.Monitoring;
using Castrimaris.HurricaneIntegration;
using Castrimaris.Interactables.Contracts;
using Castrimaris.Network;
using Castrimaris.Player;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.Interactables.Drawing {

    [RequireComponent(typeof(NetworkParentConstraint))] //Required to correctly sync an Interactor over the network
    [RequireComponent(typeof(NetworkGrabbable))]  //For VR
    [RequireComponent(typeof(NetworkSimpleSocketable))] //Required to place this Interactor in its socket
    public class EraserController : NetworkBehaviour, IRaycastInteractable, IRaycastInteractor {

        #region Private Variables

        [Header("ReadOnly Parameters")]
        [SerializeField, ReadOnly] private NetworkVariable<bool> isBeingUsed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private IPlayerController playerController = null;

        #endregion

        #region Properties

        public bool IsBeingUsed => isBeingUsed.Value;

        #endregion

        #region Public Methods

        public void OnRaycasted(IPlayerController playerController) {
            if (IsBeingUsed)
                return;

            this.playerController = playerController;
            playerController.Interactor = this;
            playerController.Animator.SetBool("IsHelding", true);

            //Set parent constraint
            var parentConstraint = this.gameObject.GetComponent<NetworkParentConstraint>();
            parentConstraint.Target = playerController.InteractorAnchoring;

            //Request ownership to the server
            OnRaycastedServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        public void Drop(IPlayerController playerController) {
            isBeingUsed.Value = false;
            var parentConstraint = this.gameObject.GetComponent<NetworkParentConstraint>();
            parentConstraint.Target = null;
            playerController.Interactor = null;
            playerController.Animator.SetBool("IsHelding", false); //Ungrab this interactor
        }

        public void OnRaycastingDown(RaycastHit[] hitInfos) {
            var hitInfo = hitInfos.FirstOrDefault(hitInfo => 
            hitInfo.collider.GetComponent<NetworkLineRenderer>() != null ||
            hitInfo.collider.GetComponent<NetworkSimpleSocket>() != null
            );

            var collider = hitInfo.collider;
            //Raycasted Nothing
            if (collider == null) {
                return;
            }

            //Raycasted socket
            if (collider.TryGetComponent<NetworkSimpleSocket>(out var networkSimpleSocket)) {
                isBeingUsed.Value= false;
                var parentConstraint = this.gameObject.GetComponent<NetworkParentConstraint>();
                parentConstraint.Target = null;
                networkSimpleSocket.PlaceSocket(this.gameObject.GetComponent<NetworkSimpleSocketable>());
                playerController.Interactor = null;
                playerController.Animator.SetBool("IsHelding", false); //Ungrab this interactor
                //TODO remove ownership?
                return;
            }

            //Raycasted Line Renderer
            if (collider.TryGetComponent<NetworkLineRenderer>(out var networkLineRenderer)) {
                var networkLineRendererId = hitInfo.collider.GetComponent<NetworkObject>().NetworkObjectId;
                OnRaycastingDownServerRpc(NetworkManager.Singleton.LocalClientId, networkLineRendererId);
                return;
            }
        }

        public void OnRaycasting(RaycastHit[] hitInfos) {
            if (!TryGetNetworkLineRenderer(hitInfos, out var networkLineRendererId))
                return;
            OnRaycastingServerRpc(NetworkManager.Singleton.LocalClientId, networkLineRendererId);
        }

        public void OnRaycastingUp(RaycastHit[] hitInfos) {
            if (!TryGetNetworkLineRenderer(hitInfos, out var networkLineRendererId))
                return;
            OnRaycastingUpServerRpc(NetworkManager.Singleton.LocalClientId, networkLineRendererId);
        }

        #endregion

        #region Unity Overrides

        private void OnCollisionEnter(Collision collision) {
            //VR Only behaviour
            if (!IsOwner ||
                !collision.collider.TryGetComponent<NetworkLineRenderer>(out var networkLineRenderer))
                return;

            OnCollisionEnterServerRpc(NetworkManager.Singleton.LocalClientId, networkLineRenderer.NetworkObjectId);
        }

        #endregion

        #region Private Methods

        private bool TryGetNetworkLineRenderer(RaycastHit[] hitInfos, out ulong networkLineRendererId) {
            networkLineRendererId = ulong.MaxValue;
            var hitInfo = hitInfos.FirstOrDefault(hitInfo => hitInfo.collider.GetComponent<NetworkLineRenderer>() != null);
            if (hitInfo.collider == null) { 
                return false;
            }
            networkLineRendererId = hitInfo.collider.GetComponent<NetworkObject>().NetworkObjectId;
            return true;
        }

        private bool TryDestroyNetworkLineRenderer(ulong networkLineRendererId) {
            if (!NetworkManager.Singleton.IsServer)
                return false;

            var lineObject = networkLineRendererId.FindNetworkObject();
            if (lineObject == null)
                return false;

            lineObject.Despawn(destroy: true);
            return true;
        }

        #region Server RPCs
        [ServerRpc(RequireOwnership = false)]
        private void OnRaycastedServerRpc(ulong requestingClientId) {
            isBeingUsed.Value = true;
            this.NetworkObject.ChangeOwnership(requestingClientId);
        }

        [ServerRpc] private void OnRaycastingDownServerRpc(ulong requestingClientId, ulong networkLineRendererId) => TryDestroyNetworkLineRenderer(networkLineRendererId);
        [ServerRpc] private void OnRaycastingServerRpc(ulong requestingClientId, ulong networkLineRendererId) => TryDestroyNetworkLineRenderer(networkLineRendererId);
        [ServerRpc] private void OnRaycastingUpServerRpc(ulong requestingClientId, ulong networkLineRendererId) => TryDestroyNetworkLineRenderer(networkLineRendererId);
        [ServerRpc] private void OnCollisionEnterServerRpc(ulong requestingClientId, ulong networkLineRendererId) => TryDestroyNetworkLineRenderer(networkLineRendererId);

        
        #endregion
        #endregion
    }
}
