using Castrimaris.Core;
using Castrimaris.Core.Monitoring;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Castrimaris.Audio {

    /// <summary>
    /// Simple implementation of a spatial <see cref="AudioListener"/> with sounds coming from <see cref="SpatialEmitter">SpatialEmitters</see> that can be occluded by <see cref="SpatialOccluder">SpatialOccluders</see>
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(AudioListener))]
    [RequireComponent(typeof(Rigidbody))]
    public class SpatialListener : MonoBehaviour {

        private void Awake() {
            var collider = GetComponent<Collider>();
            collider.isTrigger = true;

            var rigidbody = GetComponent<Rigidbody>();
            rigidbody.isKinematic = false;
            rigidbody.useGravity = false;
            rigidbody.mass = 0;
            rigidbody.linearDamping = 0;
            rigidbody.angularDamping = 0;

            //TODO maybe detect collision only on layers?
        }

        private void OnTriggerEnter(Collider other) {
            Log.D($"{other.name}");
            if (!other.TryGetComponent<SpatialEmitter>(out var emitter))
                return;

            emitter.Activate(targetListener: this);
        }

        private void OnTriggerExit(Collider other) {
            if (!other.TryGetComponent<SpatialEmitter>(out var emitter))
                return;

            emitter.Deactivate();
        }

    }
}