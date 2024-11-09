using Castrimaris.Core;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Castrimaris.Player;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.Interactables {

    /// <summary>
    /// Simple networked controller for opening doors on triggers. Usually the trigger is based on player proximity
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NetworkAnimator))]
    [RequireComponent(typeof(NetworkObject))]
    public class ProximityDoor : NetworkBehaviour, IDoor {

        #region Private Variables

        [Header("Parameters")]
        [Tooltip("The tags to check on the colliding entity. Must be set in a Tags components.")]
        [SerializeField] List<string> tagsToCheck = new List<string>() { "Player" };
        [Tooltip("Wheter this door is locked or not.")]
        [SerializeField] private NetworkVariable<bool> isLocked = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        [Header("Events")]
        [Tooltip("Callbacks invoked on door opening")]
        [SerializeField] private UnityEvent onOpen = new UnityEvent();
        [Tooltip("Callbacks invoked on door closing")]
        [SerializeField] private UnityEvent onClose = new UnityEvent();

        private Animator animator; //The animator on the door for the opening animation

        #endregion

        #region Properties

        /// <summary>
        /// Callbacks invoked on door opening
        /// </summary>
        public UnityEvent OnOpen => onOpen;

        /// <summary>
        /// Callbacks invoked on door closing
        /// </summary>
        public UnityEvent OnClose => onClose;

        /// <summary>
        /// Wheter this door is open or not
        /// </summary>
        public bool IsOpen { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens the door
        /// </summary>
        public void Open() => OpenServerRpc();

        /// <summary>
        /// Closes the door
        /// </summary>
        public void Close() => CloseServerRpc();

        /// <summary>
        /// Locks the door in its current state
        /// </summary>
        public void Lock() => SetDoorLockServerRpc(true);

        /// <summary>
        /// Unlocks the door, permitting state change
        /// </summary>
        public void Unlock() => SetDoorLockServerRpc(false);

        #endregion

        #region Unity Overrides

        private void Awake() {
            animator = GetComponent<Animator>();
        }

        private void OnTriggerEnter(Collider other) {
            if (!CheckCollisionRequirements(other.gameObject, out _))
                return;

            Open();
        }

        private void OnTriggerExit(Collider other) {
            if (!CheckCollisionRequirements(other.gameObject, out _))
                return;

            Close();
        }

        #endregion

        #region Private Methods

        [ServerRpc(RequireOwnership = false)]
        private void SetDoorLockServerRpc(bool Value) => isLocked.Value = Value;

        [ServerRpc(RequireOwnership = false)]
        private void OpenServerRpc() {
            if (isLocked.Value)
                return;

            var nearPlayers = animator.GetInteger("NearPlayers");
            animator.SetInteger("NearPlayers", nearPlayers + 1);
            OnOpen.Invoke();
            OpenClientRpc();
        }

        [ClientRpc]
        private void OpenClientRpc() {
            OnOpen.Invoke();
        }

        [ServerRpc(RequireOwnership = false)]
        private void CloseServerRpc() {
            var nearPlayers = animator.GetInteger("NearPlayers");
            animator.SetInteger("NearPlayers", nearPlayers - 1);
            OnClose.Invoke();
            CloseClientRpc();
        }

        [ClientRpc]
        private void CloseClientRpc() => OnClose.Invoke();

        private bool CheckCollisionRequirements(GameObject target, out IEntity entity) {
            if (target.TryGetComponent<Core.Utilities.NetworkObjectReference>(out var reference)) {
                Log.D($"Found target reference");
                target = reference.Reference.gameObject;
            }

            if (!target.TryGetInterface<IEntity>(out entity)) {
                Log.D($"Could not find a component of type {nameof(IEntity)} on the collidee");
                return false;
            }

            if (!target.TryGetComponent<Tags>(out var tags)) {
                Log.D($"Could not find a component of type {nameof(Tags)} on the collidee");
                return false;
            }

            if (!tags.HasAny(tagsToCheck)) {
                Log.D($"Tags on the collidee do not match.");
                return false;
            }

            return true;
        }

        #endregion
    }
}
