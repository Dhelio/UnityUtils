using Castrimaris.Attributes;
using Castrimaris.Core.Collections;
using Castrimaris.Network;
using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.Interactables.Drawing {

    /// <summary>
    /// Simple controller for controlling drawing with a <see cref="PenController"/>
    /// </summary>
    public class BoardController : NetworkBehaviour { //Even though we're not using netcode related functions here, we need this NetworkBehaviour to correctly reparent lines to the object, as seen in PenController

        [Header("References")]
        [SerializeField] private Collider boardCollider;

        [Header("ReadOnly References")]
        [SerializeField, ReadOnly] private BoardColor[] boardColors;
        [SerializeField, ReadOnly] private BoardSize[] boardSizes;

        public bool ActivateCollider { set => boardCollider.enabled = value; }

        private void Reset() {
            boardColors = this.transform.root.GetComponentsInChildren<BoardColor>();
            boardSizes = this.transform.root.GetComponentsInChildren<BoardSize>();
        }

        public void SetActiveBoardSize(BoardSize boardSize) {
            foreach (var size in boardSizes) {
                size.SetActiveState(false);
            }
            boardSize.SetActiveState(true);
        }

        public void SetActiveBoardColor(BoardColor boardColor) {
            foreach (var color in boardColors) {
                color.SetActiveState(false);
            }
            boardColor.SetActiveState(true);
        }

    }
}
