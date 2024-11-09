using Castrimaris.Player;
using UnityEngine;

namespace Castrimaris.Management {

    [RequireComponent(typeof(PlatformPlayerPicker))]
    public class OfflineAvatarPicker : MonoBehaviour {

        private const string SPAWN_POINT_TAG = "Respawn";

        private void Start() {
            SpawnPlayer();
        }

        private void SpawnPlayer() {
            var playerPrefab = PlatformPlayerPicker.Instance.GetPlayerPrefab();
            var spawnPoint = GameObject.FindGameObjectWithTag(SPAWN_POINT_TAG);
            var player = GameObject.Instantiate(playerPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
        }
    }

}
