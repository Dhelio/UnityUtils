using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.Animations {

    public class AnimationEvents : MonoBehaviour {

        [Header("Events")]
        public UnityEvent RegisteredEvents = new UnityEvent();

        public void InvokeRegisteredEvents() {
            RegisteredEvents.Invoke();
        }

    }

}