using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.Core
{
    public class ClientID : MonoBehaviour
    {
        public NetworkVariable<ulong> clientID = new NetworkVariable<ulong>(0);
        private bool isLocalPlayer = false;
        private void Start()
        {
            // set by client variable
            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
            {
                isLocalPlayer = true;
                SetClientID(NetworkManager.Singleton.LocalClientId);
            }
        }
        
        public void SetClientID(ulong id)
        {
            clientID.Value = id;
        }
        
        public ulong GetClientID()
        {
            return clientID.Value;
        }
        
        public bool IsLocalPlayer()
        {
            return isLocalPlayer;
        }
    }
}