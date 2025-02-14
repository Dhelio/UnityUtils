#if HVR_OCULUS

using Castrimaris.HurricaneIntegration;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.HurricaneIntegration {

    /// <summary>
    /// If it collides with a <see cref="NetworkBumpable"/> then transfers its forces to it.
    /// </summary>
    [RequireComponent(typeof(NetworkGrabbable))]
    public class NetworkThrowable : NetworkBehaviour {

        private new Rigidbody rigidbody;

        private void Awake() {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision) {
            if (collision.gameObject.TryGetComponent<NetworkBumpable>(out var bumpable)) {
                var contact = collision.contacts.First();
                bumpable.TransferImpactForce(rigidbody, contact);
            }
        }

    }
}

#endif