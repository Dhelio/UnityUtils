using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

namespace Castrimaris.Network {

    /// <summary>
    /// Client-authoritative network animator
    /// </summary>
    public class ClientNetworkAnimator : NetworkAnimator {
        protected override bool OnIsServerAuthoritative() {
            return false;
        }

    }

}
