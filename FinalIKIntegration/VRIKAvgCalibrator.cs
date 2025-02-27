#if HVR_OCULUS

using Castrimaris.Core;
using RootMotion;
using RootMotion.FinalIK;
using UnityEngine;

public static class VRIKAvgCalibrator {

    private static ProgressiveMean rootHeight = new ProgressiveMean();

    /// <summary>
    /// Recalibrates only the avatar scale, updates CalibrationData to the new scale value
    /// </summary>
    public static void RecalibrateScale(VRIK ik, VRIKCalibrator.CalibrationData data, VRIKCalibrator.Settings settings) {
        RecalibrateScale(ik, data, settings.scaleMlp);
    }

    /// <summary>
    /// Recalibrates only the avatar scale, updates CalibrationData to the new scale value
    /// </summary>
    public static void RecalibrateScale(VRIK ik, VRIKCalibrator.CalibrationData data, float scaleMlp) {
        CalibrateScale(ik, scaleMlp);
        data.scale = ik.references.root.localScale.y;
    }

    /// <summary>
    /// Calibrates only the avatar scale.
    /// </summary>
    private static void CalibrateScale(VRIK ik, VRIKCalibrator.Settings settings) {
        CalibrateScale(ik, settings.scaleMlp);
    }

    /// <summary>
    /// Calibrates only the avatar scale.
    /// </summary>
    private static void CalibrateScale(VRIK ik, float scaleMlp = 1f) {
        float sizeF = (ik.solver.spine.headTarget.position.y - ik.references.root.position.y) / (ik.references.head.position.y - ik.references.root.position.y);
        ik.references.root.localScale *= sizeF * scaleMlp;
    }

    /// <summary>
    /// Calibrates VRIK to the specified trackers using the VRIKTrackerCalibrator.Settings.
    /// </summary>
    /// <param name="ik">Reference to the VRIK component.</param>
    /// <param name="settings">Calibration settings.</param>
    /// <param name="headTracker">The HMD.</param>
    /// <param name="bodyTracker">(Optional) A tracker placed anywhere on the body of the player, preferrably close to the pelvis, on the belt area.</param>
    /// <param name="leftHandTracker">(Optional) A tracker or hand controller device placed anywhere on or in the player's left hand.</param>
    /// <param name="rightHandTracker">(Optional) A tracker or hand controller device placed anywhere on or in the player's right hand.</param>
    /// <param name="leftFootTracker">(Optional) A tracker placed anywhere on the ankle or toes of the player's left leg.</param>
    /// <param name="rightFootTracker">(Optional) A tracker placed anywhere on the ankle or toes of the player's right leg.</param>
    public static VRIKCalibrator.CalibrationData Calibrate(VRIK ik, VRIKCalibrator.Settings settings, Transform headTracker, Transform bodyTracker = null, Transform leftHandTracker = null, Transform rightHandTracker = null, Transform leftFootTracker = null, Transform rightFootTracker = null) {
        if (!ik.solver.initiated) {
            Debug.LogError("Can not calibrate before VRIK has initiated.");
            return null;
        }

        if (headTracker == null) {
            Debug.LogError("Can not calibrate VRIK without the head tracker.");
            return null;
        }

        VRIKCalibrator.CalibrationData data = new VRIKCalibrator.CalibrationData();

        ik.solver.FixTransforms();

        // Root position and rotation
        Vector3 headPos = headTracker.position + headTracker.rotation * Quaternion.LookRotation(settings.headTrackerForward, settings.headTrackerUp) * settings.headOffset;
        rootHeight.Add(ik.references.root.position.y);
        ik.references.root.position = new Vector3(headPos.x, rootHeight.Mean, headPos.z);
        Vector3 headForward = headTracker.rotation * settings.headTrackerForward;
        headForward.y = 0f;
        ik.references.root.rotation = Quaternion.LookRotation(headForward);

        // Head
        Transform headTarget = ik.solver.spine.headTarget == null ? (new GameObject("Head Target")).transform : ik.solver.spine.headTarget;
        headTarget.position = headPos;
        headTarget.rotation = ik.references.head.rotation;
        headTarget.parent = headTracker;
        ik.solver.spine.headTarget = headTarget;

        // Size
        float sizeF = (headTarget.position.y - ik.references.root.position.y) / (ik.references.head.position.y - ik.references.root.position.y);
        ik.references.root.localScale *= sizeF * settings.scaleMlp;

        // Body
        if (bodyTracker != null) {
            Transform pelvisTarget = ik.solver.spine.pelvisTarget == null ? (new GameObject("Pelvis Target")).transform : ik.solver.spine.pelvisTarget;
            pelvisTarget.position = ik.references.pelvis.position;
            pelvisTarget.rotation = ik.references.pelvis.rotation;
            pelvisTarget.parent = bodyTracker;
            ik.solver.spine.pelvisTarget = pelvisTarget;

            ik.solver.spine.pelvisPositionWeight = settings.pelvisPositionWeight;
            ik.solver.spine.pelvisRotationWeight = settings.pelvisRotationWeight;

            ik.solver.plantFeet = false;
            ik.solver.spine.maxRootAngle = 180f;
        } else if (leftFootTracker != null && rightFootTracker != null) {
            ik.solver.spine.maxRootAngle = 0f;
        }

        // Left Hand
        if (leftHandTracker != null) {
            Transform leftHandTarget = ik.solver.leftArm.target == null ? (new GameObject("Left Hand Target")).transform : ik.solver.leftArm.target;
            leftHandTarget.position = leftHandTracker.position + leftHandTracker.rotation * Quaternion.LookRotation(settings.handTrackerForward, settings.handTrackerUp) * settings.handOffset;
            Vector3 leftHandUp = Vector3.Cross(ik.solver.leftArm.wristToPalmAxis, ik.solver.leftArm.palmToThumbAxis);
            leftHandTarget.rotation = QuaTools.MatchRotation(leftHandTracker.rotation * Quaternion.LookRotation(settings.handTrackerForward, settings.handTrackerUp), settings.handTrackerForward, settings.handTrackerUp, ik.solver.leftArm.wristToPalmAxis, leftHandUp);
            leftHandTarget.parent = leftHandTracker;
            ik.solver.leftArm.target = leftHandTarget;
            ik.solver.leftArm.positionWeight = 1f;
            ik.solver.leftArm.rotationWeight = 1f;
        } else {
            ik.solver.leftArm.positionWeight = 0f;
            ik.solver.leftArm.rotationWeight = 0f;
        }

        // Right Hand
        if (rightHandTracker != null) {
            Transform rightHandTarget = ik.solver.rightArm.target == null ? (new GameObject("Right Hand Target")).transform : ik.solver.rightArm.target;
            rightHandTarget.position = rightHandTracker.position + rightHandTracker.rotation * Quaternion.LookRotation(settings.handTrackerForward, settings.handTrackerUp) * settings.handOffset;
            Vector3 rightHandUp = -Vector3.Cross(ik.solver.rightArm.wristToPalmAxis, ik.solver.rightArm.palmToThumbAxis);
            rightHandTarget.rotation = QuaTools.MatchRotation(rightHandTracker.rotation * Quaternion.LookRotation(settings.handTrackerForward, settings.handTrackerUp), settings.handTrackerForward, settings.handTrackerUp, ik.solver.rightArm.wristToPalmAxis, rightHandUp);
            rightHandTarget.parent = rightHandTracker;
            ik.solver.rightArm.target = rightHandTarget;
            ik.solver.rightArm.positionWeight = 1f;
            ik.solver.rightArm.rotationWeight = 1f;
        } else {
            ik.solver.rightArm.positionWeight = 0f;
            ik.solver.rightArm.rotationWeight = 0f;
        }

        // Legs
        if (leftFootTracker != null) CalibrateLeg(settings, leftFootTracker, ik.solver.leftLeg, (ik.references.leftToes != null ? ik.references.leftToes : ik.references.leftFoot), ik.references.root.forward, true);
        if (rightFootTracker != null) CalibrateLeg(settings, rightFootTracker, ik.solver.rightLeg, (ik.references.rightToes != null ? ik.references.rightToes : ik.references.rightFoot), ik.references.root.forward, false);

        // Root controller
        bool addRootController = bodyTracker != null || (leftFootTracker != null && rightFootTracker != null);
        var rootController = ik.references.root.GetComponent<VRIKRootController>();

        if (addRootController) {
            if (rootController == null) rootController = ik.references.root.gameObject.AddComponent<VRIKRootController>();
            rootController.Calibrate();
        } else {
            if (rootController != null) GameObject.Destroy(rootController);
        }

        // Additional solver settings
        ik.solver.spine.minHeadHeight = 0f;
        ik.solver.locomotion.weight = bodyTracker == null && leftFootTracker == null && rightFootTracker == null ? 1f : 0f;

        // Fill in Calibration Data
        data.scale = ik.references.root.localScale.y;
        data.head = new VRIKCalibrator.CalibrationData.Target(ik.solver.spine.headTarget);
        data.pelvis = new VRIKCalibrator.CalibrationData.Target(ik.solver.spine.pelvisTarget);
        data.leftHand = new VRIKCalibrator.CalibrationData.Target(ik.solver.leftArm.target);
        data.rightHand = new VRIKCalibrator.CalibrationData.Target(ik.solver.rightArm.target);
        data.leftFoot = new VRIKCalibrator.CalibrationData.Target(ik.solver.leftLeg.target);
        data.rightFoot = new VRIKCalibrator.CalibrationData.Target(ik.solver.rightLeg.target);
        data.leftLegGoal = new VRIKCalibrator.CalibrationData.Target(ik.solver.leftLeg.bendGoal);
        data.rightLegGoal = new VRIKCalibrator.CalibrationData.Target(ik.solver.rightLeg.bendGoal);
        data.pelvisTargetRight = rootController != null ? rootController.pelvisTargetRight : Vector3.zero;
        data.pelvisPositionWeight = ik.solver.spine.pelvisPositionWeight;
        data.pelvisRotationWeight = ik.solver.spine.pelvisRotationWeight;

        return data;
    }

    private static void CalibrateLeg(VRIKCalibrator.Settings settings, Transform tracker, IKSolverVR.Leg leg, Transform lastBone, Vector3 rootForward, bool isLeft) {
        string name = isLeft ? "Left" : "Right";
        Transform target = leg.target == null ? (new GameObject(name + " Foot Target")).transform : leg.target;

        // Space of the tracker heading
        Quaternion trackerSpace = tracker.rotation * Quaternion.LookRotation(settings.footTrackerForward, settings.footTrackerUp);
        Vector3 f = trackerSpace * Vector3.forward;
        f.y = 0f;
        trackerSpace = Quaternion.LookRotation(f);

        // Target position
        float inwardOffset = isLeft ? settings.footInwardOffset : -settings.footInwardOffset;
        target.position = tracker.position + trackerSpace * new Vector3(inwardOffset, 0f, settings.footForwardOffset);
        target.position = new Vector3(target.position.x, lastBone.position.y, target.position.z);

        // Target rotation
        target.rotation = lastBone.rotation;

        // Rotate target forward towards tracker forward
        Vector3 footForward = AxisTools.GetAxisVectorToDirection(lastBone, rootForward);
        if (Vector3.Dot(lastBone.rotation * footForward, rootForward) < 0f) footForward = -footForward;
        Vector3 fLocal = Quaternion.Inverse(Quaternion.LookRotation(target.rotation * footForward)) * f;
        float angle = Mathf.Atan2(fLocal.x, fLocal.z) * Mathf.Rad2Deg;
        float headingOffset = isLeft ? settings.footHeadingOffset : -settings.footHeadingOffset;
        target.rotation = Quaternion.AngleAxis(angle + headingOffset, Vector3.up) * target.rotation;

        target.parent = tracker;
        leg.target = target;

        leg.positionWeight = 1f;
        leg.rotationWeight = 1f;

        // Bend goal
        Transform bendGoal = leg.bendGoal == null ? (new GameObject(name + " Leg Bend Goal")).transform : leg.bendGoal;
        bendGoal.position = lastBone.position + trackerSpace * Vector3.forward + trackerSpace * Vector3.up;// * 0.5f;
        bendGoal.parent = tracker;
        leg.bendGoal = bendGoal;
        leg.bendGoalWeight = 1f;
    }

    /// <summary>
    /// Calibrates VRIK to the specified trackers using CalibrationData from a previous calibration. Requires this character's bone orientations to match with the character's that was used in the previous calibration.
    /// </summary>
    /// <param name="ik">Reference to the VRIK component.</param>
    /// <param name="data">Use calibration data from a previous calibration.</param>
    /// <param name="headTracker">The HMD.</param>
    /// <param name="bodyTracker">(Optional) A tracker placed anywhere on the body of the player, preferrably close to the pelvis, on the belt area.</param>
    /// <param name="leftHandTracker">(Optional) A tracker or hand controller device placed anywhere on or in the player's left hand.</param>
    /// <param name="rightHandTracker">(Optional) A tracker or hand controller device placed anywhere on or in the player's right hand.</param>
    /// <param name="leftFootTracker">(Optional) A tracker placed anywhere on the ankle or toes of the player's left leg.</param>
    /// <param name="rightFootTracker">(Optional) A tracker placed anywhere on the ankle or toes of the player's right leg.</param>
    public static void Calibrate(VRIK ik, VRIKCalibrator.CalibrationData data, Transform headTracker, Transform bodyTracker = null, Transform leftHandTracker = null, Transform rightHandTracker = null, Transform leftFootTracker = null, Transform rightFootTracker = null) {
        if (!ik.solver.initiated) {
            Debug.LogError("Can not calibrate before VRIK has initiated.");
            return;
        }

        if (headTracker == null) {
            Debug.LogError("Can not calibrate VRIK without the head tracker.");
            return;
        }

        ik.solver.FixTransforms();

        // Head
        Transform headTarget = ik.solver.spine.headTarget == null ? (new GameObject("Head Target")).transform : ik.solver.spine.headTarget;
        headTarget.parent = headTracker;
        data.head.SetTo(headTarget);
        ik.solver.spine.headTarget = headTarget;

        // Size
        ik.references.root.localScale = data.scale * Vector3.one;

        // Body
        if (bodyTracker != null && data.pelvis != null) {
            Transform pelvisTarget = ik.solver.spine.pelvisTarget == null ? (new GameObject("Pelvis Target")).transform : ik.solver.spine.pelvisTarget;
            pelvisTarget.parent = bodyTracker;
            data.pelvis.SetTo(pelvisTarget);
            ik.solver.spine.pelvisTarget = pelvisTarget;

            ik.solver.spine.pelvisPositionWeight = data.pelvisPositionWeight;
            ik.solver.spine.pelvisRotationWeight = data.pelvisRotationWeight;

            ik.solver.plantFeet = false;
            ik.solver.spine.maxRootAngle = 180f;
        } else if (leftFootTracker != null && rightFootTracker != null) {
            ik.solver.spine.maxRootAngle = 0f;
        }

        // Left Hand
        if (leftHandTracker != null) {
            Transform leftHandTarget = ik.solver.leftArm.target == null ? (new GameObject("Left Hand Target")).transform : ik.solver.leftArm.target;
            leftHandTarget.parent = leftHandTracker;
            data.leftHand.SetTo(leftHandTarget);
            ik.solver.leftArm.target = leftHandTarget;
            ik.solver.leftArm.positionWeight = 1f;
            ik.solver.leftArm.rotationWeight = 1f;
        } else {
            ik.solver.leftArm.positionWeight = 0f;
            ik.solver.leftArm.rotationWeight = 0f;
        }

        // Right Hand
        if (rightHandTracker != null) {
            Transform rightHandTarget = ik.solver.rightArm.target == null ? (new GameObject("Right Hand Target")).transform : ik.solver.rightArm.target;
            rightHandTarget.parent = rightHandTracker;
            data.rightHand.SetTo(rightHandTarget);
            ik.solver.rightArm.target = rightHandTarget;
            ik.solver.rightArm.positionWeight = 1f;
            ik.solver.rightArm.rotationWeight = 1f;
        } else {
            ik.solver.rightArm.positionWeight = 0f;
            ik.solver.rightArm.rotationWeight = 0f;
        }

        // Legs
        if (leftFootTracker != null) CalibrateLeg(data, leftFootTracker, ik.solver.leftLeg, (ik.references.leftToes != null ? ik.references.leftToes : ik.references.leftFoot), ik.references.root.forward, true);
        if (rightFootTracker != null) CalibrateLeg(data, rightFootTracker, ik.solver.rightLeg, (ik.references.rightToes != null ? ik.references.rightToes : ik.references.rightFoot), ik.references.root.forward, false);

        // Root controller
        bool addRootController = bodyTracker != null || (leftFootTracker != null && rightFootTracker != null);
        var rootController = ik.references.root.GetComponent<VRIKRootController>();

        if (addRootController) {
            if (rootController == null) rootController = ik.references.root.gameObject.AddComponent<VRIKRootController>();
            rootController.Calibrate(data);
        } else {
            if (rootController != null) GameObject.Destroy(rootController);
        }

        // Additional solver settings
        ik.solver.spine.minHeadHeight = 0f;
        ik.solver.locomotion.weight = bodyTracker == null && leftFootTracker == null && rightFootTracker == null ? 1f : 0f;
    }

    private static void CalibrateLeg(VRIKCalibrator.CalibrationData data, Transform tracker, IKSolverVR.Leg leg, Transform lastBone, Vector3 rootForward, bool isLeft) {
        if (isLeft && data.leftFoot == null) return;
        if (!isLeft && data.rightFoot == null) return;

        string name = isLeft ? "Left" : "Right";
        Transform target = leg.target == null ? (new GameObject(name + " Foot Target")).transform : leg.target;

        target.parent = tracker;

        if (isLeft) data.leftFoot.SetTo(target);
        else data.rightFoot.SetTo(target);

        leg.target = target;

        leg.positionWeight = 1f;
        leg.rotationWeight = 1f;

        // Bend goal
        Transform bendGoal = leg.bendGoal == null ? (new GameObject(name + " Leg Bend Goal")).transform : leg.bendGoal;
        bendGoal.parent = tracker;

        if (isLeft) data.leftLegGoal.SetTo(bendGoal);
        else data.rightLegGoal.SetTo(bendGoal);

        leg.bendGoal = bendGoal;
        leg.bendGoalWeight = 1f;
    }

    /// <summary>
    /// Simple calibration to head and hands using predefined anchor position and rotation offsets.
    /// </summary>
    /// <param name="ik">The VRIK component.</param>
    /// <param name="centerEyeAnchor">HMD.</param>
    /// <param name="leftHandAnchor">Left hand controller.</param>
    /// <param name="rightHandAnchor">Right hand controller.</param>
    /// <param name="centerEyePositionOffset">Position offset of the camera from the head bone (root space).</param>
    /// <param name="centerEyeRotationOffset">Rotation offset of the camera from the head bone (root space).</param>
    /// <param name="handPositionOffset">Position offset of the hand controller from the hand bone (controller space).</param>
    /// <param name="handRotationOffset">Rotation offset of the hand controller from the hand bone (controller space).</param>
    /// <param name="scaleMlp">Multiplies the scale of the root.</param>
    /// <returns></returns>
    public static VRIKCalibrator.CalibrationData Calibrate(VRIK ik, Transform centerEyeAnchor, Transform leftHandAnchor, Transform rightHandAnchor, Vector3 centerEyePositionOffset, Vector3 centerEyeRotationOffset, Vector3 handPositionOffset, Vector3 handRotationOffset, float scaleMlp = 1f) {
        CalibrateHead(ik, centerEyeAnchor, centerEyePositionOffset, centerEyeRotationOffset);
        CalibrateHands(ik, leftHandAnchor, rightHandAnchor, handPositionOffset, handRotationOffset);
        CalibrateScale(ik, scaleMlp);

        // Fill in Calibration Data
        VRIKCalibrator.CalibrationData data = new VRIKCalibrator.CalibrationData();
        data.scale = ik.references.root.localScale.y;
        data.head = new VRIKCalibrator.CalibrationData.Target(ik.solver.spine.headTarget);
        data.leftHand = new VRIKCalibrator.CalibrationData.Target(ik.solver.leftArm.target);
        data.rightHand = new VRIKCalibrator.CalibrationData.Target(ik.solver.rightArm.target);

        return data;
    }

    /// <summary>
    /// Calibrates head IK target to specified anchor position and rotation offset independent of avatar bone orientations.
    /// </summary>
    public static void CalibrateHead(VRIK ik, Transform centerEyeAnchor, Vector3 anchorPositionOffset, Vector3 anchorRotationOffset) {
        if (ik.solver.spine.headTarget == null) ik.solver.spine.headTarget = new GameObject("Head IK Target").transform;

        Vector3 forward = Quaternion.Inverse(ik.references.head.rotation) * ik.references.root.forward;
        Vector3 up = Quaternion.Inverse(ik.references.head.rotation) * ik.references.root.up;
        Quaternion headSpace = Quaternion.LookRotation(forward, up);

        Vector3 anchorPos = ik.references.head.position + ik.references.head.rotation * headSpace * anchorPositionOffset;
        Quaternion anchorRot = ik.references.head.rotation * headSpace * Quaternion.Euler(anchorRotationOffset);
        Quaternion anchorRotInverse = Quaternion.Inverse(anchorRot);

        ik.solver.spine.headTarget.parent = centerEyeAnchor;
        ik.solver.spine.headTarget.localPosition = anchorRotInverse * (ik.references.head.position - anchorPos);
        ik.solver.spine.headTarget.localRotation = anchorRotInverse * ik.references.head.rotation;
    }

    /// <summary>
    /// Calibrates body target to avatar pelvis position and position/rotation offsets in character root space.
    /// </summary>
    public static void CalibrateBody(VRIK ik, Transform pelvisTracker, Vector3 trackerPositionOffset, Vector3 trackerRotationOffset) {
        if (ik.solver.spine.pelvisTarget == null) ik.solver.spine.pelvisTarget = new GameObject("Pelvis IK Target").transform;

        ik.solver.spine.pelvisTarget.position = ik.references.pelvis.position + ik.references.root.rotation * trackerPositionOffset;
        ik.solver.spine.pelvisTarget.rotation = ik.references.root.rotation * Quaternion.Euler(trackerRotationOffset);
        ik.solver.spine.pelvisTarget.parent = pelvisTracker;
    }

    /// <summary>
    /// Calibrates hand IK targets to specified anchor position and rotation offsets independent of avatar bone orientations.
    /// </summary>
    public static void CalibrateHands(VRIK ik, Transform leftHandAnchor, Transform rightHandAnchor, Vector3 anchorPositionOffset, Vector3 anchorRotationOffset) {
        if (ik.solver.leftArm.target == null) ik.solver.leftArm.target = new GameObject("Left Hand IK Target").transform;
        if (ik.solver.rightArm.target == null) ik.solver.rightArm.target = new GameObject("Right Hand IK Target").transform;

        CalibrateHand(ik, leftHandAnchor, anchorPositionOffset, anchorRotationOffset, true);
        CalibrateHand(ik, rightHandAnchor, anchorPositionOffset, anchorRotationOffset, false);
    }

    private static void CalibrateHand(VRIK ik, Transform anchor, Vector3 positionOffset, Vector3 rotationOffset, bool isLeft) {
        if (isLeft) {
            positionOffset.x = -positionOffset.x;
            rotationOffset.y = -rotationOffset.y;
            rotationOffset.z = -rotationOffset.z;
        }

        var hand = isLeft ? ik.references.leftHand : ik.references.rightHand;
        var forearm = isLeft ? ik.references.leftForearm : ik.references.rightForearm;
        var target = isLeft ? ik.solver.leftArm.target : ik.solver.rightArm.target;

        Vector3 forward = isLeft ? ik.solver.leftArm.wristToPalmAxis : ik.solver.rightArm.wristToPalmAxis;
        if (forward == Vector3.zero) forward = VRIKCalibrator.GuessWristToPalmAxis(hand, forearm);

        Vector3 up = isLeft ? ik.solver.leftArm.palmToThumbAxis : ik.solver.rightArm.palmToThumbAxis;
        if (up == Vector3.zero) up = VRIKCalibrator.GuessPalmToThumbAxis(hand, forearm);

        Quaternion handSpace = Quaternion.LookRotation(forward, up);
        Vector3 anchorPos = hand.position + hand.rotation * handSpace * positionOffset;
        Quaternion anchorRot = hand.rotation * handSpace * Quaternion.Euler(rotationOffset);
        Quaternion anchorRotInverse = Quaternion.Inverse(anchorRot);

        target.parent = anchor;
        target.localPosition = anchorRotInverse * (hand.position - anchorPos);
        target.localRotation = anchorRotInverse * hand.rotation;
    }

    public static Vector3 GuessWristToPalmAxis(Transform hand, Transform forearm) {
        Vector3 toForearm = forearm.position - hand.position;
        Vector3 axis = AxisTools.ToVector3(AxisTools.GetAxisToDirection(hand, toForearm));
        if (Vector3.Dot(toForearm, hand.rotation * axis) > 0f) axis = -axis;
        return axis;
    }

    public static Vector3 GuessPalmToThumbAxis(Transform hand, Transform forearm) {
        if (hand.childCount == 0) {
            Debug.LogWarning("Hand " + hand.name + " does not have any fingers, VRIK can not guess the hand bone's orientation. Please assign 'Wrist To Palm Axis' and 'Palm To Thumb Axis' manually for both arms in VRIK settings.", hand);
            return Vector3.zero;
        }

        float closestSqrMag = Mathf.Infinity;
        int thumbIndex = 0;

        for (int i = 0; i < hand.childCount; i++) {
            float sqrMag = Vector3.SqrMagnitude(hand.GetChild(i).position - hand.position);
            if (sqrMag < closestSqrMag) {
                closestSqrMag = sqrMag;
                thumbIndex = i;
            }
        }

        Vector3 handNormal = Vector3.Cross(hand.position - forearm.position, hand.GetChild(thumbIndex).position - hand.position);
        Vector3 toThumb = Vector3.Cross(handNormal, hand.position - forearm.position);
        Vector3 axis = AxisTools.ToVector3(AxisTools.GetAxisToDirection(hand, toThumb));
        if (Vector3.Dot(toThumb, hand.rotation * axis) < 0f) axis = -axis;
        return axis;
    }
}

#endif