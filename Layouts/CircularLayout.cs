using UnityEngine;

namespace Castrimaris.Layouts {

    [ExecuteAlways, DisallowMultipleComponent]
    public partial class Circular3DLayout : Base3DLayout {

        #region Public Fields

        [Header("Parameters")]
        public float DistanceFromCenter = 0.0f;
        public float RotationOffset = 0.0f;
        public bool ShouldLookAtCenter = false;
        public DirectionModes layoutDirection = DirectionModes.X;

        #endregion Public Fields

        #region Public Methods

        [ContextMenu("Force Update")]
        public override void UpdateLayout() {
            base.UpdateLayout();

            var normal = ParseDirection();
            var degPerChild = 360.0f / (float)this.transform.childCount;
            for (int i = 0; i < this.transform.childCount; i++) {
                var childRad = Mathf.Deg2Rad * (degPerChild * i + RotationOffset);
                var childTransform = this.transform.GetChild(i);

                var inverseNormals = ParseInverseDirections();
                var inverseAxisOne = DistanceFromCenter * inverseNormals.Item1 * Mathf.Cos(childRad);
                var inverseAxisTwo = DistanceFromCenter * inverseNormals.Item2 * Mathf.Sin(childRad);

                childTransform.localPosition = inverseAxisOne + inverseAxisTwo;

                if (ShouldLookAtCenter) {
                    childTransform.LookAt(this.transform.position);
                } else {
                    childTransform.rotation = Quaternion.Euler(normal);
                }
            }
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Awake() {
            base.Awake();
        }

        protected override void Update() {
            base.Update();

#if UNITY_EDITOR
            EditorUpdate();
#endif
        }

        #endregion Protected Methods

        #region Private Methods

        private Vector3 ParseDirection() {
            switch (layoutDirection) {
                case DirectionModes.X:
                    return this.transform.right;

                case DirectionModes.Y:
                    return this.transform.up;

                case DirectionModes.Z:
                    return this.transform.forward;

                default:
                    return Vector3.zero;
            }
        }

        private (Vector3, Vector3) ParseInverseDirections() {
            switch (layoutDirection) {
                case DirectionModes.X:
                    return (this.transform.up, this.transform.forward);

                case DirectionModes.Y:
                    return (this.transform.right, this.transform.forward);

                case DirectionModes.Z:
                    return (this.transform.up, this.transform.right);

                default:
                    return (Vector3.zero, Vector3.zero);
            }
        }

        #endregion Private Methods
    }
}