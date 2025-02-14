#if HVR_OCULUS

using Castrimaris.Core;
using Castrimaris.Core.Extensions;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.HurricaneIntegration {

    /// <summary>
    /// Makes <see cref="NetworkGrabbable"/> components stick to other objects with a certain tag.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NetworkGrabbable))]
    public class NetworkStickable : NetworkBehaviour {

        [Header("Paramater")]
        [Tooltip("The tag to look for. Must be set in a Tags component.")]
        [SerializeField] private string tagToLookFor = "Wall";

        [Header("Events")]
        public UnityEvent OnSticked = new UnityEvent();

        private new Rigidbody rigidbody;

        private void Awake() {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision) {
            if (!IsOwner)
                return;

            if (!collision.collider.TryGetComponent<Tags>(out var tags))
                return;

            if (!tags.Has(tagToLookFor))
                return;

            rigidbody.velocity = Vector3.zero;
            StickedServerRpc();
        }

        private void Sticked() {
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }

        [ServerRpc(RequireOwnership = true)]
        private void StickedServerRpc() {
            Sticked();
            StickedClientRpc();
            OnSticked.Invoke();
        }

        [ClientRpc]
        private void StickedClientRpc() {
            Sticked();
            OnSticked.Invoke();
        }
    }

}

#endif