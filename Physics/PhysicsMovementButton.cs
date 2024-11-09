using System;
using UnityEngine.Events;
using UnityEngine;
using System.Collections;

namespace Castrimaris.Physics {
    //TODO move to interactables
    /// <summary>
    /// Physic button that activates based on movement
    /// </summary>
    [RequireComponent(typeof(ConfigurableJoint))]
    [RequireComponent(typeof(BoxCollider))]
    public class PhysicsMovementButton : MonoBehaviour, IButton {

        [Header("Parameters")]
        [Tooltip("How much the button should travel before activating")]
        [SerializeField] float minimumTravelDistance = 0.1f;
        [Tooltip("In case of very small or very big distances, values can be multiplied by this value to make it easier to check distances")]
        [SerializeField] float magnifying = 1;
        [Tooltip("Optionally, a delay between each press/release cycle can be set to avoid multiple erroneous presses")]
        [Range(0, 2.0f)]
        [SerializeField] float activationsInterval = 0.0f;

        [SerializeField] private float DEBUG = 0.0f;

        private Vector3 originalPosition;
        private ConfigurableJoint joint;
        private bool hasBeenPressed = false;
        private bool canBeActivated = true;

        [Header("Callbacks")]
        public UnityEvent onPressed;
        public UnityEvent onRelease;

        #region PUBLIC METHODS

        public void OnPressed() {
            if (!hasBeenPressed) {
                hasBeenPressed = true;
                onPressed?.Invoke();
                if (activationsInterval > 0.0f) {
                    StartCoroutine(ActivationDelayBehaviour());
                }
            }
        }

        public void OnRelease() {
            if (hasBeenPressed) {
                hasBeenPressed = false;
                onRelease?.Invoke();
                if (activationsInterval > 0.0f) {
                    StartCoroutine(ActivationDelayBehaviour());
                }
            }
        }

        #endregion

        #region PRIVATE METHODS

        private IEnumerator ActivationDelayBehaviour() {
            canBeActivated = false;
            yield return new WaitForSeconds(activationsInterval);
            canBeActivated = true;
        }

        #endregion

        #region UNITY OVERRIDES

        private void Awake() {
            this.transform.localPosition = originalPosition;
        }

        private void FixedUpdate() {
            if (!canBeActivated)
                return;

            float distance = (Vector3.Distance(this.transform.localPosition, originalPosition)) * magnifying;

            DEBUG = distance;

            if (distance > minimumTravelDistance) {
                OnPressed();
            } else {
                OnRelease();
            }
        }

        #endregion

    }
}
