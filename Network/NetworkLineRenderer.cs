using Castrimaris.Attributes;
using Castrimaris.Core.Monitoring;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.Network {

    /// <summary>
    /// Network implementation of the <see cref="LineRenderer"/> component.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkLineRenderer : NetworkBehaviour {

        #region Private Variables
        [Header("Parameters")]
        [SerializeField] private NetworkVariable<Color> startColor = new NetworkVariable<Color>(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        [SerializeField] private NetworkVariable<Color> endColor = new NetworkVariable<Color>(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        [SerializeField] private NetworkVariable<float> startWidth = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        [SerializeField] private NetworkVariable<float> endWidth = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        [SerializeField] private NetworkVariable<bool> isBaked = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [Header("ReadOnly Parameters")]
        [Tooltip("Networked list of positions relative to the line renderer.")]
        [SerializeField] private NetworkList<Vector3> positions;

        [Header("ReadOnly References")]
        [Tooltip("The line renderer this component uses directly.")]
        [SerializeField, ReadOnly] private LineRenderer line;
        [Tooltip("Optional mesh collider created when using the Bake() command")]
        [SerializeField, ReadOnly] private MeshCollider meshCollider = null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Changes the ownership of this <see cref="NetworkLineRenderer"/> back to the Server.
        /// </summary>
        public void ReleaseOwnership() {
            if (!IsOwner) {
                Log.W($"Tried to release ownership for this {nameof(NetworkLineRenderer)}, but the request didn't come from an Owner! Ignoring request...");
                return;
            }

            ReleaseOwnershipServerRpc();
        }

        /// <summary>
        /// Sets the parameter <see cref="LineRenderer.startWidth"/> network wide. Can only be called by the Owner of this <see cref="NetworkObject"/>.
        /// </summary>
        /// <param name="width">The width in Unity units.</param>
        public void SetStartWidth(float width) {
            if (!IsOwner) {
                Log.W($"Tried to set Start with for this {nameof(NetworkLineRenderer)}, but the request didn't come from an Owner! Ignoring request...");
                return;
            }

            startWidth.Value = width;
        }

        /// <summary>
        /// Sets the parameter <see cref="LineRenderer.endWidth"/> network wide. Can only be called by the Owner of this <see cref="NetworkObject"/>.
        /// </summary>
        /// <param name="width">The width in Unity units.</param>
        public void SetEndWidth(float width) {
            if (!IsOwner) {
                Log.W($"Tried to set End with for this {nameof(NetworkLineRenderer)}, but the request didn't come from an Owner! Ignoring request...");
                return;
            }

            endWidth.Value = width;
        }

        /// <summary>
        /// Sets the parameters <see cref="LineRenderer.startWidth"/> and <see cref="LineRenderer.endWidth"/> network wide. Can only be called by the Owner of this <see cref="NetworkObject"/>.
        /// </summary>
        /// <param name="width"></param>
        public void SetWidth(float width) {
            if (!IsOwner) {
                Log.W($"Tried to set width for this {nameof(NetworkLineRenderer)}, but the request didn't come from an Owner! Ignoring request...");
                return;
            }

            startWidth.Value = width;
            endWidth.Value = width;
        }

        /// <summary>
        /// Uses the command <see cref="LineRenderer.BakeMesh(Mesh, bool)"/> network wide. Can only be called by the Owner of this <see cref="NetworkObject"/>.
        /// </summary>
        public void Bake() {
            if (!IsOwner) {
                Log.W($"Tried to bake a mesh for this {nameof(NetworkLineRenderer)}, but the request didn't come from an Owner! Ignoring request...");
                return;
            }

            if (meshCollider != null) {
                Log.E($"Tried to bake a mesh for this {nameof(NetworkLineRenderer)}, but a mesh already exists!");
                return;
            }

            isBaked.Value = true;

            InternalBake();
            BakeServerRpc(OwnerClientId);
        }

        /// <summary>
        /// Adds a point to the <see cref="LineRenderer"/> network wide. Can only be called by the Owner of this <see cref="NetworkObject"/>.
        /// </summary>
        /// <param name="point">The coordinates of the new point.</param>
        public void AddPoint(Vector3 point) {
            if (!IsOwner) {
                Log.W($"Tried to add a point {point} to this {nameof(NetworkLineRenderer)}, but the request didn't come from an Owner! Ignoring request...");
                return;
            }

            InternalAddPoint(point);
            AddPointServerRpc(point, NetworkManager.Singleton.LocalClientId);
        }

        /// <summary>
        /// Remove a point from the <see cref="LineRenderer"/> network wide. Can only be called by the Owner of this <see cref="NetworkObject"/>.
        /// </summary>
        /// <param name="index">The index of the position to remove from the <see cref="LineRenderer"/></param>
        public void RemovePoint(int index) {
            if (!IsOwner) {
                Log.W($"Tried to remove the point at index {index} in this {nameof(NetworkLineRenderer)}, but the request didn't come from an Owner! Ignoring request...");
                return;
            }

            InternalRemovePoint(index);
            RemovePointServerRpc(index, NetworkManager.Singleton.LocalClientId);
        }

        public void SetStartColor(Color color) {
            if (!IsOwner) {
                Log.W($"Tried to set start color for this {nameof(NetworkLineRenderer)}, but the request didn't come from an Owner! Ignoring request...");
                return;
            }

            startColor.Value = color;
        }

        public void SetEndColor(Color color) {
            if (!IsOwner) {
                Log.W($"Tried to set end color for this {nameof(NetworkLineRenderer)}, but the request didn't come from an Owner! Ignoring request...");
                return;
            }

            endColor.Value = color;
        }

        /// <summary>
        /// Sets <see cref="LineRenderer.startColor"/> and <see cref="LineRenderer.endColor"/> network wide. Can only be called by the Owner of this <see cref="NetworkObject"/>.
        /// </summary>
        /// <param name="color">The color to apply.</param>
        public void SetColor(Color color) {
            if (!IsOwner) {
                Log.W($"Tried to set color for this {nameof(NetworkLineRenderer)}, but the request didn't come from an Owner! Ignoring request...");
                return;
            }

            startColor.Value = color;
            endColor.Value = color;
        }

        #endregion

        #region Unity Overrides

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();

            //Set colors
            line.startColor = startColor.Value;
            line.endColor = endColor.Value;

            //Set width
            line.startWidth = startWidth.Value;
            line.endWidth = endWidth.Value;

            //Set positions
            line.positionCount = positions.Count;
            for (int i = 0; i < positions.Count; i++) {
                line.SetPosition(i, positions[i]);
            }

            //Set bake
            if (isBaked.Value) {
                InternalBake();
            }
        }

        private void Reset() {
            line = GetComponent<LineRenderer>();
        }

        private void Awake() {
            line = GetComponent<LineRenderer>();

            positions = new NetworkList<Vector3>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); //TODO move to sync before awake, otherwise lines won't show for new connecting clients

            //Callbacks setup
            startColor.OnValueChanged += (_, newColor) => { line.startColor = newColor; };
            endColor.OnValueChanged += (_, newColor) => { line.endColor = newColor; };
            startWidth.OnValueChanged += (_, newWidth) => { line.startWidth = newWidth; };
            endWidth.OnValueChanged += (_, newWidth) => { line.endWidth = newWidth; };
        }

        #endregion

        #region Private Methods

        #region Internal Methods

        [ExposeInInspector]
        private void InternalBake() {
            var mesh = new Mesh();
            line.BakeMesh(mesh, true);
            if (mesh.vertices.Distinct().Count() < 3) {
                Destroy(this.gameObject);
                return;
            }
            meshCollider = this.gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            line.gameObject.layer = LayerMask.NameToLayer("DrawingBoardLine");
        }

        private void InternalAddPoint(Vector3 point) {
            line.positionCount += 1;
            line.SetPosition(line.positionCount - 1, point);

            if (IsOwner)
                positions.Add(point);
        }

        private void InternalRemovePoint(int index) {
            var positionsArray = new Vector3[line.positionCount];
            line.GetPositions(positionsArray);
            var updatedPositionsArray = positionsArray.Where((position, i) => i != index).ToArray();
            line.SetPositions(updatedPositionsArray);

            if (IsOwner)
                positions.RemoveAt(index);
        }

        #endregion

        #region Server RPCs

        [ServerRpc(RequireOwnership = true)] private void ReleaseOwnershipServerRpc() => this.NetworkObject.RemoveOwnership();

        [ServerRpc(RequireOwnership = true)]
        private void BakeServerRpc(ulong requestingClientId) {
            InternalBake();
            BakeClientRpc(requestingClientId);
        }

        [ServerRpc(RequireOwnership = true)]
        private void AddPointServerRpc(Vector3 point, ulong requestingClientId) {
            InternalAddPoint(point);
            AddPointClientRpc(point, requestingClientId);
        }

        [ServerRpc(RequireOwnership = true)]
        private void RemovePointServerRpc(int index, ulong requestingClientId) {
            InternalRemovePoint(index);
            RemovePointClientRpc(index, requestingClientId);
        }

        #endregion

        #region Client RPCs

        [ClientRpc]
        private void BakeClientRpc(ulong requestingClientId) {
            if (NetworkManager.Singleton.LocalClientId == requestingClientId)
                return; //Avoid double bake
            InternalBake();
        }

        [ClientRpc]
        private void AddPointClientRpc(Vector3 point, ulong requestingClientId) {
            if (NetworkManager.Singleton.LocalClientId == requestingClientId)
                return; //Avoid double point insertion
            InternalAddPoint(point);
        }

        [ClientRpc]
        private void RemovePointClientRpc(int index, ulong requestingClientId) {
            if (NetworkManager.Singleton.LocalClientId == requestingClientId)
                return; //Avoid double point removal
            InternalRemovePoint(index);
        }

        #endregion

        #endregion
    }
}
