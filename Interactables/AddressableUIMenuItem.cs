using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Castrimaris.Interactables {

    public class AddressableUIMenuItem : MonoBehaviour {

        #region Protected Variables
        [SerializeField] protected Button button;
        [SerializeField] protected Image image;
        #endregion

        #region Properties
        public UnityEvent onClick { get; private set; } = new UnityEvent();
        #endregion

        #region Public Methods
        public virtual void Initialize(UnityAction OnSelectedAction = null) {
            if (!this.TryGetComponent(out button)) {
                button = GetComponentInChildren<Button>();
                if (button == null) {
                    throw new MissingComponentException($"No component of type Button attached to this {nameof(AddressableUIMenuItem)} or its children!");
                }
            }
            if (!this.TryGetComponent(out image)) {
                image = GetComponentInChildren<Image>();
                if (image == null) {
                    throw new MissingComponentException($"No component of type Image attached to this {nameof(AddressableUIMenuItem)} or its children!");
                }
            }
            onClick.RemoveAllListeners();
            AddListener(OnSelectedAction);
        }

        public virtual void AddListener(UnityAction Action) {
            if (Action == null)
                return;
            onClick.AddListener(Action);
        }

        public virtual void OnClick() {
            onClick.Invoke();
        }
        #endregion

    }

}