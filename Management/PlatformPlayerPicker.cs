using Castrimaris.Core;
using Castrimaris.Core.Collections;
using Castrimaris.Core.Exceptions;
using Castrimaris.Core.Utilities;
using UnityEngine;

namespace Castrimaris.Player {

    /// <summary>
    /// Simple script used to spawn Player prefabs based on platform Runtime
    /// </summary>
    [DisallowMultipleComponent]
    public class PlatformPlayerPicker : SingletonMonoBehaviour<PlatformPlayerPicker> {

        [Header("References")]
        [SerializeField] private SerializableDictionary<RuntimePlatformTypes, GameObject> playerPrefabs = new SerializableDictionary<RuntimePlatformTypes, GameObject>();

        public GameObject GetPlayerPrefab() {
            var runtimePlatform = Utilities.GetRuntimePlatform();
            if (!playerPrefabs.TryGetValue(runtimePlatform, out var playerPrefab)) throw new ReferenceMissingException(nameof(runtimePlatform)+" "+nameof(playerPrefab));

            return playerPrefab;
        }

        public GameObject GetPlayerPrefab(RuntimePlatformTypes runtimePlatform) {
            if (!playerPrefabs.TryGetValue(runtimePlatform, out var playerPrefab)) throw new ReferenceMissingException(nameof(runtimePlatform) + " " + nameof(playerPrefab));

            return playerPrefab;
        }

    }
}
