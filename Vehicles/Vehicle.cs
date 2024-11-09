using Castrimaris.Core;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Castrimaris.Core.Utilities;
using Castrimaris.Player;
using System.Collections.Generic;
using UnityEngine;

namespace Castrimaris.Vehicles {

    [RequireComponent(typeof(Collider))]
    public abstract class Vehicle : MonoBehaviour, IVehicle {

        #region PRIVATE VARIABLES

        [Header(nameof(Vehicle) + " Parameters")]
        [Tooltip("Wheter or not the entity collision should be checked on trigger rather than collision.")]
        [SerializeField] private bool shouldAttachEntityOnTrigger = false;
        [SerializeField] private bool shouldReparentEntity = false;
        [SerializeField] private bool shouldLockEntities = false;
        [Tooltip("After how long the colliding entity should be considered attached to the Vehicle.")]
        [Range(0.0f, 5.0f), SerializeField] private float entityAttachmentTime = 0.0f;
        [SerializeField] List<string> tagsToCheck = new List<string>();

        protected HashSet<ulong> attachedEntities = new HashSet<ulong>();

        private float startCollisionTime = float.PositiveInfinity;

        #endregion

        #region PUBLIC METHODS

        public virtual void AttachEntity(IEntity entity) {
            if (!attachedEntities.Add(entity.Id))
                return;

            if (shouldReparentEntity)
                entity.transform.SetParent(this.gameObject.transform);

            if (shouldLockEntities)
                entity.Lock();
        }

        public virtual void DetachEntity(IEntity entity) {
            if (!attachedEntities.Remove(entity.Id))
                return;

            if (shouldReparentEntity)
                entity.transform.SetParent(null);

            if (shouldLockEntities)
                entity.Unlock();
        }

        #endregion

        #region UNITY OVERRIDES

        protected virtual void OnCollisionEnter(Collision collision) {
            if (!CheckCollisionRequirements(collision, out var entity))
                return;

            startCollisionTime = Time.realtimeSinceStartup;

            if (entityAttachmentTime > 0.0f)
                return;

            AttachEntity(entity);
        }

        protected virtual void OnCollisionStay(Collision collision) {
            if (entityAttachmentTime <= 0.0f)
                return;

            if (!CheckCollisionRequirements(collision, out var entity))
                return;

            if ((Time.realtimeSinceStartup - startCollisionTime) > entityAttachmentTime) {
                AttachEntity(entity);
            }
        }

        protected virtual void OnCollisionExit(Collision collision) {
            if (!CheckCollisionRequirements(collision, out var entity))
                return;

            DetachEntity(entity);
        }

        protected virtual void OnTriggerEnter(Collider other) {
            if (!CheckTriggerRequirements(other, out var entity))
                return;

            startCollisionTime = Time.realtimeSinceStartup;

            if (entityAttachmentTime > 0.0f)
                return;

            AttachEntity(entity);
        }

        private void OnTriggerStay(Collider other) {
            if (entityAttachmentTime <= 0.0f)
                return;

            if (!CheckTriggerRequirements(other, out var entity))
                return;

            if ((Time.realtimeSinceStartup - startCollisionTime) > entityAttachmentTime) {
                AttachEntity(entity);
            }
        }

        protected virtual void OnTriggerExit(Collider other) {
            if (!CheckTriggerRequirements(other, out var entity))
                return;

            DetachEntity(entity);
        }

        protected virtual void Awake() {
            SetupVehicleCollisionDetection();
        }

        /// <summary>
        /// Checks if the trigger collision has all the requirements to be valid
        /// </summary>
        /// <param name="collider">The collider of the collision</param>
        /// <param name="entity">An eventual entity that will be returned from the collision</param>
        /// <returns>True if the collision has all the requirements, false otherwise</returns>
        protected bool CheckTriggerRequirements(Collider collider, out IEntity entity) {
            if (!shouldAttachEntityOnTrigger) {
                entity = null;
                return false;
            }

            return InternalCheckCollisionRequirements(collider.gameObject, out entity);
        }

        /// <summary>
        /// Checks if the physics collision has all the requirements to be valid
        /// </summary>
        /// <param name="collision">The collision</param>
        /// <param name="entity">An eventual entity that will be returned from the collision</param>
        /// <returns>True if the collision has all the requirements, false otherwise</returns>
        protected bool CheckCollisionRequirements(Collision collision, out IEntity entity) {
            if (shouldAttachEntityOnTrigger) {
                entity = null;
                return false;
            }

            return InternalCheckCollisionRequirements(collision.collider.gameObject, out entity);
        }
        #endregion

        #region PRIVATE METHODS

        private bool InternalCheckCollisionRequirements(GameObject target, out IEntity entity) {
            if (target.TryGetComponent<TransformReference>(out var reference)) {
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

        private void SetupVehicleCollisionDetection() {
            var collider = GetComponent<Collider>();

            collider.isTrigger = shouldAttachEntityOnTrigger;

            shouldAttachEntityOnTrigger = collider.isTrigger;
        }

        #endregion
    }
}