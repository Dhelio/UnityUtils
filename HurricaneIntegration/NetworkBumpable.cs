#if HVR_OCULUS

using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Castrimaris.HurricaneIntegration {

    /// <summary>
    /// Obtains incoming impact forces coming from a <see cref="NetworkThrowable"/> <see cref="Rigidbody"/> and applies them to this <see cref="Rigidbody"/>
    /// </summary>
    [RequireComponent(typeof(NetworkRigidbody))]
    public class NetworkBumpable : NetworkBehaviour {

        private new Rigidbody rigidbody;

        public void TransferImpactForce(Rigidbody ImpactingRigidbody, ContactPoint Contact) {
            //Get data
            var mass = ImpactingRigidbody.mass;
            var point = Contact.point;
            var normal = Contact.normal;
            var speed = ImpactingRigidbody.velocity.magnitude;

            TransferImpactForceServerRpc(mass, point, normal, speed);
        }

        private void Awake() {
            rigidbody = GetComponent<Rigidbody>();
        }

        [ServerRpc(RequireOwnership = false)]
        private void TransferImpactForceServerRpc(float Mass, Vector3 Point, Vector3 Normal, float Speed) {
            //Either the server updates the object or the owning client
            if (!NetworkObject.IsOwnedByServer) {
                TransferImpactForceClientRpc(NetworkObject.OwnerClientId, Mass, Point, Normal, Speed);
            } else {
                TransferImpactForce(Mass, Point, Normal, Speed); 
            }
        }

        [ClientRpc]
        private void TransferImpactForceClientRpc(ulong OwnerClientId, float Mass, Vector3 Point, Vector3 Normal, float Speed) {
            if (NetworkManager.Singleton.LocalClientId != OwnerClientId)
                return;

            TransferImpactForce(Mass, Point, Normal, Speed);
        }

        private void TransferImpactForce(float Mass, Vector3 Point, Vector3 Normal, float Speed) {
            rigidbody.AddForceAtPosition(Mass * Speed * -Normal, Point, ForceMode.Impulse);
        }

    }

}

#endif