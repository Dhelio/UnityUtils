using Castrimaris.Attributes;
using Castrimaris.Core.Monitoring;
using UnityEngine;

namespace Castrimaris.Core {

    /// <summary>
    /// A kind-of serializable interface method.
    /// </summary>
    /// <remarks>
    /// So, Unity simple and fast serialization system doesn't allow for interface serialization in Editor; this is due to the fact that Unity uses a different method of serialization
    /// for structs and classes (namely: for classes Unity serializes a reference, something like 7823487909835270; while for structs Unity serializes by value). If Unity was to serialize
    /// an inteface, then they wouldn't know if the interface was implemented in a struct or a class. The attribute <see cref="SerializeReference"/> kind of solves this, but not for
    /// interfaces implemented by <see cref="MonoBehaviour">MonoBehaviours</see>.
    /// By using a kind-of container that holds the interface reference we can hold the reference to that interface and assign it in the Editor, making all the assigning process more modular
    /// and avoiding to have hundreds of lines of null checks or searches in the hierarchy for references to the interfaces.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public sealed class InterfaceReference<T> : ISerializationCallbackReceiver where T : class {

        [SerializeField] private Object reference;

        public T Interface;

        public void OnBeforeSerialize() {
            if (reference != null && reference is not T) {
                Log.E($"Can't serialize reference because it doesn't implement an interface of type {typeof(T)}! Nulling reference.");
                reference = null;
            }
        }

        public void OnAfterDeserialize() {
            Interface = reference as T;
        }
        
    }
}
