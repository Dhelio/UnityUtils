using Castrimaris.Core.Monitoring;
using UnityEngine;

namespace Castrimaris.Player {

    /// <summary>
    /// Teleporter for the Mobile user.
    /// </summary>
    public class MobileTeleporter : MonoBehaviour, ITeleporter {

        [Header("References")]
        [SerializeField] private Transform teleportTarget;

        public void ForceTeleport(Transform transform) {
            Teleport(transform);
        }

        public void ForceTeleport(Vector3 Position, Quaternion Rotation) {
            Teleport(Position, Rotation);
        }

        public void Teleport(Vector3 position, Quaternion rotation) {
            //TODO
            var cc = teleportTarget.gameObject.GetComponent<CharacterController>();
            cc.enabled = false;

            teleportTarget.transform.position = position;
            teleportTarget.transform.rotation = rotation;

            cc.enabled = true;
        }

        public void Teleport(Transform transform) {
            teleportTarget.transform.position = transform.position;
            teleportTarget.transform.rotation = transform.rotation;
        }

        private void Awake() {
            if (teleportTarget == null) {
                Log.W($"No reference set for {nameof(teleportTarget)}, using local transform as target instead.");
                teleportTarget = this.transform;
            }
        }
    }

}
