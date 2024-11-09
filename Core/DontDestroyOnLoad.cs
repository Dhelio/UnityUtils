using UnityEngine;

namespace Castrimaris.Core {

    /// <summary>
    /// Simple script that sets a GameObject to not be destroyed on load.
    /// </summary>
    public class DontDestroyOnLoad : MonoBehaviour {
        private void Awake() {
            DontDestroyOnLoad(this.gameObject);
        }
    }

}
