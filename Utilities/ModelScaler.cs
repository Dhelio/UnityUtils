using Castrimaris.Core.Monitoring;
using UnityEngine;

namespace Castrimaris.Utilities {

    public sealed class ModelScaler : MonoBehaviour {

        [Header("Parameters")]
        [SerializeField] private Bounds targetBounds = new Bounds();
        [SerializeField] private Color targetBoundsColor = Color.red;
        [Range(0.1f, 1.0f), SerializeField] private float targetBoundsCenterSphereSize = 1.0f;
        [SerializeField] private Color meshBoundsColor = Color.blue;
        [Range(0.1f, 1.0f), SerializeField] private float meshBoundsCenterSphereSize = 1.0f;

        [Header("References")]
        [SerializeField] private GameObject target = null;

        private void OnDrawGizmosSelected() {
            Gizmos.color = targetBoundsColor;
            Gizmos.DrawWireCube(targetBounds.center, targetBounds.size);
            Gizmos.DrawSphere(targetBounds.center, targetBoundsCenterSphereSize);
            if (target != null) { 
                Gizmos.color = meshBoundsColor;
                if (target.TryGetComponent<MeshRenderer>(out var meshRenderer)) {
                    Gizmos.DrawWireCube(meshRenderer.bounds.center, meshRenderer.bounds.size);
                    Gizmos.DrawSphere(meshRenderer.bounds.center, meshBoundsCenterSphereSize);
                } else if (target.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer)) {
                    Gizmos.DrawWireCube(skinnedMeshRenderer.bounds.center, skinnedMeshRenderer.bounds.size);
                    Gizmos.DrawSphere(skinnedMeshRenderer.bounds.center, meshBoundsCenterSphereSize);
                } else {
                    throw new MissingComponentException($"{nameof(ModelScaler)} needs either a MeshRenderer or SkinnedMeshRenderer on target to work!");
                }
            }
        }

        [ContextMenu("Fit To Bounds")]
        private void FitToBounds() {
            if (target == null) {
                target = this.gameObject;
            }

            if (target.TryGetComponent<MeshRenderer>(out var meshRenderer)) {
                Log.D($"Detected component of type MeshRenderer. Encapsulating target bounds.");
                meshRenderer.bounds.Encapsulate(targetBounds);
                SetPositionToCenter(meshRenderer.bounds);
            } else if (target.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer)) {
                Log.D($"Detected component of type SkinnedMeshRenderer. Encapsulating target bounds.");
                skinnedMeshRenderer.bounds.Encapsulate(targetBounds);
                SetPositionToCenter(skinnedMeshRenderer.bounds);
            } else {
                throw new MissingComponentException($"{nameof(ModelScaler)} needs either a MeshRenderer or SkinnedMeshRenderer on target to work!");
            }
        }

        private void SetPositionToCenter(Bounds MeshBounds) {
            Log.D($"Setting local position of GameObject {this.name} to target bounds center: {targetBounds.center}");
            this.transform.position = targetBounds.center + (transform.position - MeshBounds.center);
        }
    }
}