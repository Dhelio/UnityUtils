using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.HurricaneIntegration {

    /// <summary>
    /// An object that applies magnetic forces to <see cref="NetworkAttractable"/>s.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class NetworkAttractor : NetworkBehaviour {

        #region PRIVATE VARIABLES
        [Header("Parameters")]
        [Range(0f, 10f), SerializeField] private float attractionStrength = 1f;
        #endregion

        #region UNITY OVERRIDES
        /// <summary>
        /// If the colliding object is a <see cref="NetworkAttractable"/> then applies its forces to the object.
        /// </summary>
        private void OnTriggerEnter(Collider other) {
            if (!IsServer)
                return;

            if (!other.gameObject.TryGetComponent<NetworkAttractable>(out var attractable))
                return;

            attractable.AddMagneticForce(attractionStrength, this.transform.position);
        }

        /// <summary>
        /// If the colliding object is a <see cref="NetworkAttractable"/> then removes its forces from the object.
        /// </summary>
        private void OnTriggerExit(Collider other) {
            if (!IsServer)
                return;

            if (!other.gameObject.TryGetComponent<NetworkAttractable>(out var attractable))
                return;

            attractable.RemoveMagneticForce(this.transform.position);
        }
        #endregion
    }
}