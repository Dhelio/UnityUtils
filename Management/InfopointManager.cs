using Castrimaris.UI;
using Castrimaris.UI.Contracts;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Castrimaris.Management {

    public class InfopointManager : MonoBehaviour {

        private bool isConnecting = false;

        public void FadeIn() => UIManager.Instance.FadeIn();

        public void ConnectAfter(float Seconds) {
            if (isConnecting)
                return;

            isConnecting = true;
            StartCoroutine(ConnectAfterBehaviour(Seconds));
        }

        private IEnumerator ConnectAfterBehaviour(float Seconds) {
            var wfs = new WaitForSeconds(Seconds);
            yield return wfs;
            InitializationManager.Instance.Connect();
        }

    }

}
