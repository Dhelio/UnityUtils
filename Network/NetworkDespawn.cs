#if DOTWEEN

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.Network {

    /// <summary>
    /// Server-authoritative object despawn
    /// </summary>
    public class NetworkDespawn : NetworkBehaviour {

        [Header("Parameters")]
        [SerializeField] private float delay = 10.0f;
        [SerializeField] private float animationDuration = 2.0f;
        [SerializeField] private Ease animationEase = Ease.OutBounce;

        public override void OnNetworkSpawn() {
            if (!IsServer)
                return;

            StartCoroutine(Countdown());
        }

        private IEnumerator Countdown() {
            yield return new WaitForSeconds(delay);
            FadeOutClientRpc();
            var wfs = new WaitForSeconds(animationDuration * 1.3f); //We wait a bit more to give time to each client to correctly play the despawn animation
            FadeOutClientRpc();
            yield return wfs;
            NetworkObject.Despawn(true);
        }

        [ClientRpc]
        private void FadeOutClientRpc() {
            this.transform.DOScale(Vector3.zero, animationDuration).SetEase(animationEase);
        }

    }

}

#endif