using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.Interactables {

    public class TriggerButtonInteractable : MonoBehaviour {

        private bool isPressed = false;
        private AudioSource pressSound;
        private Vector3 originalPosition;
        private GameObject presser;

        public GameObject Presser { get { return presser; } }

        [Header("Parameters")]
        [SerializeField] private Vector3 pressedPosition = Vector3.zero;

        [Header("References")]
        [SerializeField] private GameObject buttonModel;

        [Header("Events")]
        public UnityEvent OnPress;
        public UnityEvent OnRelease;

        private void Awake() {
            pressSound = GetComponent<AudioSource>();
            originalPosition = buttonModel.transform.position;
        }

        private void OnTriggerEnter(Collider other) {
            if (!isPressed) {
                buttonModel.transform.position = pressedPosition;
                presser = other.gameObject;
                isPressed = true;
                OnPress.Invoke();
                pressSound.Play();
            }
        }

        private void OnTriggerExit(Collider other) {
            if (isPressed) {
                buttonModel.transform.position = originalPosition;
                isPressed = false;
                OnRelease.Invoke();
            }
        }

    }

}
