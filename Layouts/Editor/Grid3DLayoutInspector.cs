#if UNITY_EDITOR

using UnityEngine;

namespace Castrimaris.Layouts {

    public partial class Grid3DLayout : Base3DLayout {

        [Header("Editor Parameters")]
        [SerializeField] private bool alwaysShow = false;
        [SerializeField] private bool updateInEditor = false;
        [SerializeField] private bool drawCellsBounds = false;
        [SerializeField] private bool drawChildrenBounds = false;
        [SerializeField] private Color containerBoundsColor = Color.yellow;
        [SerializeField] private Color cellBoundsColor = Color.cyan;
        [SerializeField] private Color childrenBoundsColor = Color.red;

        protected void EditorUpdate() {
            if (updateInEditor) {
                InitializeGridCells();
                InitializeAnimationMode();
                UpdateLayout();
            }
        }

        private void OnDrawGizmosSelected() {
            if (alwaysShow)
                return;

            DrawGizmos();
        }

        private void OnDrawGizmos() {
            if (!alwaysShow)
                return;

            DrawGizmos();
        }

        private void DrawGizmos() {
            var rotationMatrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, this.transform.lossyScale);
            Gizmos.matrix = rotationMatrix;
            DrawContainerBounds();
            if (drawChildrenBounds) {
                DrawChildrenBounds();
            }
            if (drawCellsBounds) {
                DrawCellsBounds();
            }
        }

        private void DrawContainerBounds() {
            Gizmos.color = containerBoundsColor;
            Gizmos.DrawWireCube(container.Origin, container.Size);
        }

        private void DrawCellsBounds() {
            Gizmos.color = cellBoundsColor;
            foreach (var cell in cells) {
                Gizmos.DrawWireCube(cell.Center, cell.Extents * 2);
            }
        }

        private void DrawChildrenBounds() {
            Gizmos.color = childrenBoundsColor;
            for (int i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);
                var bounds = GetObjectMeshBounds(child.gameObject);
                Gizmos.DrawWireCube(child.localPosition, bounds.extents * 2);
            }
        }

    }
}

#endif