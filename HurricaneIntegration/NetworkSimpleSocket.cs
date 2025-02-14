#if HVR_OCULUS

using Castrimaris.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.HurricaneIntegration {

    /// <summary>
    /// Class for handling simple socket operations.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Tags))]
    public class NetworkSimpleSocket : NetworkBehaviour {

        #region PRIVATE VARIABLES

        [Header("Parameters")]
        [Tooltip("The rotation the socketable should take when socketed")]
        [SerializeField] private Quaternion socketableRotation = Quaternion.identity;
        [Tooltip("The position the socketable should take when socketed")]
        [SerializeField] private Vector3 socketablePosition = Vector3.zero;
        [SerializeField] private NetworkVariable<bool> hasSocketable = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [Tooltip("Wheter the socketable should be reparented to this socket.")]
        [SerializeField] private bool reparentSocketable = false;
        [Tooltip("Wheter the socketable should be destroyed when enters this socket.")]
        [SerializeField] private bool destroySocketable = false;
        [Tooltip("If true, once socketed, the socketable won't be grabbable anymore")]
        [SerializeField] private bool socketableIsntRetrievable = false;
        [Tooltip("To be socketed, the socketable should be held by the Player, otherwise it will also attach if thrown")]
        [SerializeField] private bool socketableShouldBeHeld = false;
        [Tooltip("Wheter the socketable should be made kinematic once socketed")]
        [SerializeField] private bool socketableShouldBeKinematic = false;

        [Header("Events")]
        [Tooltip("Event fired when a Grabbable is socketed")]
        [SerializeField] private UnityEvent onSocketPlaced = new UnityEvent();
        [Tooltip("Event fired when a socketed Grabbable is removed")]
        [SerializeField] private UnityEvent onSocketRemoved = new UnityEvent();

        private NetworkSimpleSocketable socketedObject;

        #endregion

        #region PROPERTIES
        public bool SocketableShouldBeKinematic => socketableShouldBeKinematic;
        public UnityEvent OnSocketPlaced => onSocketPlaced;
        public UnityEvent OnSocketRemoved => onSocketRemoved;
        #endregion

        #region PUBLIC METHODS
        public void SaveSocketTransformValues(Transform transform) {
            socketableRotation = transform.rotation;
            socketablePosition = transform.position;
        }

        public void SaveSocketTransformValues((Vector3, Quaternion) values) {
            socketablePosition = values.Item1;
            socketableRotation = values.Item2;
        }

        public (Vector3, Quaternion) GetSocketableTransformValues() => (socketablePosition, socketableRotation);
        public void PlaceSocket(NetworkSimpleSocketable Socketable) => PlaceSocketServerRpc(Socketable.NetworkObjectId);
        public void RemoveSocket() => RemoveSocketServerRpc();

        #endregion

        #region UNITY OVERRIDES

        private void Awake() {
            GetComponent<Collider>().isTrigger = true;
        }

        #endregion

        #region PRIVATE METHODS

        [ServerRpc(RequireOwnership = false)]
        private void PlaceSocketServerRpc(ulong NetworkObjectId) {
            if (hasSocketable.Value)
                return;

            var socketableObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[NetworkObjectId];
            var socketable = socketableObject.GetComponent<NetworkSimpleSocketable>();

            if (socketableShouldBeHeld && !socketable.IsBeingHeld)
                return;

            socketableObject.RemoveOwnership();
            socketable.ForceRelease();

            if (socketableShouldBeKinematic)
                socketable.IsKinematic = true;

            socketableObject.transform.SetPositionAndRotation(socketablePosition, socketableRotation);

            if (reparentSocketable)
                socketableObject.TrySetParent(this.NetworkObject);

            if (socketableIsntRetrievable)
                socketable.Disable();

            if (destroySocketable) {
                socketableObject.Despawn(true);
            }

            if (!destroySocketable) {
                PlaceSocketClientRpc(NetworkObjectId);
                this.socketedObject = socketable;
                socketedObject.IsSocketed.Value = true;
            }

            OnSocketPlaced.Invoke();
            hasSocketable.Value = true;
        }

        [ClientRpc]
        private void PlaceSocketClientRpc(ulong networkObjectId) {
            var socketable = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId].GetComponent<NetworkSimpleSocketable>();
            this.socketedObject = socketable;
            if (socketableIsntRetrievable)
                socketable.Disable();
            OnSocketPlaced.Invoke();
        }

        [ServerRpc(RequireOwnership = false)]
        private void RemoveSocketServerRpc() {
            if (destroySocketable) //Can't remove that which doesn't exist.
                return;

            if (reparentSocketable) {
                socketedObject.NetworkObject.TryRemoveParent();
            }
            RemoveSocketClientRpc();
            socketedObject.IsSocketed.Value = false;
            OnSocketRemoved.Invoke();
            hasSocketable.Value = false;
            socketedObject = null;
        }

        [ClientRpc]
        private void RemoveSocketClientRpc() {
            OnSocketRemoved.Invoke();
        }

        #endregion
    }
}

#endif