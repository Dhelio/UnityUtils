using Castrimaris.Core.Monitoring;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Castrimaris.Network {

    /// <summary>
    /// Client-authoritative Network Transform component
    /// </summary>
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform {

        public new void ApplyAuthoritativeState() => base.ApplyAuthoritativeState();

        protected override bool OnIsServerAuthoritative() {
            return false;
        }

        [ContextMenu("Copy Constraints from RigidBody")]
        private void CopyFromRigidBody() {
            var constraints = GetComponent<Rigidbody>().constraints;
            this.SyncPositionX = !(constraints.HasFlag(RigidbodyConstraints.FreezePositionX));
            this.SyncPositionY = !(constraints.HasFlag(RigidbodyConstraints.FreezePositionY));
            this.SyncPositionZ = !(constraints.HasFlag(RigidbodyConstraints.FreezePositionZ));
            this.SyncRotAngleX = !(constraints.HasFlag(RigidbodyConstraints.FreezeRotationX));
            this.SyncRotAngleY = !(constraints.HasFlag(RigidbodyConstraints.FreezeRotationY));
            this.SyncRotAngleZ = !(constraints.HasFlag(RigidbodyConstraints.FreezeRotationZ));
            this.SyncScaleX = false;
            this.SyncScaleY = false;
            this.SyncScaleZ = false;
        }
    }
}
