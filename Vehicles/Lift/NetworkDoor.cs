using Castrimaris.Interactables;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.Vehicles {

    [RequireComponent(typeof(NetworkAnimator))]
    public class NetworkDoor : NetworkBehaviour, IDoor {

        [Header("Events")]
        [SerializeField] private UnityEvent onOpen = new UnityEvent();
        [SerializeField] private UnityEvent onClose = new UnityEvent();

        private Animator animator;

        public bool IsOpen { get; }
        public UnityEvent OnOpen => onOpen;
        public UnityEvent OnClose => onClose;

        public void Open() {
            if (!IsServer)
                return;

            animator.SetBool("IsOpen", true);
            OnOpen.Invoke();
            OpenClientRpc();
        }

        public void Close() {
            if (!IsServer)
                return;

            animator.SetBool("IsOpen", false);
            OnClose.Invoke();
            CloseClientRpc();
        }

        private void Awake() {
            animator = GetComponent<Animator>();
        }

        [ClientRpc]
        private void OpenClientRpc() {
            OnOpen.Invoke();
        }

        [ClientRpc]
        private void CloseClientRpc() {
            OnClose.Invoke();
        }

    }

}
