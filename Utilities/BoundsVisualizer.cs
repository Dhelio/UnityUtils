using Castrimaris.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Castrimaris.Core.Monitoring;

namespace Castrimaris.Utilities {

    public class BoundsVisualizer : MonoBehaviour {

        [Header("Parameters")]
        [SerializeField] private Color boundsColor = Color.red;

        [Header("References")]
        [SerializeField, ReadOnly] private Bounds bounds;
        [SerializeField, ReadOnly] private MeshRenderer meshRenderer = null;
        [SerializeField, ReadOnly] private SkinnedMeshRenderer skinnedMeshRenderer = null;

        private void OnDrawGizmosSelected() {
            GetBounds();
            var rotationMatrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, this.transform.lossyScale);
            Gizmos.matrix = rotationMatrix;
            Gizmos.color = boundsColor;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        private void FindRenderer() {
            if (!this.gameObject.TryGetComponent<MeshRenderer>(out meshRenderer)) {
                if (!this.gameObject.TryGetComponent<SkinnedMeshRenderer>(out skinnedMeshRenderer)) {
                    Log.E($"Object must have either MeshRenderer or SkinnedMeshRenderer!");
                    return;
                }
            }
        }

        private void GetBounds() {
            if (meshRenderer == null && skinnedMeshRenderer == null) 
                FindRenderer();
            if (meshRenderer != null) { 
                bounds = meshRenderer.bounds;
                return;
            }
            if (skinnedMeshRenderer != null) { 
                bounds = skinnedMeshRenderer.bounds;
                return;
            }

            throw new MissingComponentException("No component of type MeshRenderer or SkinnedMeshRenderer found");
        }
    }

}
