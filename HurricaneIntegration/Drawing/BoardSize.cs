using Castrimaris.Attributes;
using UnityEngine;

namespace Castrimaris.Interactables.Drawing {

    /// <summary>
    /// Simple controller used to change the size of the <see cref="PenController"/> <see cref="Castrimaris.Network.NetworkLineRenderer"/> when drawing on a <see cref="BoardController"/>
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BoardSize : MonoBehaviour {

        [Header("Parameters")]
        [SerializeField] private float size = 0.001f;
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = Color.gray;

        [Header("ReadOnly References")]
        [SerializeField, ReadOnly] private SpriteRenderer circle;
        [SerializeField, ReadOnly] private BoardController boardController;

        public float Size => size;

        public void SetAsActiveSize() => boardController.SetActiveBoardSize(this);
        public void SetActiveState(bool isActive) => circle.color = (isActive) ? activeColor : inactiveColor;

        private void Reset() {
            circle = GetComponentInChildren<SpriteRenderer>();
            boardController = this.transform.root.GetComponentInChildren<BoardController>();
        }

        private void Awake() {
            if (circle == null)
                circle = GetComponentInChildren<SpriteRenderer>();
            if (boardController == null)
                boardController = this.transform.root.GetComponentInChildren<BoardController>();
        }
    }
}