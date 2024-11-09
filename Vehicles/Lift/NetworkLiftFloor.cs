using Castrimaris.Interactables;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.Vehicles {

    [RequireComponent(typeof(NetworkObject))]
    public class NetworkLiftFloor : NetworkBehaviour {

        [Header("Parameters")]
        [SerializeField] private int index = -1;

        [Header("References")]
        [SerializeField] private IDoor door;

        public UnityEvent OnLiftArrival = new UnityEvent();
        public UnityEvent OnLiftDeparture = new UnityEvent();

        public bool CanLiftLeave => !door.IsOpen;
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
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(Position, new Vector3(10f, .1F, 10F));
        }
    }

}
