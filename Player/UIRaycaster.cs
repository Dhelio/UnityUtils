using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Castrimaris.Player {

    [RequireComponent(typeof(LineRenderer))]
    public class UIRaycaster : MonoBehaviour {

        private bool isActive = false;

        public bool IsActive { 
            get {
                return isActive;
            } set {
                isActive = value;
                GetComponent<LineRenderer>().enabled = isActive;
            }
        
        }

        private void Update() {
            if (!isActive)
                return;
        }

    }

}
