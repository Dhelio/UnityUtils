using Castrimaris.Core;
using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.HurricaneIntegration {

    /// <summary>
    /// Transfers the ownership of this object to the client that is colliding with it.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Rigidbody))]
    public class NetworkTouchOwnership : NetworkBehaviour {

        [Header("Parameters")]
        [SerializeField] private string tagToCheck = "Player";

        private void OnCollisionEnter(Collision collision) {
            if (!collision.gameObject.TryGetComponent<Tags>(out var tags))
                return;

            if (!tags.Has(tagToCheck))
                return;

            if (collision.gameObject.TryGetComponent<NetworkGrabbable>(out var networkGrabbable)) {
                if (networkGrabbable.IsBeingHeld)
                    return;
            }

            RequestOwnershipServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestOwnershipServerRpc(ServerRpcParams serverParams = default) {
            NetworkObject.ChangeOwnership(serverParams.Receive.SenderClientId);
        }

    }

}