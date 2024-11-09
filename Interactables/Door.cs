using Castrimaris.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.Interactables {

    /// <summary>
    /// Simple animated door
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class Door : MonoBehaviour, IDoor {

        [Header("ReadOnly Parameters")]
        [ReadOnly, SerializeField] private bool isOpen = false;

        [Header("Events")]
        [SerializeField] private UnityEvent onOpen = new UnityEvent();
        [SerializeField] private UnityEvent onClose = new UnityEvent();

        private Animator animator;

        /// <inheritdoc cref="IDoor.IsOpen"/>
        public bool IsOpen => isOpen;

        /// <inheritdoc cref="IDoor.OnOpen"/>
        public UnityEvent OnOpen => onOpen;

        /// <inheritdoc cref="IDoor.OnClose"/>
        public UnityEvent OnClose => onClose;

        /// <inheritdoc cref="IDoor.Close"/>
        public void Close() {
            animator.SetBool("IsOpen", false);
            onClose.Invoke();
        }

        /// <inheritdoc cref="IDoor.Open"/>
        public void Open() {
            animator.SetBool("IsOpen", true);
            onOpen.Invoke();
        }

        private void Awake() {
            animator = GetComponent<Animator>();
        }
    }

}