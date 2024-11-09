using Castrimaris.Attributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Castrimaris.Core
{
    [RequireComponent(typeof(Tags))]
    public class DelayedInitializer : MonoBehaviour {
        [Header("Parameters")]
        [SerializeField] private InitializationTypes initializationType = InitializationTypes.OnAwake;
        [SerializeField] private string tagToCheck = "PlayerController";

        private void Awake() {
            if (initializationType != InitializationTypes.OnAwake)
                return;

            Initialize();
        }

        private void Start() {
            if (initializationType != InitializationTypes.OnStart)
                return;

            Initialize();
        }

        private void Initialize() {
            var delayedInitializer = FindObjectsOfType<MonoBehaviour>().OfType<IDelayedInitializer>().First();
            delayedInitializer.InitializeGameObject(this.gameObject);
        }
    }
}
