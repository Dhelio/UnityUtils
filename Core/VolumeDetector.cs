using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.Core {

    [RequireComponent(typeof(SphereCollider))] //TODO make generic
    public class VolumeDetector : MonoBehaviour {
        #region Private Variables
        [Header("Parameters")]
        [SerializeField] protected string[] tagsToCheck = new string[] { "Player" };

        [Header("Events")]
        [SerializeField] protected UnityEvent onPresentInVolume = new UnityEvent();
        [SerializeField] protected UnityEvent onAbsentInVolume = new UnityEvent();

        private new SphereCollider collider;
        #endregion

        #region Properties
        public UnityEvent OnPresentInVolume => onPresentInVolume;
        public UnityEvent OnAbsentInVolume => onAbsentInVolume;
        #endregion

        public void Detect() {
            var colliders = Physics.OverlapSphere(collider.bounds.center, collider.radius);
            var isPresent = false;
            foreach (var collider in colliders) {
                var gameObject = collider.gameObject;
                if (!gameObject.TryGetComponent<Tags>(out var tags))
                    continue;
                if (!tags.Has(tagsToCheck))
                    continue;

                isPresent = true;
            }

            if (isPresent)
                onPresentInVolume.Invoke();
            else
                onAbsentInVolume.Invoke();
        }

        #region Unity Overrides
        private void Awake() {
            collider = GetComponent<SphereCollider>();
            collider.isTrigger = true;
        }
        
        #endregion
    }
}
