using UnityEngine;

namespace Castrimaris.Layouts {

    [ExecuteAlways, DisallowMultipleComponent]
    public abstract class Base3DLayout : MonoBehaviour, ILayout {

        #region Public Methods

        public virtual void UpdateLayout() {
        }

        public virtual int Count() {
            return this.transform.childCount;
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void Awake() {
        }

        protected virtual void Update() {
        }

        #endregion Protected Methods
    }
}