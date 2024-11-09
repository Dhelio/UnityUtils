using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Castrimaris.Player {

    public class PlayerHaptics : MonoBehaviour, IHaptics {

        [Header("Parameters")]
        [Range(0f, 1f), SerializeField] private float softAmplitude = .3f;
        [Range(0f, 5f), SerializeField] private float shortDuration = 1f;
        [Range(0f, 1f), SerializeField] private float hardAmplitude = 1f;
        [Range(0f, 5f), SerializeField] private float longDuration = 2f;

        [Header("References")]
        [SerializeField] private XRBaseController leftController;
        [SerializeField] private XRBaseController rightController;


        #region Vibrations Left
        public void LeftLongHardVibrate()  => LongHardVibrate(HapticsTargets.LEFT);
        public void LeftLongSoftVibrate() => LongSoftVibrate(HapticsTargets.LEFT);
        public void LeftShortHardVibrate() => ShortHardVibrate(HapticsTargets.LEFT);
        public void LeftShortSoftVibrate() => ShortSoftVibrate(HapticsTargets.LEFT);
        #endregion

        #region Vibrations Right
        public void RightLongHardVibrate() => LongHardVibrate(HapticsTargets.RIGHT);
        public void RightLongSoftVibrate() => LongSoftVibrate(HapticsTargets.RIGHT);
        public void RightShortHardVibrate() => ShortHardVibrate(HapticsTargets.RIGHT);
        public void RightShortSoftVibrate() => ShortSoftVibrate(HapticsTargets.RIGHT);
        #endregion

        #region Vibrations With Target
        public void LongHardVibrate(HapticsTargets Target) => Vibrate(Target, hardAmplitude, longDuration);
        public void LongSoftVibrate(HapticsTargets Target) => Vibrate(Target, softAmplitude, shortDuration);
        public void ShortHardVibrate(HapticsTargets Target) => Vibrate(Target, hardAmplitude, shortDuration);
        public void ShortSoftVibrate(HapticsTargets Target) => Vibrate(Target, softAmplitude, shortDuration);
        #endregion

        #region Vibrations
        public void Vibrate(float Amplitude, float Duration) {
            leftController.SendHapticImpulse(Amplitude, Duration);
            rightController.SendHapticImpulse(Amplitude, Duration);
        }

        public void Vibrate(HapticsTargets Target, float Amplitude, float Duration) {
            var target = GetTarget(Target);
            if (target == null) {
                Vibrate(Amplitude, Duration);
            } else {
                target.SendHapticImpulse(Amplitude, Duration);
            }
        }
        #endregion

        #region UNITY OVERRIDES
        private void Awake() {
            FindReferences();
        }

        private void Reset() {
            FindReferences();
        }
        #endregion

        #region Private Methods
        private XRBaseController GetTarget(HapticsTargets Target) {
            switch (Target) {
                case HapticsTargets.LEFT:
                    return leftController;
                case HapticsTargets.RIGHT:
                    return rightController;
                default:
                    return null;
            }
        }

        [ContextMenu("Autofind References")]
        //TODO
        private void FindReferences() {
            //if (leftController == null || rightController == null) {
            //    var controllers = GetComponentsInChildren<XRBaseController>();
            //    if (controllers.Length < 2) {
            //        throw new MissingComponentException("There must be exactly 2 controllers! Please, create another XRBaseController");
            //    }
            //    if (controllers[0].name.ToLower().Contains("left")) {
            //        leftController = controllers[0];
            //        rightController = controllers[1];
            //    } else {
            //        leftController = controllers[1];
            //        rightController = controllers[0];
            //    }
            //}
        }
        #endregion
    }

}
