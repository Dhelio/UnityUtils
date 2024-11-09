using Castrimaris.Attributes;
using Castrimaris.IO.Contracts;
using System.Linq;
using UnityEngine;

namespace Castrimaris.IO {

    [RequireComponent(typeof(TouchJoystick))]
    public class JoystickDynamicInitializer : MonoBehaviour {

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
            var delayedInitializer = FindObjectsOfType<MonoBehaviour>().OfType<IDelayedJoystickInitializer>().First();
            delayedInitializer.InitializeJoystick(this.GetComponent<TouchJoystick>());
        }
    }
}
