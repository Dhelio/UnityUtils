using Castrimaris.Player;

namespace Castrimaris.Interactables.Contracts {

    /// <summary>
    /// Interface used by components that implement some kind of behaviour implemented through raycasts. Mainly used in PC and Android applications.
    /// </summary>
    public interface IRaycastInteractor {
        public bool IsBeingUsed { get; }

        public void OnRaycastingDown(UnityEngine.RaycastHit[] hitInfos);
        public void OnRaycasting(UnityEngine.RaycastHit[] hitInfos);
        public void OnRaycastingUp(UnityEngine.RaycastHit[] hitInfos);
        public void Drop(IPlayerController playerController);
    }

}