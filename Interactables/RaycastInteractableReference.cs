using Castrimaris.Core.Extensions;
using Castrimaris.Player;
using UnityEngine;

namespace Castrimaris.Interactables {

    /// <summary>
    /// References a <see cref="GameObject"/> which implements a <see cref="IRaycastInteractable"/> component, so that the method <see cref="IRaycastInteractable.OnRaycasted"/> can be called
    /// </summary>
    public class RaycastInteractableReference : MonoBehaviour, IRaycastInteractable {

        [Header("References")]
        [SerializeField] private GameObject reference;

        private void Awake() {
            //Sanity checks
            if (reference == null)
                throw new MissingReferenceException($"No reference set for {this.gameObject.name} {nameof(RaycastInteractableReference)}. Assign one in Editor.");

            if (!reference.TryGetInterface<IRaycastInteractable>(out _))
                throw new MissingReferenceException($"No component with interface {nameof(IRaycastInteractable)} attached to {reference.name}.");
        }

        public void OnRaycasted(IPlayerController playerController) => reference.GetComponent<IRaycastInteractable>().OnRaycasted(playerController);
    }
}
