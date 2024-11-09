using Castrimaris.Attributes;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Castrimaris.HurricaneIntegration {

    /// <summary>
    /// Simple script that syncs a transform based on another transform.
    /// </summary>
    public class TransformFollow : MonoBehaviour {

        [Header("References")]
        [SerializeField] private Transform transformReference;
        
        private void Update() {
            this.transform.position = transformReference.position;
            this.transform.rotation = transformReference.rotation;
        }

        [ExposeInInspector]
        [ContextMenu("Find Same")]
        private void FindSame() {
            var root = this.gameObject.transform.root;
            var transforms = root.MultipleRecursiveFind(this.gameObject.name);
            if (transforms.Length <= 1 || transforms.Length >= 3) {
                Log.E($"Can't find the same named transforms! Either there's just one, none or multiple transforms were found under root of {root.name} with the name '{this.gameObject.name}'. For the script to work they must be exactly two and named the same way.");
                return;
            }
            foreach (var t in transforms) {
                if (t == this) //Skip this and take the other one
                    continue;
                transformReference = t;
            }
        }
    }
}