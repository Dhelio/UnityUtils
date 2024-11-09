using Castrimaris.Attributes;
using Castrimaris.IO.Contracts;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Castrimaris.IO {

    [RequireComponent(typeof(Button))]
    public class ButtonDynamicInitializer : MonoBehaviour {
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
            var delayedInitializer = FindObjectsOfType<MonoBehaviour>().OfType<IDelayedButtonInitializer>().First();
            delayedInitializer.InitializeButton(this.GetComponent<Button>());
        }
    }
}