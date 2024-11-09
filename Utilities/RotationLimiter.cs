using Castrimaris.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Castrimaris.Utilities
{
    public class RotationLimiter : MonoBehaviour
    {
        public float minRotationX = -45f;
        public float maxRotationX = 45f;
        public float minRotationY = -45f;
        public float maxRotationY = 45f;
        public float minRotationZ = -45f;
        public float maxRotationZ = 45f;

        private Rigidbody rb;

        void Start() {
            rb = GetComponent<Rigidbody>();
        }

        void FixedUpdate() {
            Quaternion clampedRotation = ClampRotation(rb.rotation);
            rb.MoveRotation(clampedRotation);
        }

        Quaternion ClampRotation(Quaternion rotation) {
            Vector3 euler = rotation.eulerAngles;

            euler.x = ClampAngle(euler.x, minRotationX, maxRotationX);
            euler.y = ClampAngle(euler.y, minRotationY, maxRotationY);
            euler.z = ClampAngle(euler.z, minRotationZ, maxRotationZ);

            return Quaternion.Euler(euler);
        }

        float ClampAngle(float angle, float min, float max) {
            angle = angle > 180 ? angle - 360 : angle;
            angle = Mathf.Clamp(angle, min, max);
            return angle < 0 ? angle + 360 : angle;
        }
    }
}
