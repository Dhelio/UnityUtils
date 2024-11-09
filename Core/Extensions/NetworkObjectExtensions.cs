using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.Core
{
    public static class NetworkObjectExtensions
    {

        /// <summary>
        /// Looks up the spawned objects dictionary in <see cref="NetworkManager"/> for the passed Id.
        /// </summary>
        /// <param name="networkObjectId">The id of the <see cref="NetworkObject"/> to search for in the <see cref="NetworkManager"/></param>
        /// <returns>The <see cref="NetworkObject"/> with the passed id if found, null otherwise</returns>
        public static NetworkObject FindNetworkObject(this ulong networkObjectId) => (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var result) ? result : null);

        /// <summary>
        /// Checks if the passed id is the same as the local client id contained in <see cref="NetworkManager"/>
        /// </summary>
        /// <param name="networkClientId">The client id to check against.</param>
        /// <returns>True if the id is of the local client, false otherwise</returns>
        public static bool IsLocalClient(this ulong networkClientId) => NetworkManager.Singleton.LocalClientId == networkClientId;

        /// <summary>
        /// Checks if the passed id is NOT the same as the local client id contained in <see cref="NetworkManager"/>
        /// </summary>
        /// <param name="networkClientId">The client id to check against.</param>
        /// <returns>True if the id is NOT of the local client, false otherwise</returns>
        public static bool IsNotLocalClient(this ulong networkClientID) => !networkClientID.IsLocalClient();

    }
}
