using Castrimaris.Attributes;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Castrimaris.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Castrimaris.Interactables {

    [RequireComponent(typeof(BoxCollider))]
    public class Teleporter : MonoBehaviour {

        #region Private Variables

        [Header("Parameters")]
        [SerializeField] private Vector3 teleportingOffset = Vector3.up;

        [Header("References")]
        [SerializeField] private Teleporter connectedTeleporter = null;

        [Header("ReadOnly Parameters")]
        [SerializeField, ReadOnly] private bool isTeleporting = false;

        [Header("Debug")]
        [SerializeField] private Color debugColor = Color.red;

        #endregion

        #region Properties

        public Vector3 TeleportingPosition => transform.position + teleportingOffset;
        public bool IsTeleporting => isTeleporting;

        #endregion

        #region Public Methods

        public void ResetTeleportingFlags() {
            isTeleporting = false;
        }

        #endregion

        #region Unity Overrides

        private void Awake() {
            if (connectedTeleporter == null)
                throw new MissingReferenceException($"No reference set for {nameof(connectedTeleporter)}. Did you forget to set it in the Editor?");
            if (connectedTeleporter == this)
                throw new System.InvalidOperationException($"{nameof(connectedTeleporter)} reference can't be itself! Assign another {typeof(Teleporter)} to this reference!");

            //Setup boxcollider
            var boxCollider = GetComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider collider) {
            if (!CheckCollisionRequirements(collider.gameObject, out var entity))
                return;

            if (connectedTeleporter.IsTeleporting)
                return;

            isTeleporting = true;
            entity.transform.position = connectedTeleporter.TeleportingPosition;
        }

        private void OnTriggerExit(Collider collider) {
            if (!CheckCollisionRequirements(collider.gameObject, out _))
                return;

            if (!connectedTeleporter.IsTeleporting)
                return;

            connectedTeleporter.ResetTeleportingFlags();
        }

        #endregion

        #region Private Methods

        private bool CheckCollisionRequirements(GameObject target, out IEntity entity) {
            if (!target.TryGetInterface<IEntity>(out entity))
                return false;

            return true;
        }

        #endregion

        #region Debug Methods

        private void OnDrawGizmosSelected() {
            if (connectedTeleporter == null)
                return;

            Gizmos.color = debugColor;
            Gizmos.DrawLine(this.transform.position, connectedTeleporter.transform.position);
            Gizmos.DrawSphere(TeleportingPosition, .1f);
        }

        #endregion
    }
}
