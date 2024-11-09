using Castrimaris.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.HurricaneIntegration {

    /// <summary>
    /// A Grabbable that is subject to forces coming from <see cref="NetworkAttractor"/>s.
    /// </summary>
    [RequireComponent(typeof(NetworkGrabbable))]
    public class NetworkAttractable : NetworkBehaviour {

        #region PRIVATE VARIABLES
        /// <summary>
        /// The grabbable attached to this Attractable
        /// </summary>
        private NetworkGrabbable grabbable;
        /// <summary>
        /// The Rigidbody attached to this Attractable
        /// </summary>
        private new Rigidbody rigidbody;
        /// <summary>
        /// A List of tuples representing forces acting on this attractable, with the float representing the magnitude and the <see cref="Vector3"/> representing the position of the magnetic source.
        /// </summary>
        private List<(float,Vector3)> forces = new List<(float, Vector3)> ();
        #endregion

        #region PROPERTIES
        /// <summary>
        /// Shorthand for <see cref="NetworkGrabbable.IsBeingHeld"/>
        /// </summary>
        public bool IsBeingHeld => grabbable.IsBeingHeld;
        /// <summary>
        /// Shorthand for the Rigidbody attached to this Attractable
        /// </summary>
        public Rigidbody Rigidbody => rigidbody;
        #endregion

        #region PUBLIC METHODS
        /// <summary>
        /// Adds a magnetic force source to this attractable
        /// </summary>
        /// <param name="forceMagnitude">The magnitude of the force</param>
        /// <param name="magneticForcePosition">The position of the magnetic force</param>
        public void AddMagneticForce(float forceMagnitude, Vector3 magneticForcePosition) {
            if (IsOwnedByServer)
                ApplyMagneticForceServerRpc(forceMagnitude, magneticForcePosition);
            else
                ApplyMagneticForceClientRpc(forceMagnitude, magneticForcePosition);
        }

        /// <summary>
        /// Removes a magnetic force source from this attractable
        /// </summary>
        /// <param name="magneticForcePosition">The poisition of the magnetic force</param>
        public void RemoveMagneticForce(Vector3 magneticForcePosition) {
            if (IsOwnedByServer)
                RemoveMagneticForceServerRpc(magneticForcePosition);
            else
                RemoveMangeticForceClientRpc(magneticForcePosition);
        }
        #endregion

        #region UNITY OVERRIDES
        private void Awake() {
            grabbable = GetComponent<NetworkGrabbable>();
            rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate() {
            if (!IsOwner) 
                return;

            if (IsBeingHeld)
                return;

            if (forces.Count <= 0)
                return;

            for (int i = 0; i < forces.Count; i++) {
                var forceMagnitude = forces[i].Item1;
                var magneticForcePosition = forces[i].Item2;

                var forceDirection = (magneticForcePosition - this.transform.position).normalized;
                Rigidbody.AddForce(forceDirection * forceMagnitude, ForceMode.Force);
            }

        }
        #endregion

        #region PRIVATE METHODS
        /// <inheritdoc cref="AddMagneticForce(float, Vector3)"/>
        [ServerRpc(RequireOwnership = false)] private void ApplyMagneticForceServerRpc(float forceMagnitude, Vector3 magneticForcePosition) => InternalAddMagneticForce(forceMagnitude, magneticForcePosition);
        /// <inheritdoc cref="AddMagneticForce(float, Vector3)"/>
        [ClientRpc] private void ApplyMagneticForceClientRpc(float forceMagnitude, Vector3 magneticForcePosition) => InternalAddMagneticForce(forceMagnitude, magneticForcePosition);
        /// <inheritdoc cref="AddMagneticForce(float, Vector3)"/>
        private void InternalAddMagneticForce(float forceMagnitude, Vector3 magneticForcePosition) => forces.Add((forceMagnitude, magneticForcePosition));

        /// <inheritdoc cref="RemoveMagneticForce(Vector3)"/>
        [ServerRpc(RequireOwnership = false)] private void RemoveMagneticForceServerRpc(Vector3 magneticForcePosition) => InternalRemoveMagneticForce(magneticForcePosition);
        /// <inheritdoc cref="RemoveMagneticForce(Vector3)"/>
        [ClientRpc] private void RemoveMangeticForceClientRpc(Vector3 magneticForcePosition) => InternalRemoveMagneticForce(magneticForcePosition);
        /// <inheritdoc cref="RemoveMagneticForce(Vector3)"/>
        private void InternalRemoveMagneticForce(Vector3 magneticForcePosition) => forces.Remove(forces.Where(forceTuple => forceTuple.Item2 == magneticForcePosition).First());
        #endregion
    }

}