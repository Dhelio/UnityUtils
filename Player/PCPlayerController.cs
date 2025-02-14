using Castrimaris.Animations.Contracts;
using Castrimaris.Attributes;
using Castrimaris.Core;
using Castrimaris.Core.Exceptions;
using Castrimaris.Core.Monitoring;
using Castrimaris.InputActions;
using Castrimaris.Interactables;
using Castrimaris.Interactables.Contracts;
using Castrimaris.UI;
using System.Linq;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

namespace Castrimaris.Player {

    /// <summary>
    /// Player controller for PC-based clients.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PCPlayerController : MonoBehaviour, IPlayerController, IDelayedInitializer {

        #region Private Variables

        [Header("Parameters")]
        [Tooltip("Maximum distance for raycasting operations, un Unity meters.")]
        [SerializeField, Range(.5f, 5f)] private float raycastMaxDistance = 1.5f;
        [Tooltip("How fast the player turns with the mouse look, both vertically and horizontally.")]
        [SerializeField, Range(1f, 150f)] private float turningSpeed = 1f;
        [Tooltip("How fast the player moves.")]
        [SerializeField, Range(1f, 50f)] private float movementSpeed = 1f;

        [Header("References")]
        [Tooltip("Reference to the animator interface.")]
        [SerializeField] private InterfaceReference<IAnimator> animator;
        [Tooltip("Reference to the teleporter interface.")]
        [SerializeField] private InterfaceReference<ITeleporter> teleporter;
        [Tooltip("Anchoring point for grabbable interactors")]
        [SerializeField] private Transform interactorTransform;

        [Header("ReadOnly Parameters")]
        [SerializeField, ReadOnly] private bool canTurn = true;

        private IRaycastInteractor interactor;
        private bool startedRaycastingWithInteractor = false;
        private PlayerActions playerActions;
        private new Camera camera;
        private CharacterController characterController;
        private Vector2 movement;
        private float cameraXRotation = 0f; //Variable that holds the current rotation of the Camera, since this controller supposes that the camera is separated from the CharacterController
        private GameObject dropInfo;
        #endregion

        #region Properties
        public bool IsLockedToVehicle { get; set; }
        public bool IsLocked { get; set; } = false;
        public IAnimator Animator => animator.Interface;
        public IRaycastInteractor Interactor { set => interactor = (value == null) ? null : interactor ?? value; } //Assign value only if the value is null OR if the interactor is null. This is to avoid assigning multiple interactors to players.
        public Transform InteractorAnchoring => interactorTransform;
        #endregion

        #region Public Methods

        public void InitializeGameObject(GameObject gameObject) {
            if (!gameObject.TryGetComponent<Tags>(out var tags))
                return;

            if (tags.Has("Drop Info")) {
                dropInfo = gameObject;
                dropInfo.SetActive(false);
                return;
            }
        }

        #endregion

        #region Unity Overrides
        private void Awake() {
            //Sanity Checks
            if (animator == null) throw new ReferenceMissingException(nameof(animator));
            if (teleporter == null) throw new ReferenceMissingException(nameof(teleporter));
            if (interactorTransform == null) throw new ReferenceMissingException(nameof(interactorTransform));

            //Initialization
            playerActions = new PlayerActions();
            camera = GetComponentInChildren<Camera>();
            characterController = GetComponent<CharacterController>();
        }

        private void Start() {
            UIManager.Instance.FadeOut();
        }

        private void Update() {
            Move();
            Animate();
            Raycast();
        }

        private void OnEnable() {
            Cursor.lockState = CursorLockMode.Locked;
            playerActions.PC.Direction.performed += HandleLook;
            playerActions.PC.Movement.performed += HandleMovement;
            playerActions.PC.Movement.canceled += HandleMovement;
            playerActions.PC.Exit.performed += HandleExit;
            playerActions.PC.Click.performed += HandleClick;
            playerActions.PC.Click.canceled += HandleClick;
            playerActions.PC.MouseLock.performed += HandleMouseLock;
            playerActions.PC.Sprint.performed += (_) => { movementSpeed *= 2; };
            playerActions.PC.Sprint.canceled += (_) => { movementSpeed /= 2; };
            playerActions.PC.Drop.performed += HandleDrop;

            playerActions.Enable();
        }

        private void OnDisable() {
            Cursor.lockState = CursorLockMode.None;
            playerActions.PC.Direction.performed -= HandleLook;
            playerActions.PC.Movement.performed -= HandleMovement;
            playerActions.PC.Exit.performed -= HandleExit;
            playerActions.PC.Click.performed -= HandleClick;
            playerActions.PC.Click.canceled -= HandleClick;
            playerActions.PC.MouseLock.performed -= HandleMouseLock;
            playerActions.PC.Sprint.performed -= (_) => { movementSpeed *= 2; };
            playerActions.PC.Sprint.canceled -= (_) => { movementSpeed /= 2; };
            playerActions.PC.Drop.performed -= HandleDrop;
            playerActions.Disable();
        }
        #endregion

        #region Private Methods

        private void HandleDrop(CallbackContext context) {  
            interactor?.Drop(this);
            dropInfo.SetActive(false);
        }

        private void HandleMouseLock(CallbackContext context) {
            Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.None : CursorLockMode.Locked;
            canTurn = (Cursor.lockState == CursorLockMode.Locked);
        }

        private void HandleClick(CallbackContext context) {
            //Pre-emptive reset
            startedRaycastingWithInteractor = false;

            if (!TryGetRaycastHits(out var hitInfos)) {
                return;
            }
            
            //TODO Debug pls remove
            //var hittedNames = System.String.Join(",",(from hit in hitInfos select hit.collider.name).ToArray());
            //Log.D($"Hitted objects {hittedNames}");

            //No interactor raycast
            if (interactor == null) {
                Log.D($"Detected raycasting without interactor. Executing raycasting.");
                //If no interactor is defined, then use the first interactable found
                var firstHitInfo = hitInfos.FirstOrDefault(hitInfo => hitInfo.collider.GetComponent<IRaycastInteractable>() != null);
                var interactable = (firstHitInfo.collider != null) ? firstHitInfo.collider.GetComponent<IRaycastInteractable>() : null;
                if (interactable != null) {
                    interactable.OnRaycasted(this);
                    var interactor = interactable.gameObject.GetComponent<IRaycastInteractor>();
                    if (interactor != null) {
                        dropInfo.SetActive(true);
                    } else {
                        Log.D($"No interactor found on {interactable.gameObject.name}, skipping enabling of drop info");
                    }
                }
                return;
            }

            //Interactor raycast
            if (context.performed) {
                Log.D($"Detected raycasting with interactor. Executing raycasting down.");
                startedRaycastingWithInteractor = true;
                interactor.OnRaycastingDown(hitInfos);
            } else if (context.canceled) {
                Log.D($"Detected raycasting with interactor. Executing raycasting up.");
                interactor.OnRaycastingUp(hitInfos);
            }
        }

        private void HandleExit(CallbackContext context) {
            Application.Quit();
        }

        private void HandleLook(CallbackContext context) {
            if (IsLocked)
                return;

            if (!canTurn)
                return;

            var directionDelta = context.ReadValue<Vector2>();

            var x = directionDelta.x * turningSpeed * Time.deltaTime;
            var y = directionDelta.y * turningSpeed * Time.deltaTime;

            var controllerRotation = characterController.transform.localEulerAngles;
            controllerRotation += Vector3.up * x;

            cameraXRotation -= y;
            cameraXRotation = Mathf.Clamp(cameraXRotation, -89f, 89f);

            camera.transform.localRotation = Quaternion.Euler(Vector3.right * cameraXRotation);
            characterController.transform.localEulerAngles = controllerRotation;
        }

        private void HandleMovement(CallbackContext context) {
            movement = context.ReadValue<Vector2>();
        }

        private void Move() {
            var movement = characterController.transform.forward * this.movement.y * Time.deltaTime * movementSpeed;
            movement += characterController.transform.right * this.movement.x * Time.deltaTime * movementSpeed;
            movement += Physics.gravity * Time.deltaTime;
            characterController.Move(movement);
        }

        private void Animate() {
            //Define animation variables
            var isMoving = (movement != Vector2.zero);

            //Animation
            animator.Interface.SetBool("VRIK_IsMoving", isMoving);
            animator.Interface.SetFloat("VRIK_Horizontal", movement.x * movementSpeed);
            animator.Interface.SetFloat("VRIK_Vertical", movement.y * movementSpeed);
        }

        private void Raycast() {
            if (interactor == null || !startedRaycastingWithInteractor)
                return;

            if (!TryGetRaycastHits(out var hitInfos))
                return;

            interactor.OnRaycasting(hitInfos);
        }

        private bool TryGetRaycastHits(out RaycastHit[] hitInfos) {
            //Retrieve hits from raycast
            var midScreen = new Vector2(Screen.width / 2, Screen.height / 2);
            var ray = Camera.main.ScreenPointToRay(midScreen);
            hitInfos = Physics.RaycastAll(ray, raycastMaxDistance);
            Debug.DrawRay(ray.origin, ray.direction, Color.yellow);
            return hitInfos.Length > 0;
        }

        #endregion
    }
}
