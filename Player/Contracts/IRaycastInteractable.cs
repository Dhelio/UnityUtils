using Castrimaris.Core;
using Castrimaris.Player;

namespace Castrimaris.Interactables {

    /// <summary>
    /// Interface for all interactables that can be used with raycasting in PC and Android applications..
    /// </summary>
    public interface IRaycastInteractable : IGameObjectSource {

        public void OnRaycasted(IPlayerController playerController);

    }

}
