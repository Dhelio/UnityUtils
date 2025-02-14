using Castrimaris.Animations.Contracts;
using Castrimaris.Core;
using Castrimaris.InputActions;
using Castrimaris.Interactables;
using Castrimaris.Interactables.Contracts;
using Castrimaris.IO.Contracts;
using Castrimaris.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

namespace Castrimaris.Player {

    [RequireComponent(typeof(CharacterController))]
    public class MobilePlayerController : MonoBehaviour, IPlayerController, IDelayedJoystickInitializer, IDelayedButtonInitializer {

        #region Private Variables

        [Header("Parameters")]
        [SerializeField, Range(1f, 150f)] private float movementSpeed = 1f;
        [SerializeField, Range(1, 150f)] private float turningSpeed = 1f;
        [SerializeField, Range(1f, 5f)] private float raycastMaxDistance = 1.5f;

        [Header("References")]
        [SerializeField] private InterfaceReference<IAnimator> animator;
        [Tooltip("Anchoring point for grabbable interactors")]
        [SerializeField] private Transform interactorAnchoring;

        private ITouchJoystick movementJoystick;
        private ITouchJoystick directionJoystick;
        private IRaycastInteractor interactor;

        private EventSystem eventSystem;
        private GraphicRaycaster uiGraphicRaycaster;
        private PlayerActions playerActions;
        private new Camera camera;
        private CharacterController characterController;
        private bool startedRaycastingWithInteractor = false;

        #endregion

        #region Properties

        public bool IsLockedToVehicle { get; set; }
        public bool IsLocked { get; set; }
        public bool IsInteracting { get; set; }
        public IAnimator Animator => animator.Interface;
        public IRaycastInteractor Interactor { set => interactor = (interactor == null) ? value : interactor; }
        public Transform InteractorAnchoring => interactorAnchoring;

        #endregion

        #region Public Methods

        public void InitializeJoystick(ITouchJoystick joystick) {
            var tags = joystick.gameObject.GetComponent<Tags>();
            if (tags.Has("Movement")) {
                movementJoystick = joystick;
                return;
            }

            if (tags.Has("Direction")) {
                directionJoystick = joystick;
                return;
            }
        }

        public void InitializeButton(Button button) {
            var tags = button.gameObject.GetComponent<Tags>();
            if (tags.Has("Drop")) {
                button.onClick.AddListener(HandleDrop);
                button.gameObject.SetActive(false);
                return;
            }
        }

        #endregion

        #region Unity Overrides

        private void Awake() {
            playerActions = new PlayerActions();
            camera = GetComponentInChildren<Camera>();
            characterController = GetComponent<CharacterController>();

            if (animator.Interface == null)
                throw new MissingReferenceException($"No reference set for interface {nameof(animator)}! Please set it in the Editor.");
        }

        private void Start() {
            eventSystem = FindObjectOfType<EventSystem>();
            uiGraphicRaycaster = FindObjectOfType<GraphicRaycaster>();
            UIManager.Instance.FadeOut();
        }

        private void OnEnable() {
            playerActions.Mobile.TouchPosition.performed += HandleTouches;
            playerActions.Enable();
        }

        private void OnDisable() {
            playerActions.Disable();
            playerActions.Mobile.TouchPosition.performed -= HandleTouches;
        }

        private void Update() {
            Move();
            Rotate();
            Animate();
        }

        #endregion

        #region Private Methods

        private void HandleDrop() => interactor?.Drop(this);

        private void Animate() {
            //Define animation variables
            var mv = movementJoystick.Vertical * -1;
            var mh = movementJoystick.Horizontal * -1;
            var dv = directionJoystick.Vertical * -1;
            var dh = directionJoystick.Horizontal * -1;
            var isMoving = (mv != 0 || mh != 0 || dv != 0 || dh != 0);

            //Local animation
            animator.Interface.SetBool("VRIK_IsMoving", isMoving);
            animator.Interface.SetFloat("VRIK_Horizontal", mh * movementSpeed);
            animator.Interface.SetFloat("VRIK_Vertical", mv * movementSpeed);
            animator.Interface.SetFloat("VRIK_Turn", dh);
        }

        private void Move() {
            if (movementJoystick == null)
                return;

            var vertical = movementJoystick.Vertical * -1;
            var horizontal = movementJoystick.Horizontal * -1;

            var movementDelta = characterController.transform.forward * vertical * Time.deltaTime * movementSpeed;
            movementDelta += characterController.transform.right * horizontal * Time.deltaTime * movementSpeed;
            movementDelta += Physics.gravity * Time.deltaTime; //This makes fall speed very very fast

            characterController.Move(movementDelta);
        }

        private void Rotate() {
            if (directionJoystick == null)
                return;

            if (directionJoystick.Vertical != 0) {
                //Camera
                var currentCameraLocalRotation = camera.transform.localEulerAngles;

                var cameraRotationDelta = new Vector3(directionJoystick.Vertical * turningSpeed * Time.deltaTime, 0, 0);

                currentCameraLocalRotation = currentCameraLocalRotation + cameraRotationDelta;

                camera.transform.localEulerAngles = currentCameraLocalRotation;
            }

            if (directionJoystick.Horizontal != 0) {
                //Controller
                var currentControllerLocalRotation = characterController.transform.localEulerAngles;

                var controllerRotationDelta = new Vector3(0, directionJoystick.Horizontal * turningSpeed * Time.deltaTime, 0);
                controllerRotationDelta = -controllerRotationDelta; //Somehow it's inverted, while camera isn't

                currentControllerLocalRotation = currentControllerLocalRotation + controllerRotationDelta;

                characterController.transform.localEulerAngles = currentControllerLocalRotation;
            }
        }

        private void HandleTouches(CallbackContext context) {
            var touchPosition = context.ReadValue<Vector2>();
            if (!TryGetRaycastHits(touchPosition, out var hitInfos))
                return;

            //No interactor raycast
            if (interactor == null) {
                //If no interactor is defined, then use the first interactable found
                var hitInfo = hitInfos.FirstOrDefault(hitInfo => hitInfo.collider.GetComponent<IRaycastInteractable>() != null);
                var interactable = (hitInfo.collider != null) ? hitInfo.collider.GetComponent<IRaycastInteractable>() : null;
                interactable?.OnRaycasted(this);
                return;
            }

            //Interactor raycast TODO maybe add multitouch support
            if (context.performed) {
                startedRaycastingWithInteractor = true;
                interactor.OnRaycastingDown(hitInfos);
            } else if (context.canceled) {
                interactor.OnRaycastingUp(hitInfos);
            }
        }

        private bool TryGetRaycastHits(Vector2 touchPosition, out RaycastHit[] hitInfos) {
            //Translate touch position to UI position
            var eventData = new PointerEventData(eventSystem) {
                position = touchPosition
            };

            //Skip interaction if user's touch is over any UI elements (this is because there could be UI commands like Joysticks.)
            var raycastResults = new List<RaycastResult>();
            uiGraphicRaycaster.Raycast(eventData, raycastResults);
            if (raycastResults.Count > 0) {
                hitInfos = null;
                return false;
            }

            //Raycast in world
            var ray = Camera.main.ScreenPointToRay(touchPosition);
            hitInfos = Physics.RaycastAll(ray, raycastMaxDistance);
            Debug.DrawRay(ray.origin, ray.direction, Color.yellow);
            return hitInfos.Length > 0;
        }

        #endregion
    }

}
