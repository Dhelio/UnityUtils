using Castrimaris.Player.Contracts;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Castrimaris.Management {

    public class InfopointManager : MonoBehaviour {

        private bool isConnecting = false;

        public void ConnectAfter(float Seconds) {
            if (isConnecting)
                return;
            isConnecting = true;
            var fader = FindObjectsOfType<MonoBehaviour>().OfType<IFader>().First();
            fader.FadeIn();
            StartCoroutine(ConnectAfterBehaviour(Seconds));
        }

        private IEnumerator ConnectAfterBehaviour(float Seconds) {
            var wfs = new WaitForSeconds(Seconds);
            yield return wfs;
            InitializationManager.Instance.Connect();
        }

    }

}
