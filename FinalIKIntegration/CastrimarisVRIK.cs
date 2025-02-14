#if HVR_OCULUS

using Castrimaris.Core;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using RootMotion.FinalIK;
using System.Collections;
using UnityEngine;

namespace Castrimaris.FinalIKIntegration {

    /// <summary>
    /// Customized version of Final IK's VR version
    /// </summary>
    public class CastrimarisVRIK : VRIK {

        [Header("Additional Settings")]
        [Tooltip("Wheter to set the local Y position to 0 always even when using rootmotion to avoid compenetration into geometry.")]
        [SerializeField] private bool isFixedOnY = false;
        [Tooltip("How long should the initial height calibration last.")]
        [SerializeField, Range(10f, 60f)] private float calibrationTime = 10.0f;
        [Tooltip("How often the calibration should happen during the calibration time.")]
        [SerializeField, Range(.1f, 10f)] private float calibrationPollingRate = 1.0f;

        [ContextMenu("Autodetect Targets")]
        public void DetectTargets() {
            var root = transform.root;
            var headTarget = root.RecursiveFind("Head VRIK Target");
            var leftHandTarget = root.RecursiveFind("Left Hand VRIK Target");
            var rightHandTarget = root.RecursiveFind("Right Hand VRIK Target");
            if (headTarget == null || leftHandTarget == null || rightHandTarget == null) {
                Debug.LogError($"ERROR: Couldn't find targets for VRIK! Did you change the GO names?");
                return;
            }
            solver.spine.headTarget = headTarget;
            solver.leftArm.target = leftHandTarget;
            solver.rightArm.target = rightHandTarget;
        }

        protected override void UpdateSolver() {
            base.UpdateSolver();
            if (isFixedOnY) {
                this.transform.localPosition = new Vector3(this.transform.localPosition.x, 0, this.transform.localPosition.z);
            }
        }

        private void Start() {
            StartCoroutine(CalibrationBehaviour());
        }

        private IEnumerator CalibrationBehaviour() {
            //TODO add progressive mean
            var progressiveMean = new ProgressiveMean();

            Log.D($"Entering calibration period...");
            var wfs = new WaitForSeconds(calibrationPollingRate);
            var iterations = calibrationTime / calibrationPollingRate;
            for (int i = 0; i < iterations; i++) {
                VRIKAvgCalibrator.Calibrate(this, solver.spine.headTarget, solver.leftArm.target, solver.rightArm.target, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
                yield return wfs;
            }
            Log.D($"Calibration period ended.");
        }
    }
}

#endif