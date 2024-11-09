using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Castrimaris.Network;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Castrimaris.Core.Editor {
    public class NetworkTools {

        [MenuItem("Tools/Castrimaris/Network/Generate Network Prefabs Lists")]
        public static void GeneratePefabsLists() {
            var prefabsList = new List<GameObject>();
            var prefabsGuids = AssetDatabase.FindAssets($"t:Prefab");
            foreach (var prefabGuid in prefabsGuids) {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGuid));
                if (!prefab.TryGetComponent<NetworkObject>(out _))
                    continue;
                prefabsList.Add(prefab);
            }

            var networkPrefabsList = ScriptableObject.CreateInstance<NetworkPrefabsList>();
            foreach (var prefab in prefabsList) {
                var networkPrefab = new NetworkPrefab();
                networkPrefab.Prefab = prefab;
                networkPrefabsList.Add(networkPrefab);
            }

            var savePath = $"Assets/ScriptableObjects/DefaultNetworkPrefabList.asset";
            AssetDatabase.Refresh();
            AssetDatabase.CreateAsset(networkPrefabsList, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/Castrimaris/Network/Set All NetworkTransforms to 'Use Unreliable Deltas'")]
        public static void SetAllNetworkTransformsToUseUnreliableDeltas() {
            var networkTransforms = GameObject.FindObjectsOfType<NetworkTransform>();
            foreach (var networkTransform in networkTransforms) {
                networkTransform.UseUnreliableDeltas = true;
            }

            Log.D($"Changed settings for {networkTransforms.Length} objects.");
        }

        [MenuItem("Tools/Castrimaris/Network/Set All NetworkTransforms to 'In Local Space'")]
        public static void SetAllNetworkTransformsToUseInLocalSpace() {
            var networkTransforms = GameObject.FindObjectsOfType<NetworkTransform>();
            foreach (var networkTransform in networkTransforms) {
                networkTransform.InLocalSpace = true;
            }

            Log.D($"Changed settings for {networkTransforms.Length} objects.");
        }

        [MenuItem("Tools/Castrimaris/Network/Set All Prefab ClientNetworkTransforms to 'Use Unreliable Deltas'")]
        public static void SetAllPrefabClientNetworkTransformsToUseUnreliableDeltas() {
            var prefabs = GetPrefabsWithComponent<ClientNetworkTransform>();

            foreach (var prefab in prefabs) {
                var clientNetworkTransforms = prefab.GetComponentsRecursive<ClientNetworkTransform>();
                foreach (var clientTransform in clientNetworkTransforms) {
                    clientTransform.UseUnreliableDeltas = true;
                }
                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/Castrimaris/Network/Set All Prefab ClientNetworkTransforms to 'In Local Space'")]
        public static void SetAllPrefabClientNetworkTransformsToUseInLocalSpace() {
            var prefabs = GetPrefabsWithComponent<ClientNetworkTransform>();

            foreach (var prefab in prefabs) {
                var clientNetworkTransforms = prefab.GetComponentsRecursive<ClientNetworkTransform>();
                foreach (var clientTransform in clientNetworkTransforms) {
                    clientTransform.InLocalSpace = true;
                }
                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
            }

            AssetDatabase.Refresh();
        }

        private static List<GameObject> GetPrefabsWithComponent<T>() {
            var prefabsList = new List<GameObject>();
            var prefabsGuids = AssetDatabase.FindAssets($"t:Prefab");
            foreach (var prefabGuid in prefabsGuids) {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGuid));
                if (!prefab.TryGetComponent<T>(out _)) {
                    if (!prefab.TryGetComponentInChildren<T>(out _))
                        continue;
                }
                prefabsList.Add(prefab);
            }
            return prefabsList;
        }
    }
}
