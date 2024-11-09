using UnityEngine.Events;

namespace Castrimaris.Interactables {

    /// <summary>
    /// General interface for door <see cref="UnityEngine.GameObject">GameObjects</see>
    /// </summary>
    public interface IDoor {

        //TODO OnLatched, OnUnlatched, IsLatched, ...

        /// <summary>
        /// Wheter the door is open or not.
        /// </summary>
        public bool IsOpen { get; }

        /// <summary>
        /// Event called on door opening
        /// </summary>
        public UnityEvent OnOpen { get; }

        /// <summary>
        /// Event called on door closing
        /// </summary>
        public UnityEvent OnClose { get; }

        /// <summary>
        /// Opens the door
        /// </summary>
        public void Open();

        /// <summary>
        /// Closes the door
        /// </summary>
        public void Close();

    }

}