using HurricaneVR.Framework.Components;
using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.HurricaneIntegration {

    /// <summary>
    /// Simple pressable button by the client
    /// </summary>
    [RequireComponent(typeof(HVRPhysicsButton))]
    [RequireComponent(typeof(NetworkTouchOwnership))]
    public class NetworkButton : NetworkBehaviour {}

}