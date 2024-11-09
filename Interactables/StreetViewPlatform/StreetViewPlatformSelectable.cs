using UnityEngine;

namespace Castrimaris.Interactables {

    public class StreetViewPlatformSelectable : MonoBehaviour {

        [Header("Parameters")]
        [SerializeField] private string poiName = null;

        public string PoiName => poiName;

        public void Setup(string poiName) {
            this.poiName = poiName;
        }

    }

}