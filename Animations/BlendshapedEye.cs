using Castrimaris.Attributes;
using Castrimaris.Core;
using Castrimaris.Core.Exceptions;
using Castrimaris.Core.Extensions;
using UnityEngine;

namespace Castrimaris.Animations {

    /// <summary>
    /// Simple eye movment for eyes that use blendshapes to change direction.
    /// This is most useful when using automatic face rigging tools like "Face It!" For Blender and then importing those models in Unity.
    /// </summary>
    public class BlendshapedEye : MonoBehaviour {

        private const float BLENDSHAPE_MAX = 100f; //Maximum value of the blendshape in Unity

        [Header("Parameters")]
        [Tooltip("The target eye model whose belndshapes to change")]
        [SerializeField] private SkinnedMeshRenderer eye;
        [Tooltip("The name of the blendshape that makes the eye look right.")]
        [SerializeField] private string lookRightBlendshapeName;
        [Tooltip("The name of the blendshape that makes the eye look left.")]
        [SerializeField] private string lookLeftBlendshapeName;
        [Tooltip("The name of the blendshape that makes the eye look up.")]
        [SerializeField] private string lookUpBlendshapeName;
        [Tooltip("The name of the blendshape that makes the eye look down.")]
        [SerializeField] private string lookDownBlendshapeName;
        [Tooltip("Sensitivity of the animation (higher values make the eye move faster")]
        [SerializeField, Range(1f, 300f)] private float sensitivity = 100f;
        [Tooltip("The axis where to apply the rotation (sometimes models can have blendshapes that are oriented on another axis instead of the Unity default, this overrides it)")] //TODO is this correct?
        [SerializeField] private AxisTypes xAxis, yAxis;
        [SerializeField] private Vector3 localPositionOffset = Vector3.zero;

        [Header("ReadOnly Parameters")]
        [ReadOnly, SerializeField] private Vector3 direction = Vector3.zero;
        [ReadOnly, SerializeField] private Vector3 localDirection = Vector3.zero;
        [ReadOnly, SerializeField] private Vector3 valueToAnalyze = Vector3.zero;
        [ReadOnly, SerializeField] private float verticalWeight, horizontalWeight, distance, targetSensitivity;

        [Header("References")]
        [SerializeField] private Transform target = null;

        private int lookRightBlendshapeIndex;
        private int lookLeftBlendshapeIndex;
        private int lookUpBlendshapeIndex;
        private int lookDownBlendshapeIndex;

        /// <summary>
        /// The target transform to look at.
        /// </summary>
        public Transform Target { set => target = value; }

        private void Awake() {
            if (eye == null) throw new ReferenceMissingException(nameof(eye));

            var sharedMesh = eye.sharedMesh;
            lookRightBlendshapeIndex = sharedMesh.GetBlendShapeIndex(lookRightBlendshapeName);
            lookLeftBlendshapeIndex = sharedMesh.GetBlendShapeIndex(lookLeftBlendshapeName);
            lookUpBlendshapeIndex = sharedMesh.GetBlendShapeIndex(lookUpBlendshapeName);
            lookDownBlendshapeIndex = sharedMesh.GetBlendShapeIndex(lookDownBlendshapeName);
        }

        private void Update() {
            if (target != null) {
                FocusTarget();
            }
            //TODO idle animation
        }

        private void Reset() {
            if (!this.TryGetComponent<SkinnedMeshRenderer>(out eye))
                return;
            target = null;
        }

        private void ResetBlendShapes() {
            eye.SetBlendShapeWeight(lookRightBlendshapeIndex, 0);
            eye.SetBlendShapeWeight(lookLeftBlendshapeIndex, 0);
            eye.SetBlendShapeWeight(lookUpBlendshapeIndex, 0);
            eye.SetBlendShapeWeight(lookDownBlendshapeIndex, 0);
        }

        private void FocusTarget() {

            ResetBlendShapes();

            direction = target.position - (transform.position + localPositionOffset);
            localDirection = transform.InverseTransformDirection(direction);

            distance = Vector3.Distance(target.position, transform.position + localPositionOffset);
            targetSensitivity = sensitivity / distance;

            valueToAnalyze = localDirection;

            valueToAnalyze = valueToAnalyze.Remap(X: xAxis, Y: yAxis);

            horizontalWeight = Mathf.Min(targetSensitivity * Mathf.Abs(valueToAnalyze.x), BLENDSHAPE_MAX);
            verticalWeight = Mathf.Min(targetSensitivity * Mathf.Abs(valueToAnalyze.y), BLENDSHAPE_MAX);

            if (valueToAnalyze.x > 0) {
                eye.SetBlendShapeWeight(lookRightBlendshapeIndex, horizontalWeight);
            } else {
                eye.SetBlendShapeWeight(lookLeftBlendshapeIndex, horizontalWeight);
            }

            if (valueToAnalyze.y > 0) {
                eye.SetBlendShapeWeight(lookUpBlendshapeIndex, verticalWeight);
            } else {
                eye.SetBlendShapeWeight(lookDownBlendshapeIndex, verticalWeight);
            }
        }
    }

}