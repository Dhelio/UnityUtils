using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.Core.Events {

    /// <summary>
    /// Base class for redirecting events based on a switch.
    /// </summary>
    /// <typeparam name="T">The type for the callback</typeparam>
    public abstract class EventRedirector<T> : MonoBehaviour {
        [Header("Parameters")]
        [SerializeField] private bool switchState = true;

        [Header("Events")]
        [SerializeField] private UnityEvent<T> switchOnPath = new UnityEvent<T>();
        [SerializeField] private UnityEvent<T> switchOffPath = new UnityEvent<T>();

        public void Redirect(T data) {
            if (switchState) {
                switchOnPath.Invoke(data);
            } else {
                switchOffPath.Invoke(data);
            }
        }

        public void Switch() => switchState = !switchState;

        public void SetSwitch(bool switchState) => this.switchState = switchState;
    }
}
