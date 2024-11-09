using Castrimaris.Attributes;
using Castrimaris.Player;
using HurricaneVR.Framework.Core.Player;
using UnityEngine;

namespace Castrimaris.HurricaneIntegration {

    /// <summary>
    /// Helper class that implements <see cref="ITeleporter"/> interface and uses Hurricane's <see cref="HVRTeleporter"/> system.
    /// </summary>
    [RequireComponent(typeof(HVRTeleporter))]
    public class HVRTeleporterInterface : MonoBehaviour, ITeleporter {

        [Header("Debugging")]
        [SerializeField] private Vector3 pos;

        private HVRTeleporter teleporter;

        /// <inheritdoc cref="ITeleporter.ForceTeleport(Transform)"/>
        public void ForceTeleport(Transform transform) => InternalForceTeleport(transform.position);

        /// <inheritdoc cref="ITeleporter.ForceTeleport(Vector3, Quaternion)"/>
        public void ForceTeleport(Vector3 position, Quaternion rotation) => InternalForceTeleport(position);

        /// <inheritdoc cref="ITeleporter.Teleport(Vector3, Quaternion)"/>
        public void Teleport(Vector3 position, Quaternion rotation) {
            if (teleporter == null) {
                Awake();
            }
            
            teleporter.Teleport(position, rotation * Vector3.forward);
        }

        /// <inheritdoc cref="ITeleporter.Teleport(Transform)"/>
        public void Teleport(Transform transform) {
            if (teleporter == null) {
                Awake();
            }
            teleporter.Teleport(transform.position, transform.rotation * transform.forward);
        }

        private void Awake() {
            teleporter = GetComponent<HVRTeleporter>();
        }

        private void InternalForceTeleport(Vector3 position) {
            bool originalGroundedCheck = teleporter.PlayerGroundedCheck;
            bool originalClimbingCheck = teleporter.PlayerClimbingCheck;

            teleporter.PlayerGroundedCheck = false;
            teleporter.PlayerClimbingCheck = false;

            teleporter.Teleport(position);

            teleporter.PlayerGroundedCheck = originalGroundedCheck;
            teleporter.PlayerClimbingCheck = originalClimbingCheck;
        }

        [ExposeInInspector]
        private void EditorForceTeleport() => InternalForceTeleport(pos);
    }

}