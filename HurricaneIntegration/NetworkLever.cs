#if HVR_OCULUS

using Castrimaris.Attributes;
using Castrimaris.Core;
using Castrimaris.Core.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.HurricaneIntegration {

    /// <summary>
    /// Physical grabbable lever synced on the Network.
    /// </summary>
    [RequireComponent(typeof(NetworkGrabbable))]
    public class NetworkLever : NetworkBehaviour {

        public enum ActivationTypes { POSITION = 0, ROTATION }

        [Header("Parameters")]
        [Tooltip("Wheter this lever gets activated by rotating it or by moving its position")]
        [SerializeField] private ActivationTypes activationType = ActivationTypes.ROTATION;
        [Tooltip("The axis on which the rotation or position moves")]
        [SerializeField] private AxisTypes axisType = AxisTypes.X;

        [ConditionalField(nameof(activationType), ActivationTypes.ROTATION, DisablingTypes.Hidden)]
        [Tooltip("Max rotation over which the value returned by the lever stays at the max.")]
        [SerializeField] private Quaternion maxRotation;
        [ConditionalField(nameof(activationType), ActivationTypes.ROTATION, DisablingTypes.Hidden)]
        [Tooltip("Min rotation over which the value returned by the lever stays at the min.")]
        [SerializeField] private Quaternion minRotation;

        [ConditionalField(nameof(activationType), ActivationTypes.POSITION, DisablingTypes.Hidden)]
        [Tooltip("Max position over which the value returned by the lever stays at the min.")]
        [SerializeField] private Vector3 maxPosition;
        [ConditionalField(nameof(activationType), ActivationTypes.POSITION, DisablingTypes.Hidden)]
        [Tooltip("Min position over which the value returned by the lever stays at the min.")]
        [SerializeField] private Vector3 minPosition;

        [Header("Events")]
        [SerializeField] private UnityEvent<float> onValueChange = new UnityEvent<float>();

        private NetworkGrabbable networkGrabbable;

        [Header("Debug")]
        [SerializeField] private float maxValue;
        [SerializeField] private float currentValue;
        [SerializeField] private float normalizedValue;

        public UnityEvent<float> OnRotationChange => onValueChange;

        private void Awake() {
            networkGrabbable = GetComponent<NetworkGrabbable>();
            maxValue = (activationType == ActivationTypes.ROTATION) ? Quaternion.Angle(minRotation, maxRotation) : Vector3.Distance(minPosition, maxPosition);
        }

        private void Update() {
            if (!IsOwner)
                return;

            if (!networkGrabbable.IsBeingHeld)
                return;

            currentValue = GetActivationValue();
            normalizedValue = Mathf.Clamp01(currentValue.Normalized(0, maxValue));
            normalizedValue = (normalizedValue < 0.09f) ? 0 : normalizedValue; //HACK: please, normalize it better!
            onValueChange.Invoke(normalizedValue);
            OnChangeServerRpc();
        }

        private float GetActivationValue() {
            if (activationType == ActivationTypes.ROTATION)
                return Quaternion.Angle(minRotation, transform.localRotation);

            //If the distance is higher than the min value (because maybe the min value isn't actually the min in the lever position) we use the indicated min value, otherwise the value would increase in case it goes over the max. A sort of clamping.
            return Vector3.Distance(minPosition, transform.localPosition);
        }

        [ServerRpc]
        private void OnChangeServerRpc() {
            currentValue = GetActivationValue();
            normalizedValue = Mathf.Clamp01(currentValue.Normalized(0, maxValue));
            normalizedValue = (normalizedValue < 0.09f) ? 0 : normalizedValue; //HACK: please, normalize it better!
            onValueChange.Invoke(normalizedValue);
            OnChangeClientRpc();
        }

        [ClientRpc]
        private void OnChangeClientRpc() {
            //Don't fire the event for the client that's using this lever, as it has already fired
            if (NetworkObject.OwnerClientId == NetworkManager.LocalClientId)
                return;

            currentValue = GetActivationValue();
            normalizedValue = Mathf.Clamp01(currentValue.Normalized(0, maxValue));
            normalizedValue = (normalizedValue < 0.09f) ? 0 : normalizedValue; //HACK: please, normalize it better!
            onValueChange.Invoke(normalizedValue);
        }

        [ExposeInInspector]
        [ContextMenu("Save Min Values")]
        private void SaveMinValues() { 
            minRotation = this.transform.localRotation;
            minPosition = this.transform.localPosition;
        }

        [ExposeInInspector]
        [ContextMenu("Save Max Values")]
        private void SaveMaxValues() { 
            maxRotation = this.transform.localRotation;
            maxPosition = this.transform.localPosition;
        }

        [ExposeInInspector]
        [ContextMenu("Set Min Values")]
        private void SetMinValues() {
            this.transform.localRotation = minRotation;
            this.transform.localPosition = minPosition;
        }

        [ExposeInInspector]
        [ContextMenu("Set Max Values")]
        private void SetMaxValues() {
            this.transform.localRotation = maxRotation;
            this.transform.localPosition = maxPosition;
        }
    }

}

#endif