using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.Core {
    [RequireComponent(typeof(Collider))]
    public class ProximityDetector : MonoBehaviour {

        #region Private Variables
        [Header("Parameters")]
        [SerializeField] protected string[] tagsToCheck = new string[] { "Player" };
        [SerializeField] protected UnityEvent onTriggerEntered = new UnityEvent();
        [SerializeField] protected UnityEvent onTriggerExited = new UnityEvent();
        #endregion

        #region Properties
        public UnityEvent OnTriggerEntered => onTriggerEntered;
        public UnityEvent OnTriggerExited => onTriggerExited;
        #endregion

        #region Unity Overrides
        private void Awake() {
            var collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        protected virtual void OnTriggerEnter(Collider other) {
            if (!other.TryGetComponent<Tags>(out var tags))
                return;

            if (!tags.Has(tagsToCheck))
                return;

            onTriggerEntered.Invoke();
        }

        protected virtual void OnTriggerExit(Collider other) {
            if (!other.TryGetComponent<Tags>(out var tags))
                return;

            if (!tags.Has(tagsToCheck))
                return;

            onTriggerExited.Invoke();
        }

        #endregion
    }
}
