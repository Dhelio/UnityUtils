using Castrimaris.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.Network {
    [RequireComponent(typeof(NetworkManager))]
    public class ClientDisconnectionHandler : MonoBehaviour {

        private void Awake() {
            GetComponent<NetworkManager>().OnClientDisconnectCallback += OnClienDisconnectCallback; //we do GetComponent because the NetworkManager may or not have its singleton initialized
        }

        private void OnClienDisconnectCallback(ulong clientId) {
            if (clientId == NetworkManager.Singleton.LocalClientId) {
                UIManager.Instance.DisplayUrgentMessage("Fatal error: could not connect to the server! Please, reboot the application.");
            }
        }
    }
}
