using Castrimaris.Attributes;
using Castrimaris.Core;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Castrimaris.Core.Utilities;
using Castrimaris.HurricaneIntegration;
using Castrimaris.Interactables.Contracts;
using Castrimaris.Network;
using Castrimaris.Player;
using HurricaneVR.Framework.Components;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Castrimaris.Interactables.Drawing {

    /// <summary>
    /// Controller for the Pen in the drawing board interactables.
    /// Handles lines creation when drawing on a <see cref="BoardController"/>.
    /// Works in VR, PC and Android.
    /// </summary>
    [RequireComponent(typeof(NetworkParentConstraint))] //Required to correctly sync an Interactor over the network
    [RequireComponent(typeof(NetworkGrabbable))] //for VR
    [RequireComponent(typeof(NetworkSimpleSocketable))] //Required to place this Interactor in its socket
    public class PenController : NetworkBehaviour, IRaycastInteractable, IRaycastInteractor {

        #region Private Variables

        [Header("Parameters")]
        [Tooltip("The color of the next line to spawn")]
        [SerializeField] private NetworkVariable<Color> color = new NetworkVariable<Color>(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        [Tooltip("The size of the next eline to spawn")]
        [SerializeField] private NetworkVariable<float> size = new NetworkVariable<float>(0.005f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        [Tooltip("The minimum distance between the pen and the user controller to reach over which the pen will be disconnected from the drawing board")]
        [SerializeField] private float handToPenDistanceThreshold = .5f;

        [Header("References")]
        [SerializeField] private AssetReferenceGameObject lineAssetReference;
        [SerializeField] private GameObject linePrefab; //TODO either remove the asset reference or fix the error in referencing the Addressable
        [SerializeField] private Transform contactPoint;

        [Header("ReadOnly Parameters")]
        [SerializeField, ReadOnly] private NetworkVariable<bool> isDrawing = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        [SerializeField, ReadOnly] private NetworkVariable<bool> isBeingUsed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private NetworkLineRenderer currentLine = null;
        private IPlayerController playerController = null;
        private NetworkGrabbable networkGrabbable;
        private BoardController boardController = null;
        private Transform handTransform = null;
        private Transform controllerTransform = null;
        private Rigidbody handRigidbody = null;
        private HVRRigidBodyOverrides handRigidbodyOverrides = null;
        private Vector3 firstContactPoint = Vector3.negativeInfinity;
        private Vector3 latestPosition = Vector3.negativeInfinity;

        private float firstContactPointControllerZPositionRelativeToTheBoard = float.NegativeInfinity;
        private float firstContactPointHandZPositionRelativeToTheBoard = float.NegativeInfinity;

        private bool hasRequestedLineInstantiation = false; //HACK

        #endregion

        #region Properties

        public bool IsDrawing => isDrawing.Value;
        public bool IsBeingUsed => isBeingUsed.Value;
        public Vector3 TipPosition => contactPoint.position;

        #endregion

        #region Public Methods

        public void OnRaycasted(IPlayerController playerController) {
            if (isBeingUsed.Value)
                return;

            this.playerController = playerController;
            playerController.Interactor = this;
            playerController.Animator.SetBool("IsHelding", true);  //Grab this interactor

            //Set parent constraint
            var parentConstraint = this.gameObject.GetComponent<NetworkParentConstraint>();
            parentConstraint.Target = playerController.InteractorAnchoring;

            //Request ownership to the server.
            OnRaycastedServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        public void Drop(IPlayerController playerController) {
            isBeingUsed.Value = false;
            var parentConstraint = this.gameObject.GetComponent<NetworkParentConstraint>();
            parentConstraint.Target = null;
            playerController.Interactor = null;
            playerController.Animator.SetBool("IsHelding", false);  //Ungrab this interactor
        }

        public void OnRaycastingDown(RaycastHit[] hitInfos) {
            //Retrieve hit info
            var hits = from hit in hitInfos
                       where hit.collider.GetComponent<BoardColor>() != null
                       || hit.collider.GetComponent<BoardSize>() != null
                       || hit.collider.GetComponent<BoardController>() != null
                       || hit.collider.GetComponent<BoardController>() != null
                       select hit;

            if (hits.Count() <= 0) {
                Log.D($"Raycasted nothing.");
                return;
            }

            var hitInfo = hits.First();
            var collider = hitInfo.collider;
            if (collider.TryGetComponent<BoardColor>(out var boardColor)) {
                Log.D($"Raycasted {nameof(BoardColor)}");
                this.color.Value = boardColor.Color;
                boardColor.SetAsActiveColor();
                return;
            }
            if (collider.TryGetComponent<BoardSize>(out var boardSize)) {
                Log.D($"Raycasted {nameof(BoardSize)}");
                this.size.Value = boardSize.Size;
                boardSize.SetAsActiveSize();
                return;
            }
            if (collider.TryGetComponent<BoardController>(out var boardController)) {
                //Retrieve actual collision point with the trigger collider
                var boxCollider = collider.GetComponent<BoxCollider>();
                var closestPoint = boxCollider.ClosestPoint(hitInfo.point);
                //Spawn line
                Log.D($"Raycasted {nameof(BoardController)}");
                OnRaycastingDownServerRpc(NetworkManager.Singleton.LocalClientId, boardController.NetworkObjectId, closestPoint);
                return;
            }
            if (collider.TryGetComponent<NetworkSimpleSocket>(out var networkSimpleSocket)) {
                Log.D($"Raycasted {nameof(NetworkSimpleSocket)}");
                isBeingUsed.Value = false;
                var parentConstraint = this.gameObject.GetComponent<NetworkParentConstraint>();
                parentConstraint.Target = null;
                networkSimpleSocket.PlaceSocket(this.GetComponent<NetworkSimpleSocketable>());
                playerController.Interactor = null;
                playerController.Animator.SetBool("IsHelding", false);  //Ungrab this interactor
                return;
            }
        }

        public void OnRaycasting(RaycastHit[] hitInfos) {
            if (!IsDrawing)
                return;

            var hitInfo = hitInfos.FirstOrDefault(hitInfo => hitInfo.collider.GetComponent<BoardController>() != null);
            if (hitInfo.collider == null) {  //Didn't hit anything
                OnRaycastingUp(hitInfos); //If the raycasting doesn't continue on a suitable surface then we can assume that it has ended.
                return;
            }
            if (Vector3.Distance(latestPosition, hitInfo.point) < .01f)
                return;

            latestPosition = hitInfo.point;
            currentLine.AddPoint(hitInfo.point);
        }

        public void OnRaycastingUp(RaycastHit[] hitInfos) {
            if (!IsDrawing)
                return;

            var hitInfo = hitInfos.FirstOrDefault(hitInfo => hitInfo.collider.GetComponent<BoardController>() != null);
            if (hitInfo.collider == null) //Didn't hit anything
                return;

            currentLine.AddPoint(hitInfo.point);
            currentLine.Bake();

            currentLine = null;
            isDrawing.Value = false;
        }


        public void SaveHand(HVRGrabberBase grabber, HVRGrabbable grabbable) {
            //Sanity checks
            if (!grabber.TryGetComponent<TransformReference>(out var transformReference)) {
                Log.E($"Could not save hand that is missing {nameof(TransformReference)}!");
                return;
            }
            if (!grabber.TryGetComponent<Rigidbody>(out var rigidbody)) {
                Log.E($"Could not save hand that is missing {nameof(Rigidbody)}!");
                return;
            }
            if (!grabber.TryGetComponent<HVRRigidBodyOverrides>(out var overrides)) {
                Log.E($"Could not save hand that is missing {nameof(HVRRigidBodyOverrides)}!");
                return;
            }

            //Caching
            handTransform = grabber.transform;
            controllerTransform = transformReference.Reference;
            handRigidbody = rigidbody;
            handRigidbodyOverrides = overrides;
        }

        public void ForgetHand(HVRGrabberBase grabber, HVRGrabbable grabbable) {
            //Sanity checks
            if (handTransform == null ||
                controllerTransform == null ||
                handRigidbody == null ||
                handRigidbodyOverrides == null) {
                Log.E($"Can't forget an hand without having saved it first. Skipping.");
                return;
            }

            SetRigidbodyFreezeState(isFrozen: false);
        }

        #endregion

        #region Unity Overrides

        private void Awake() {
            networkGrabbable = GetComponent<NetworkGrabbable>();

            color.OnValueChanged += OnColorChanged;
        }

        private void FixedUpdate() {
            if (!IsOwner)
                return;

            //VR Only behaviour
            if (!IsDrawing)
                return;

            if (handTransform == null ||
                boardController == null ||
                controllerTransform == null)
                return;

            if (Vector3.Distance(handTransform.position, controllerTransform.position) > handToPenDistanceThreshold) {
                OnExitedTrigger();
            }

            var controllerPositionRelativeToTheBoard = boardController.transform.InverseTransformPoint(controllerTransform.position);
            var handPositionRelativeToTheBoard = boardController.transform.InverseTransformPoint(handTransform.position);
            if (controllerPositionRelativeToTheBoard.z > firstContactPointControllerZPositionRelativeToTheBoard) {
                handPositionRelativeToTheBoard.z = firstContactPointHandZPositionRelativeToTheBoard;
            } else if (!controllerPositionRelativeToTheBoard.z.IsBetween(firstContactPointControllerZPositionRelativeToTheBoard, firstContactPointControllerZPositionRelativeToTheBoard-.5f)) {
                OnExitedTrigger();
            }

            var handPositionGlobal = boardController.transform.TransformPoint(handPositionRelativeToTheBoard);
            handTransform.position = handPositionGlobal;
        }

        private void OnTriggerEnter(Collider collider) {
            //Drawing Boards tools checks
            if (!IsOwner ||
                !networkGrabbable.IsBeingHeld)
                return;

            //Swap color
            if (collider.TryGetComponent<BoardColor>(out var boardColor)) {
                Log.D($"Entered collision with {nameof(BoardColor)}");
                this.color.Value = boardColor.Color;
                boardColor.SetAsActiveColor();
                return;
            }

            //Swap size
            if (collider.TryGetComponent<BoardSize>(out var boardSize)) {
                Log.D($"Entered collision with {nameof(BoardSize)}");
                this.size.Value = boardSize.Size;
                boardSize.SetAsActiveSize();
                return;
            }

            //Drawing checks (VR Only Behaviour)
            if (hasRequestedLineInstantiation ||
                !Utilities.IsVrPlatform() ||
                !collider.TryGetComponent<BoardController>(out var boardController))
                return;

            //Save boardcontroller reference and the first contact point for use in FixedUpdate
            this.boardController = boardController;
            boardController.ActivateCollider = false;
            firstContactPoint = collider.ClosestPoint(TipPosition);
            firstContactPointControllerZPositionRelativeToTheBoard = boardController.transform.InverseTransformPoint(controllerTransform.position).z;
            firstContactPointHandZPositionRelativeToTheBoard = boardController.transform.InverseTransformPoint(handTransform.position).z;

            //Freeze hand rotation
            SetRigidbodyFreezeState(isFrozen: true);

            Log.D($"Board touch detected, requesting line instantiation.");
            hasRequestedLineInstantiation = true;
            OnTouchedDrawingBoardServerRpc(NetworkManager.Singleton.LocalClientId, boardController.NetworkObjectId, firstContactPoint);
        }

        private void OnTriggerStay(Collider collider) {
            //VR Only Behaviour
            if (!Utilities.IsVrPlatform() ||
                !IsOwner ||
                !isDrawing.Value ||
                !collider.TryGetComponent<BoardController>(out var boardController))
                return;

            var point = collider.ClosestPoint(TipPosition);
            if (Vector3.Distance(latestPosition, point) < .01f)
                return;

            point.z = firstContactPoint.z;
            latestPosition = point;
            currentLine.AddPoint(point);
        }

        private void OnTriggerExit(Collider collider) {
            //VR Only Behaviour
            if (!Utilities.IsVrPlatform() ||
                !IsOwner ||
                !isDrawing.Value ||
                !collider.TryGetComponent<BoardController>(out var boardController))
                return;

            OnExitedTrigger();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Callback to call on both Server and Clients when the color of the pen changes. Sets the colors on the pen so that's easy to tell what color is the pen drawing.
        /// </summary>
        /// <param name="oldColor">Old color of the pen (unused)</param>
        /// <param name="newColor">New color of the pen</param>
        private void OnColorChanged(Color oldColor, Color newColor) {
            var penColorMaterial = this.GetComponentInChildren<MeshRenderer>().materials.First(material => material.name.Contains("Pen_Color")); //Contains and not equals because the material will likely be instanced at runtime, so Unity will add " (Instanced)" to the material name
            penColorMaterial.color = newColor;
        }

        private void OnExitedTrigger() {
            //Bake line and give ownership to the server
            currentLine.Bake();
            currentLine.ReleaseOwnership();

            //Set a few flags and variables
            currentLine = null;
            isDrawing.Value = false;

            //Allow the hand to rotate again
            SetRigidbodyFreezeState(isFrozen: false);

            //Re-activate the collider
            boardController.ActivateCollider = true;
        }

        #region Server RPCs

        [ServerRpc(RequireOwnership = false)]
        private void OnRaycastedServerRpc(ulong requestingClientId) {
            ///OBSOLETE CODE
            /// Explanation: so, it just so happens that Netcode can't reparent a NetworkObject if not under another NetworkObject.
            /// So if you have a NetworkObject A that has a child transform B, you can't reparent NetworkObject C under B, but only under A.
            /// This means that we have to either make a chain of NetworkObjects that need to be spawned at runtime (absolutely not!) and reparented,
            /// OR "fake" the parenting by using a <see cref="UnityEngine.Animations.ParentConstraint"/> on the client side.
            /////Get Network Objects of the requesting client
            ///var requestingClientOwnedNetworkObjects = NetworkManager.Singleton.ConnectedClients[requestingClientId].OwnedObjects;
            ///
            /////Find the objects who has a child with a Tags component with a value of "Interactor Anchor"
            ///var ownedObjectWithAnchor = requestingClientOwnedNetworkObjects.First(networkObject => networkObject.GetComponentsInChildren<Tags>().First(tags => tags.Has("Interactor Anchor")));
            ///
            /////Retrieve the transform of the found anchor.
            ///var anchorTransform = ownedObjectWithAnchor.GetComponentsInChildren<Tags>().First(tags => tags.Has("Interactor Anchor")).transform;
            ///
            /////Actual reparenting
            ///if (!this.NetworkObject.TrySetParent(anchorTransform)) {
            ///    Log.E($"Fatal error while trying to set {anchorTransform.name} as parent for {this.name}!");
            ///}
            ///this.NetworkObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            isBeingUsed.Value = true;
            this.NetworkObject.ChangeOwnership(requestingClientId);

        }

        [ServerRpc(RequireOwnership = true)]
        private void OnRaycastingDownServerRpc(ulong requestingClientId, ulong boardNetworkId, Vector3 contactPoint) {
            Log.D($"Instantiating line at point {contactPoint} requested from client {requestingClientId} on board {boardNetworkId}.");
            var boardNetworkObject = boardNetworkId.FindNetworkObject();

            //HACK: on Windows spawning from Addressable AssetReference works, on Linux Server doesn't. Needs more investigation.
            var newLineGameObject = GameObject.Instantiate(linePrefab);
            var newLineNetworkObject = newLineGameObject.GetComponent<NetworkObject>();
            newLineNetworkObject.SpawnWithOwnership(requestingClientId);
            newLineNetworkObject.TrySetParent(boardNetworkObject);
            OnRaycastingDownClientRpc(requestingClientId, newLineNetworkObject.NetworkObjectId, contactPoint);
            
            //TODO: either remove this or make it work.
            //lineAssetReference.InstantiateAsync().Completed += (handle) => {
            //    var newLineGameObject = handle.Result;
            //    var newLineNetworkObject = newLineGameObject.GetComponent<NetworkObject>();
            //    newLineNetworkObject.SpawnWithOwnership(requestingClientId);
            //    newLineNetworkObject.TrySetParent(boardNetworkObject);
            //    OnRaycastingDownClientRpc(requestingClientId, newLineNetworkObject.NetworkObjectId, contactPoint);
            //};

        }

        [ServerRpc(RequireOwnership = true)] private void OnTouchedDrawingBoardServerRpc(ulong requestingClientId, ulong boardNetworkId, Vector3 contactPoint) => OnRaycastingDownServerRpc(requestingClientId, boardNetworkId, contactPoint);

        #endregion

        #region Client RPCs

        [ClientRpc]
        private void OnRaycastingDownClientRpc(ulong requestingClientId, ulong newLineNetworkId, Vector3 contactPoint) {
            if (requestingClientId.IsNotLocalClient())
                return;

            var newLineNetworkObject = newLineNetworkId.FindNetworkObject();
            isDrawing.Value = true;
            //Start updating the line
            currentLine = newLineNetworkObject.GetComponent<NetworkLineRenderer>();
            currentLine.AddPoint(contactPoint); //We add the same point so it makes a dot in case it's the single point of the line!
            currentLine.AddPoint(contactPoint);
            currentLine.SetColor(color.Value);
            currentLine.SetWidth(size.Value);

            hasRequestedLineInstantiation = false; //Reset flag
        }

        private void SetRigidbodyFreezeState(bool isFrozen) {
            var penRigidbody = this.GetComponent<Rigidbody>();
            if (isFrozen) {
                penRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                handRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                penRigidbody.drag = 2;
                handRigidbodyOverrides.OverrideRotation = false;
            } else {
                penRigidbody.constraints = RigidbodyConstraints.None;
                handRigidbody.constraints = RigidbodyConstraints.None;
                penRigidbody.drag = 0;
                handRigidbodyOverrides.OverrideRotation = true;
                handRigidbody.ResetInertiaTensor(); //Reset the tensor, otherwise the rotation will stay locked
            }
        }

        #endregion

        #endregion
    }
}