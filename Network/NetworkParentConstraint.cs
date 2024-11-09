using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Castrimaris.Network {
    public class NetworkParentConstraint : NetworkBehaviour {

        [Header("Parameters")]
        [SerializeField] private bool isFollowing = true;
        [SerializeField] private bool followPosition = true;
        [SerializeField] private bool followRotation = true;

        [Header("References")]
        [SerializeField] private Transform target;

        public bool IsFollowing { get => isFollowing; set => isFollowing = value; }

        private bool previousKinematicValue = false;

        public Transform Target {
            set {
                //Set the object to kinematic if it's not already, because otherwise other clients will see the object move
                if (this.gameObject.TryGetComponent<Rigidbody>(out var rigidbody)) {
                    if (value == null)
                        rigidbody.isKinematic = previousKinematicValue;
                    else {
                        previousKinematicValue = rigidbody.isKinematic;
                        rigidbody.isKinematic = false;
                    }
                }
                target = value;
            }
        }

        private void Update() {
            if (!IsOwner)
                return;
            if (target == null)
                return;
            if (!isFollowing)
                return;

            if (followPosition)
                this.transform.position = target.position;
            if (followRotation)
                this.transform.rotation = target.rotation;
        }
    }
}
