using UnityEngine;

namespace Castrimaris.Network {

    public static class NetworkExtensions {

        public static NetworkMode ToNetworkMode(this string stringNetworkMode) {
            switch (stringNetworkMode.ToLower()) {
                case "server":
                    return NetworkMode.SERVER;
                case "host":
                    return NetworkMode.HOST;
                case "client":
                    return NetworkMode.CLIENT;
                default:
                    Debug.LogError($"Fatal error while trying to convert string NetworkMode to enum: no NetworkMode for {stringNetworkMode}");
                    return NetworkMode.SERVER;
            }
        }

    }

}