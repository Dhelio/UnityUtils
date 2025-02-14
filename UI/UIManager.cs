using Castrimaris.Core;
using Castrimaris.Core.Monitoring;
using Castrimaris.Core.Utilities;
using Castrimaris.UI.Contracts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace Castrimaris.UI {

    public class UIManager : SingletonMonoBehaviour<UIManager> {

        #region Private Variables

        [Header("References")]
        [SerializeField] private InterfaceReference<IText> urgentMessage;
        [SerializeField] private InterfaceReference<IFader> fader;
        [SerializeField] private List<InterfaceReference<IContextualAction>> contextualActions;

        #endregion

        #region Public Methods

        /// <summary>
        /// Fades in the fader.
        /// </summary>
        public void FadeIn() => fader.Interface.FadeIn();

        /// <summary>
        /// Fades out the fader.
        /// </summary>
        public void FadeOut() => fader.Interface.FadeOut();

        /// <summary>
        /// Displays an urgent message to the user.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="duration"></param>
        public void DisplayUrgentMessage(string message, float duration = 0f) {
            urgentMessage.Interface.Text = message;
            if (duration <= 0.0f) {
                urgentMessage.gameObject.SetActive(true);
            } else {
                TimedDisplay(urgentMessage.Interface.gameObject, duration);
            }
        }

        #endregion

        #region Unity Overrides

        private void Reset() {
            fader = new InterfaceReference<IFader>(Utilities.FindInterfacesOfType<IFader>().FirstOrDefault());
            var contextualActions = Utilities.FindInterfacesOfType<IContextualAction>();
            this.contextualActions = new List<InterfaceReference<IContextualAction>>();
            foreach (var contextualAction in contextualActions) {
                this.contextualActions.Add(new InterfaceReference<IContextualAction>(contextualAction.gameObject));
            }
        }

        #endregion

        #region Private Variables

        private IEnumerator TimedDisplay(GameObject gameObject, float duration) {
            var wfs = new WaitForSeconds(duration);
            gameObject.SetActive(true);
            yield return wfs;
            gameObject.SetActive(false);
        }

        #endregion

    }
}