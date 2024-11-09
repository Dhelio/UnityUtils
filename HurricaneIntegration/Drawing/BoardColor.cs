using Castrimaris.Attributes;
using Castrimaris.Core.Extensions;
using UnityEngine;

namespace Castrimaris.Interactables.Drawing {

    /// <summary>
    /// Simple controller used to change the color of the <see cref="PenController"/> when drawing on a <see cref="BoardController"/>
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BoardColor : MonoBehaviour {

        [Header("Parameters")]
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = Color.gray;

        [Header("ReadOnly References")]
        [SerializeField, ReadOnly] private SpriteRenderer circle;
        [SerializeField, ReadOnly] private BoardController boardController;

        public Color Color => this.GetComponent<MeshRenderer>().material.color;

        public void SetAsActiveColor() => boardController.SetActiveBoardColor(this);
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