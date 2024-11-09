using UnityEngine;

namespace Castrimaris.Core.Editor {

    /// <summary>
    /// Fits a model to predefined bounds. Useful to uniform dimensions in something like menus, ui's, shops, etc.
    /// </summary>
    public sealed class ModelScaler : MonoBehaviour {

        [Header("Parameters")]
        [SerializeField] private Bounds targetBounds = new Bounds();
        [SerializeField] private Color targetBoundsColor = Color.red;
        [SerializeField] private Color meshBoundsColor = Color.blue;

        [Header("References")]
        [SerializeField] private GameObject target = null;

        private void OnDrawGizmosSelected() {
            Gizmos.color = targetBoundsColor;
            Gizmos.DrawWireCube(targetBounds.center, targetBounds.size);
            if (target != null) { 
                Gizmos.color = meshBoundsColor;
                if (target.TryGetComponent<MeshRenderer>(out var meshRenderer)) {
                    Gizmos.DrawWireCube(meshRenderer.bounds.center, meshRenderer.bounds.size);
                } else if (target.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer)) {
                    Gizmos.DrawWireCube(skinnedMeshRenderer.bounds.center, skinnedMeshRenderer.bounds.size);
                } else {
                    throw new MissingComponentException($"{nameof(ModelScaler)} needs either a MeshRenderer or SkinnedMeshRenderer on target to work!");
                }
            }
        }

        private void FitToBounds() {
            if (target == null) {
                target = this.gameObject;
            }

            if (target.TryGetComponent<MeshRenderer>(out var meshRenderer)) {
                //TODO?

            } else if (target.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer)) {

            } else {
                throw new MissingComponentException($"{nameof(ModelScaler)} needs either a MeshRenderer or SkinnedMeshRenderer on target to work!");
            }
        }

    }

}