using System;
using Castrimaris.Core.Exceptions;
using Castrimaris.Core.Monitoring;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Castrimaris.Core {

    /// <summary>
    /// A kind-of serializable interface class.
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

        #region  Public Variables

        /// <summary>
        /// The interface referenced.
        /// </summary>
        public T Interface;

        #endregion

        #region  Private Variables

        [SerializeField] private Object reference;

        #endregion

        #region  Properties

        /// <summary>
        /// The <see cref="GameObject"/> that the interface is assigned to. It's valid ONLY if the interface inherits from <see cref="IGameObjectSource"/>.
        /// </summary>
        public GameObject gameObject {
            get {
                if (Interface is IGameObjectSource gameObjectSource)
                    return gameObjectSource.gameObject;
                else
                    throw new TypeMismatchException($"Cannot obtain gameObject from Interface {typeof(T)} because it doesn't implement {nameof(IGameObjectSource)}!");
            }
        }

        #endregion

        #region  Constructors

        public InterfaceReference(GameObject referenceGameObject) {
            if (referenceGameObject == null)
                throw new NullReferenceException($"No reference passed to the constructor for {nameof(referenceGameObject)}.");
            if (!referenceGameObject.TryGetComponent<T>(out var component)) {
                throw new NullReferenceException($"No reference found for component of type {typeof(T)} on {referenceGameObject.name}.");
            }

            reference = component as Object;
            Interface = reference as T;
        }

        public InterfaceReference(T @interface) {
            if (@interface == null)
                throw new NullReferenceException($"No reference passed to the constructor for the interface of type {typeof(T)}.");
            if (@interface is not IGameObjectSource gameObjectSource)
                throw new InvalidCastException($"Can't assign interface of type {typeof(T)} to this {nameof(InterfaceReference<T>)} because it doesn't implement {nameof(IGameObjectSource)}! Assign a GameObject instead.");
            if (!gameObjectSource.TryGetComponent<T>(out var component))
                throw new NullReferenceException($"No reference found for component of type {typeof(T)} on {gameObjectSource.gameObject.name}");

            reference = component as Object;
            Interface = reference as T;
        }

        #endregion

        #region Operators

        public static implicit operator InterfaceReference<T>(T @interface) => new InterfaceReference<T>(@interface);
        public static implicit operator T(InterfaceReference<T> interfaceReference) => interfaceReference.Interface;

        #endregion

        #region Public Methods

        public void OnBeforeSerialize() {
            //Serialization if we're passing a GameObject on the reference field in the Inspector
            if (reference != null && reference is GameObject) {
                var gameObject = reference as GameObject;
                var component = gameObject.GetComponent<T>();
                if (component == null) {
                    Log.E($"Can't serialize reference because it doesn't implement an interface of type {typeof(T)} on the GameObject {gameObject.name}! Nulling reference.");
                    reference = null;
                    return;
                } else {
                    reference = component as Object;
                    return;
                }
            }

            //Serialization if we're passing the component implementing the interface itself
            if (reference != null && reference is not T) {
                Log.E($"Can't serialize reference because it doesn't implement an interface of type {typeof(T)}! Nulling reference.");
                reference = null;
            }
        }

        public void OnAfterDeserialize() {
            Interface = reference as T;
        }

        #endregion
    }
}
