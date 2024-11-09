using UnityEngine;

namespace Castrimaris.Core {

    /// <summary>
    /// Interface that provides properties for accessing <see cref="MonoBehaviour"/>'s <see cref="GameObject"/> and <see cref="Transform"/> from the classes implementing this interface.
    /// The intention for this interface is to be inherited by other interfaces, so that we can retrieve local objects from generic interfaces.
    /// </summary>
    public interface IGameObjectSource {
        public GameObject gameObject { get; }
        public Transform transform { get; }
        public T GetComponent<T>();
        public bool TryGetComponent<T>(out T component);
    }
}
