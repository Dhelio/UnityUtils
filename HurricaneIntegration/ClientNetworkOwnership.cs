#if HVR_OCULUS

using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using Unity.Netcode;

namespace Castrimaris.Network {

    /// <summary>
    /// Requests and releases owneships of objects
    /// </summary>
    sealed public class ClientNetworkOwnership : NetworkBehaviour {

        public void RequestOwnership(HVRGrabberBase grabber, HVRGrabbable grabbable) => RequestOwnershipServerRpc();
        public void ReleaseOwnership(HVRGrabberBase grabber, HVRGrabbable grabbable) => ReleaseOwnershipServerRpc();

        [ServerRpc]
        private void RequestOwnershipServerRpc(ServerRpcParams serverRpcParams = default) {
            NetworkObject.ChangeOwnership(serverRpcParams.Receive.SenderClientId);
        }

        [ServerRpc]
        private void ReleaseOwnershipServerRpc(ServerRpcParams serverRpcParams = default) {
            NetworkObject.RemoveOwnership();
        }

    }

}

#endif