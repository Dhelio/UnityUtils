using Castrimaris.Core;
using Castrimaris.IO.Contracts;
using HurricaneVR.Framework.ControllerInput;
using UnityEngine;

namespace Castrimaris.HurricaneIntegration {

    /// <summary>
    /// Customization of the <see cref="HVRPlayerInputs"/> class from Hurricane; it mainly adds ways to control the inputs beside VR (meaning touch controls, keyboard, etc.)
    /// </summary>
    public class PlayerInputs : HVRPlayerInputs {

        [Header("PlayerInputs Parameters")]
        [SerializeField] private bool useTouch = true;
        [SerializeField] private RuntimePlatformTypes runtimePlatform = RuntimePlatformTypes.UNKNOWN;

        private IController controller = null;

        private void Start() {
            if (!this.TryGetComponent<IController>(out controller))
                controller = null;
        }

        protected override Vector2 GetMovementAxis() {
            if (controller != null)
                return controller.Movement;
            return base.GetMovementAxis();
        }

        protected override Vector2 GetTurnAxis() {
            if (controller != null)
                return controller.Direction;
            return base.GetTurnAxis();
        }

    }

}