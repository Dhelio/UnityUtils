using Castrimaris.Animations.Contracts;
using UnityEngine;

namespace Castrimaris.Animations {

    /// <summary>
    /// Simple implementation of the interface <see cref="IAnimator"/>
    /// </summary>
    //HACK the existence of this class is in itself an hack: it hust so happens that Netcode for GameObjects doesn't synchronize BlendTree variables over the network,
    //so a custom component was needed to sync it manually, whose behaviour was abstracted in the interface IAnimator.
    //So we can just check on a IAnimator to change the animation of something, instead of using weird classes or behaviours
    [RequireComponent(typeof(Animator))]
    public class SimpleAnimator : MonoBehaviour, IAnimator {

        private Animator animator;

        public void SetBool(string parameterName, bool value) => animator.SetBool(parameterName, value);

        public void SetFloat(string parameterName, float value) => animator.SetFloat(parameterName, value);

        private void Awake() {
            animator = GetComponent<Animator>();
        }
    }
}
