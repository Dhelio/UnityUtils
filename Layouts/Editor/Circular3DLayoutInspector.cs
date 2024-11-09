#if UNITY_EDITOR

using UnityEngine;

namespace Castrimaris.Layouts {

    [ExecuteInEditMode]
    public partial class Circular3DLayout : Base3DLayout {

        #region Private Fields

        [Header("Editor Parameters")]
        [SerializeField]
        private bool shouldUpdateInEditor = false;

        #endregion Private Fields

        #region Protected Methods

        protected void EditorUpdate() {
            if (shouldUpdateInEditor)
                UpdateLayout();
        }

        #endregion Protected Methods
    }
}

#endif