#if HVR_OCULUS

using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using System;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;

namespace Castrimaris.HurricaneIntegration {

    /// <summary>
    /// Custom editor for <see cref="NetworkGrabbable"/>, automatically sets references for the callbacks used by the class to define the ownership of the object.
    /// This is necessary to speed up development, otherwise all callbacks should be added to each <see cref="HVRGrabbable"/> component one by one
    /// </summary>
    [CustomEditor(typeof(NetworkGrabbable))]
    public class NetworkGrabbableInspector : Editor {
        private NetworkGrabbable targetReference;

        private void OnEnable() {
            //Retrieve references from the base class and the Grabbable class
            targetReference = target as NetworkGrabbable;
            var grabbable = targetReference.GetComponent<HVRGrabbable>();

            //Check if the Grab event in the NetworkedGrabbable class is added as a persistent listener
            bool hasGrabbedEvent = false;
            for (int i = 0; i < grabbable.Grabbed.GetPersistentEventCount() && !hasGrabbedEvent; i++) {
                if (grabbable.Grabbed.GetPersistentMethodName(i) == nameof(targetReference.Grab))
                    hasGrabbedEvent = true;
            }

            //Check if the Release event in the NetworkedGrabbable class is added as a persistent listener
            bool hasReleasedEvent = false;
            for (int i = 0; i < grabbable.Released.GetPersistentEventCount() && !hasReleasedEvent; i++) {
                if (grabbable.Released.GetPersistentMethodName(i) == nameof(targetReference.Release))
                    hasReleasedEvent = true;
            }

            //If there is no persistent listener named "Grab" from "NetworkGrabbable", add it.
            if (!hasGrabbedEvent) {
                var methodInfo = UnityEvent.GetValidMethodInfo(targetReference, nameof(targetReference.Grab), new Type[0]);
                var method = Delegate.CreateDelegate(typeof(UnityAction<HVRGrabberBase, HVRGrabbable>), targetReference, nameof(targetReference.Grab)) as UnityAction<HVRGrabberBase, HVRGrabbable>;
                UnityEventTools.AddPersistentListener<HVRGrabberBase, HVRGrabbable>(grabbable.Grabbed, method);
            }

            //If there is no persistent listener named "Release" from "NetworkGrabbable", add it.
            if (!hasReleasedEvent) {
                var methodInfo = UnityEvent.GetValidMethodInfo(targetReference, nameof(targetReference.Release), new Type[0]);
                var method = Delegate.CreateDelegate(typeof(UnityAction<HVRGrabberBase, HVRGrabbable>), targetReference, nameof(targetReference.Release)) as UnityAction<HVRGrabberBase, HVRGrabbable>;
                UnityEventTools.AddPersistentListener<HVRGrabberBase, HVRGrabbable>(grabbable.Released, method);
            }

        }
    }
}

#endif