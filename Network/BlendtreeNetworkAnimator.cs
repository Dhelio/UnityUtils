using Castrimaris.Animations.Contracts;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.Network {

    /// <summary>
    /// A kind-of <see cref="Unity.Netcode.Components.NetworkAnimator"/> made to also sync properties in blendtrees.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class BlendTreeNetworkAnimator : NetworkBehaviour, IAnimator {

        private Animator animator;

        public void SetBool(string parameterName, bool value) => SetBoolServerRpc(parameterName, value);

        public void SetFloat(string parameterName, float value) => SetFloatServerRpc(parameterName, value);

        private void Awake() {
            animator = GetComponent<Animator>();
        }

        [ServerRpc]
        private void SetBoolServerRpc(FixedString128Bytes parameterName, bool value) {
            InternalSetBool(parameterName, value);
            SetBoolClientRpc(parameterName, value);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetFloatServerRpc(FixedString128Bytes parameterName, float value) {
            InternalSetFloat(parameterName, value);
            SetFloatClientRpc(parameterName, value);
        }

        [ClientRpc] private void SetBoolClientRpc(FixedString128Bytes parameterName, bool value) => InternalSetBool(parameterName, value);
        [ClientRpc] private void SetFloatClientRpc(FixedString128Bytes parameterName, float value) => InternalSetFloat(parameterName, value);

        private void InternalSetBool(FixedString128Bytes parameterName, bool value) {
            var parameter = parameterName.ToString();
            animator.SetBool(parameter, value);
        }

        private void InternalSetFloat(FixedString128Bytes parameterName, float value) {
            var parameter = parameterName.ToString();
            animator.SetFloat(parameter, value);
        }
    }
}