using Castrimaris.Interactables;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Castrimaris.Vehicles {

    public class LiftFloor : MonoBehaviour {

        [Header("Editor Parameters")]
        [SerializeField] private bool enableDebugVisualization = false;

        [Header("Parameters")]
        [SerializeField] private int index = -1;

        [Header("References")]
        [SerializeField] private IDoor door;

        public UnityEvent OnLiftArrival = new UnityEvent();
        public UnityEvent OnLiftDeparture = new UnityEvent();

        public int Index => index;
        public Vector3 Position => transform.position;

        private void Awake() {
            if (!this.gameObject.TryGetComponent<IDoor>(out door))
                door = GetComponentInChildren<IDoor>();
        }

        private void Reset() {
            index = this.transform.GetSiblingIndex();

            if (!this.gameObject.TryGetComponent<IDoor>(out door))
                door = GetComponentInChildren<IDoor>();
        }

        private void OnDrawGizmosSelected() {
            if (!enableDebugVisualization)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(Position, new Vector3(10f, .1F, 10F));
        }

    }

}