using Castrimaris.Player;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.Vehicles {

    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(NetworkObject))] //Required, because Entities must be reparented!
    public abstract class NetworkVehicle : NetworkBehaviour, IVehicle {

        #region Private Variables

        [Header(nameof(NetworkVehicle) + " Parameters")]
        [SerializeField] private bool shouldAttachEntityOnTrigger = false;
        [SerializeField] private bool shouldReparentEntity = false;
        [SerializeField] private bool shouldLockEntities = false;
        [SerializeField] private float entityAttachmentTime = 0.0f;

        protected HashSet<ulong> attachedEntities = new HashSet<ulong>();
        private float startCollisionTime = float.PositiveInfinity;

        #endregion

        #region Public Methods

        public virtual void AttachEntity(IEntity entity) {
            //TODO
        }

        public virtual void DetachEntity(IEntity entity) {
            //TODO
        }

        #endregion

        #region Unity Overrides

        protected virtual void OnCollisionEnter(Collision collision) {
            
        }

        protected virtual void OnCollisionStay(Collision collision) {
        
        }

        protected virtual void OnCollisionExit(Collision collision) {
        
        }

        #endregion

    }
}