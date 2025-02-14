using Castrimaris.Animations.Contracts;
using Castrimaris.Core;
using Castrimaris.Interactables.Contracts;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Player;
using UnityEngine;

namespace Castrimaris.Player {

    public class VRPlayerController : HVRPlayerController, IPlayerController {

        #region Private Variables
        [Header(nameof(VRPlayerController) + " Parameters")]
        [Tooltip("Base class HVRPlayerController uses only the Y component to rotate the player when using Smooth Rotation; by enabling this flag we can force the system to use also the X component. Useful for controllers different from VR (e.g. Mouse and Keyboard)")]
        [SerializeField] private bool useFullVectorForSmoothRotation = false;

        [Header(nameof(VRPlayerController) + "References")]
        [SerializeField] private InterfaceReference<IAnimator> animator;
        #endregion

        #region Properties
        public bool IsLockedToVehicle { get; set; }
        public bool IsLocked { get; set; }
        public bool IsInteracting { get; set; }
        public IAnimator Animator => animator.Interface;
        public IRaycastInteractor Interactor { set => throw new System.NotImplementedException($"This should never be called."); }
        public Transform InteractorAnchoring => throw new System.NotImplementedException($"This should never be called.");
        #endregion

        #region Unity Overrides
        protected override void Start() {
            base.Start();
            HVRManager.Instance.PlayerController = this;
        }
        #endregion

        #region Protected Methods
        protected override void HandleSmoothRotation() {
            if (useFullVectorForSmoothRotation) {
                var input = GetTurnAxis().y;
                if (System.Math.Abs(input) < SmoothTurnThreshold)
                    return;

                var rotation = input * SmoothTurnSpeed * Time.deltaTime * -1;
                var x = Camera.transform.localEulerAngles.x + rotation;
                var rotationVector = new Vector3(x, 0 , 0);
                Camera.transform.localRotation = Quaternion.Euler(rotationVector);
            }
            base.HandleSmoothRotation();
        }
        #endregion
    }
}