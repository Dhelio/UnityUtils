using Unity.Netcode;
namespace Castrimaris.Core.Utilities {
    /// <summary>
    /// Class that holds a reference to a <see cref="NetworkObject"/>. Useful when checking collisions on children of objects.
    /// </summary>
    public class NetworkObjectReference : ComponentReference<NetworkObject> { }
}