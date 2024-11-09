using Castrimaris.Attributes;
using System.Collections;
using UnityEngine;

namespace Castrimaris.Layouts {

    [DisallowMultipleComponent]
    public class Grid3DLayoutAnimator : MonoBehaviour {

        [Header("Parameters")]
        public AnimationCurve Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public float MovementTime = .5f;

        [Header("ReadOnly Parameters")]
        [ReadOnly][SerializeField] private bool isMoving = false;

        private Vector3 worldSpacePosition;
        private Coroutine movementCoroutine = null;

        private void Start() {
            worldSpacePosition = transform.position;
        }

        public void Move(Vector3 Position, bool IsWorldSpace = true) {
            if (!IsWorldSpace) {
                Position = this.transform.TransformPoint(Position);
            }
            //if (movementCoroutine != null) {
            //    Stop();
            //}
            movementCoroutine = StartCoroutine(MoveBehaviour(Position));
        }

        public void Stop() {
            if (movementCoroutine != null) {
                StopCoroutine(movementCoroutine);
                worldSpacePosition = transform.position;
            }
        }

        private IEnumerator MoveBehaviour(Vector3 WorldSpacePosition) {
            if (isMoving)
                yield break;

            isMoving = true;

            var elapsedTime = 0f;
            while (elapsedTime < MovementTime) {
                var normalizedPositionValue = Curve.Evaluate(elapsedTime / MovementTime);
                this.transform.position = Vector3.Lerp(worldSpacePosition, WorldSpacePosition, normalizedPositionValue);
                yield return null;
                elapsedTime += Time.deltaTime;
            }

            isMoving = false;
        }
    }

}