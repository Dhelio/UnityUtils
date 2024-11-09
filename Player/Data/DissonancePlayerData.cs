using Dissonance;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.Player.Data {

    public class DissonancePlayerData : MonoBehaviour, IDissonancePlayer {

        private Camera cameraReference;

        public string PlayerId => this.gameObject.name;

        public Vector3 Position => cameraReference.transform.position;

        public Quaternion Rotation => cameraReference.transform.rotation;

        public NetworkPlayerType Type => (GetComponent<NetworkObject>().IsLocalPlayer) ? NetworkPlayerType.Local : NetworkPlayerType.Remote;

        public bool IsTracking => true;

        private void Awake() {
            cameraReference = GetComponentInChildren<Camera>();
        }
    }

}
