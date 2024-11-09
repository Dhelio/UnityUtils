using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Castrimaris.Animations
{
    public class EyesTargeter : MonoBehaviour {
        [Header("Parameters")]
        [SerializeField] private Transform mainTarget;
        [SerializeField] private Transform[] randomizableTargets;

        [Header("References")]
        [SerializeField] private BlendshapedEye[] eyes;

        public Transform MainTarget { 
            get {
                if (mainTarget == null)
                    mainTarget = Camera.main.transform;
                return mainTarget;
            } 
        }

        public void FocusOnMainTarget() {
            foreach (var eye in eyes) {
                eye.Target = MainTarget;
            }
        }

        public void FocusOnRandomizableTarget() {
            var randomizedTarget = randomizableTargets[Random.Range(0, randomizableTargets.Length)];
            foreach(var eye in eyes) {
                eye.Target = randomizedTarget;
            }
        }

        public void FocusOnRandomizableTarget(int index) {
            index = Mathf.Clamp(index, 0, randomizableTargets.Length-1);
            var randomizedTarget = randomizableTargets[index];
            foreach (var eye in eyes) {
                eye.Target = randomizedTarget;
            }
        }

        public void FocusOnRandomizableTarget(string targetName) {
            var randomizedTarget = this.transform.Find(targetName);
            foreach (var eye in eyes) {
                eye.Target = randomizedTarget;
            }
        }

        private void Reset() {
            var eyes = GetComponentsInChildren<BlendshapedEye>();
        }
    }
}
