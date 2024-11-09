using Castrimaris.Core.Monitoring;
using Castrimaris.Network;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.Management {

    public class ServerManager : MonoBehaviour {

        private ClientNetworkTransform[] networkTransforms;

        private void Start() {
            if (!NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost) {
                Log.D("Client instance found, destroying manager gameobject...");
                Destroy(this.gameObject);
                return;
            }

            //networkTransforms = FindObjectsOfType<ClientNetworkTransform>();
            //Log.D($"Found {networkTransforms.Length} objects with {nameof(ClientNetworkTransform)}");
            //NetworkManager.Singleton.OnClientConnectedCallback += ForceUpdate;
        }

        private async void ForceUpdate(ulong clientId) {
            foreach (var networkTransform in networkTransforms) {
                networkTransform.UseUnreliableDeltas = !networkTransform.UseUnreliableDeltas;
                await Task.Delay(50);
                networkTransform.UseUnreliableDeltas = !networkTransform.UseUnreliableDeltas;
            }
            //for (int i = 5; i > 0; i--) {
            //    Log.D($"{i}...");
            //    await Task.Delay(1000);
            //}
            //Log.D($"Sync!");
            //foreach (var networkTransform in networkTransforms) {
            //    networkTransform.ApplyAuthoritativeState();
            //}
        }
    }
}
