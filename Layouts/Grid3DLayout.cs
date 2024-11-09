using UnityEngine;
using Castrimaris.Core.Monitoring;
using System.Collections.Generic;
using System;
using Castrimaris.Core.Extensions;

namespace Castrimaris.Layouts {

    /// <summary>
    /// 3D Grid Layout for GameObjects
    /// </summary>
    [ExecuteAlways, DisallowMultipleComponent]
    public partial class Grid3DLayout : Base3DLayout {

        [Header("Parameters")]
        public OrderModes LayoutOrder = OrderModes.BY_COLUMN;
        public DirectionModes LayoutDirection = DirectionModes.X;
        public OverflowModes OverflowBehaviour = OverflowModes.OVERFLOW;
        public AnimationModes animationMode = AnimationModes.INSTANT;
        [Range(1, 50)] public int ConstraintCount = 1;
        public Vector3 Spacing = Vector3.zero;
        public Vector3 Offset = Vector3.zero;

        [SerializeField] private GridContainer container = new GridContainer();
        [SerializeField] private List<GridCell> cells = new List<GridCell>();

        [ContextMenu("Force Update")]
        public override void UpdateLayout() {
            int i;
            switch (animationMode) {
                case AnimationModes.ANIMATED:
                    var animators = GetComponentsInChildren<Grid3DLayoutAnimator>();
                    i = 0;
                    foreach (var animator in animators) {
                        animator.Move(cells[i].Center, true);
                        i++;
                    }
                    break;
                case AnimationModes.INSTANT:
                default:
                    for (i = 0; i < transform.childCount; i++) {
                        var child = transform.GetChild(i);
                        child.localPosition = cells[i].Center;
                    }
                    break;
            }
        }

        protected override void Awake() {
            base.Awake();

            InitializeAnimationMode();
        }

        protected override void Update() {
            base.Update();

#if UNITY_EDITOR
            EditorUpdate();
#endif
        }

        private void InitializeGridCells() {
            //Sanity Check
            if (transform.childCount <= 0)
                return;

            if (cells.Count > 0)
                cells.Clear();

            switch (LayoutOrder) {
                case OrderModes.BY_COLUMN:
                    //Get number of columns / rows
                    var rows = transform.childCount / ConstraintCount; //either rows or columns
                    var columns = transform.childCount / Mathf.Clamp(rows, 1, int.MaxValue); //either columns or rows

                    //Extents of each cell
                    var cellsExtents = new Vector3(
                        container.Extents.x / columns,
                        container.Extents.y / rows,
                        container.Extents.z
                        );

                    for (int row = 0, i = 0; row < transform.childCount; row++) {
                        for (int column = 0; column < ConstraintCount && i < transform.childCount; column++, i++) {

                            var rowSpace = (container.Y / rows) * row;
                            var columnSpace = (container.X / columns) * column;

                            var cx = container.X / columns;
                            var cy = container.Y / rows;
                            var cz = container.Z; //Subdivide with layer

                            var cellCenter = new Vector3(
                                cx + (cx * column) + Spacing.x * column + columnSpace,
                                cy + (cy * row) + Spacing.y * row + rowSpace,
                                cz + Spacing.z
                                );

                            cellCenter += Offset;

                            var cell = new GridCell(cellCenter, cellsExtents, i);
                            cells.Add(cell);
                        }
                    }
                    break;
                case OrderModes.BY_ROW:
                    throw new NotImplementedException();
                case OrderModes.BY_BEST_FIT:
                    throw new NotImplementedException();
                default:
                    Log.E("No such LayoutOrder!");
                    break;
            }
        }

        private void InitializeAnimationMode() {
            switch (animationMode) {
                case AnimationModes.ANIMATED:
                    for (int i = 0; i < transform.childCount; i++) {
                        var child = transform.GetChild(i).gameObject;
                        if (!child.HasComponent<Grid3DLayoutAnimator>()) {
                            var animator = child.AddComponent<Grid3DLayoutAnimator>();
                        }
                    }
                    break;
                case AnimationModes.INSTANT:
                default:
                    ClearAnimatorsInChildren();

                    var animators = GetComponentsInChildren<Grid3DLayoutAnimator>();
                    foreach (var animator in animators) {
                        DestroyImmediate(animator);
                    }
                    break;
            }
        }

        private Bounds GetObjectMeshBounds(GameObject Object) {
            if (Object.HasComponent<MeshFilter>()) {
                return Object.GetComponent<MeshFilter>().sharedMesh.bounds;
            } else if (Object.HasComponent<SkinnedMeshRenderer>()) {
                return Object.GetComponent<SkinnedMeshRenderer>().sharedMesh.bounds;
            }
            throw new MissingComponentException($"No component of type {nameof(MeshFilter)} or {nameof(SkinnedMeshRenderer)} attached to the object with name {Object.name}. Could not get Bounds! Ignoring object...");
        }

        [ContextMenu("Set Bounds center to Object's Center")]
        private void SetBoundsCenterToObjectCenter() {
            container.Center = this.transform.position;
        }

        private Vector3 GetBiggestExtentsInChildrenBounds() {
            var result = Vector3.negativeInfinity;
            for (int i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);
                var bounds = GetObjectMeshBounds(child.gameObject);
                if (bounds.extents.magnitude > result.magnitude) {
                    result = bounds.extents;
                }
            }
            return result;
        }

        private void ClearAnimatorsInChildren() {
            var animators = GetComponentsInChildren<Grid3DLayoutAnimator>();
            foreach (var animator in animators) {
                DestroyImmediate(animator);
            }
        }

        [ContextMenu("Randomize Children's Order")]
        private void RandomizeOrder() {
            this.transform.ShuffleChildren();
        }

        private void Reset() {
            LayoutOrder = OrderModes.BY_COLUMN;
            LayoutDirection = DirectionModes.X;
            OverflowBehaviour = OverflowModes.OVERFLOW;
            ConstraintCount = 1;
            Spacing = Vector3.zero;
            Offset = Vector3.zero;
            container.Center = Vector3.zero;
            container.Extents = Vector3.one;
        }

        private void OnDestroy() {
            ClearAnimatorsInChildren();
        }

    }
}