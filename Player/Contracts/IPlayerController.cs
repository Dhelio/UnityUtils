using Castrimaris.Animations.Contracts;
using Castrimaris.Interactables.Contracts;
using Castrimaris.Core;
using UnityEngine;

namespace Castrimaris.Player {

    /// <summary>
    /// Generic interface for player controllers to be implemented by the various controllers specific for each platform (e.g. VR, PC, Android, etc)
    /// </summary>
    public interface IPlayerController : IGameObjectSource {

        /// <summary>
        /// Wheter the user is currently locked in a vehicle
        /// </summary>
        public bool IsLockedToVehicle { get; set; }

        /// <summary>
        /// Wheter the user is currently locked from moving at all
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Generic interface for animator.
        /// </summary>
        public IAnimator Animator { get; }

        /// <summary>
        /// Interface representing interactables that can be held and used to interact with other interactables
        /// </summary>
        public IRaycastInteractor Interactor {set;}

        /// <summary>
        /// Point where to anchor <see cref="Interactor"/>
        /// </summary>
        public Transform InteractorAnchoring { get; }

        public void Lock() { IsLocked = true; }
        public void Unlock() { IsLocked = false; }
    }
}