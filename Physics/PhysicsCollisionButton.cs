using System;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.Physics {
    //TODO move to interactables
    /// <summary>
    /// Physic button that activates based on collisions or triggers.
    /// </summary>
    [RequireComponent(typeof(ConfigurableJoint))]
    [RequireComponent(typeof(Collider))]
    [Obsolete("Use the prefab with the Hurricane button instead.")]
    public class PhysicsCollisionButton : MonoBehaviour, IButton {

        [Header("Parameters")]
        [Tooltip("Wheter the button should collide on a trigger or a collider.\n" +
            "The activating collider is always the connectedBody of the ConfigurableJoint.")]
        [SerializeField] private bool shouldCollideOnTrigger = false;
        [Tooltip("Optionally, a delay between each press/release cycle can be set to avoid multiple erroneous presses")]
        [Range(0, 2.0f)]
        [SerializeField] float activationsInterval = 0.0f;

        private ConfigurableJoint joint;
        private bool hasBeenPressed = false;

        [Header("Callbacks")]
        public UnityEvent onPressed;
        public UnityEvent onRelease;

        #region PUBLIC METHODS

        public void OnPressed() {
            if (!hasBeenPressed) {
                hasBeenPressed = true;
                onPressed?.Invoke();
            }
        }

        public void OnRelease() {
            if (hasBeenPressed) {
                hasBeenPressed = false;
                onRelease?.Invoke();
            }
        }

        #endregion

        #region UNITY OVERRIDES

        private void Awake() {
            joint = GetComponent<ConfigurableJoint>();
        }

        private void OnTriggerEnter(Collider collider) {

            if (shouldCollideOnTrigger) {
                if (collider.gameObject == joint.connectedBody.gameObject) {
                    OnPressed();
                }
            }
        }

        private void OnTriggerExit(Collider collider) {
            if (shouldCollideOnTrigger) {
                if (collider.gameObject == joint.connectedBody.gameObject) {
                    OnRelease();
                }
            }
        }

        private void OnCollisionEnter(Collision collision) {
            Debug.Log($"Collided with {collision.gameObject.name}");
            if (!shouldCollideOnTrigger) {
                if (collision.collider.gameObject == joint.connectedBody.gameObject) {
                    OnPressed();
                }
            }
        }

        private void OnCollisionExit(Collision collision) {
            if (!shouldCollideOnTrigger) {
                if (collision.collider.gameObject == joint.connectedBody.gameObject) {
                    OnRelease();
                }
            }
        }

    }

    #endregion

}