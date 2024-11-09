using UnityEngine;

namespace Castrimaris.Core.Utilities {

    /// <summary>
    /// Inheritable class to make reference components. Useful when checking collisions on child components of a root target.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ComponentReference<T> : MonoBehaviour where T : Component {

        [Header("References")]
        [SerializeField] private T reference;

        /// <summary>
        /// The reference to <see cref="T"/>. Note that it can only be set ONCE, as it is ReadOnly.
        /// </summary>
        public T Reference {
            get {
                return (reference == null) ? throw new MissingReferenceException("No reference set!") : reference;
            }

            set { //Setting the value this way makes this kind of a ReadOnly field that has lazy initialization
                if (reference != null) {
                    throw new MissingReferenceException("Tried to set an already set reference! It is ReadOnly, did you try to initialize it twice?");
                } else {
                    reference = value;
                }
            }
        }

    }

}
