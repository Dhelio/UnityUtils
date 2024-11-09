using Castrimaris.Attributes;
using Castrimaris.Core.Monitoring;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Castrimaris.Animations {

    /// <summary>
    /// Misc utilities for animations in Unity, among which delayed animation play, randomized play, flag commutation, and more.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimationUtilities : MonoBehaviour {

        private Animator animator;
        [Header("Functionalities")]
        [SerializeField] private bool playAnimationOnStartWithParameters = false;
        [SerializeField] private bool randomizeBlendtreeOverTime = false;

        #region PLAY_ANIMATION_ON_START_WITH_PARAMETERS
        [Space]
        [Header("Play Animation on Start With Parameters")]
        [ConditionalField(nameof(playAnimationOnStartWithParameters), true, DisablingTypes.Hidden)]
        [Tooltip("If starting an animation on a specific one, give the name of the animation to this parameter")]
        [SerializeField] private string startingAnimationName = null;
        [ConditionalField(nameof(playAnimationOnStartWithParameters), true, DisablingTypes.Hidden)]
        [Tooltip("The layer where the animation specified in " + nameof(startingAnimationName) + " is")]
        [SerializeField] private int startingAnimationLayer = 0;
        [ConditionalField(nameof(playAnimationOnStartWithParameters), true, DisablingTypes.Hidden)]
        [Tooltip("The normalized time of the animation specified in " + nameof(startingAnimationName) + " should start at")]
        [SerializeField] private float startingAnimationNormalizedTime = 0.0f;
        [Tooltip("The speed at which the starting animation should run")]
        [SerializeField] private float startingAnimationSpeed = 1.0f;
        #endregion

        #region BLENDTREE_RANDOMIZER
        [Space]
        [Header("Randomize BlendTree Over Time")]
        [ConditionalField(nameof(randomizeBlendtreeOverTime), true, DisablingTypes.Hidden)]
        [Range(0f, 3f), SerializeField] private float blendTreeInterpolationSpeed = 1f;


        private Dictionary<string, Coroutine> blendtreeRandomizations = null;
        #endregion

        /// <summary>
        /// Gets the animator boolean with the specified name, then commutes its value.
        /// </summary>
        public void CommuteAnimatorFlag(string FlagName) {
            animator.SetBool(FlagName, !animator.GetBool(FlagName));
        }

        public void StartRandomizingBlendtreeOverTime(string BlendTreeParameter) {
            if (!randomizeBlendtreeOverTime) {
                Log.E($"Tried to use blendtree randomization, but it's disabled for this {nameof(AnimationUtilities)}. Enabled it first.");
                return;
            }
            if (blendtreeRandomizations == null)
                blendtreeRandomizations = new Dictionary<string, Coroutine>();
            blendtreeRandomizations.Add(BlendTreeParameter, StartCoroutine(BlendtreeRandomizer(BlendTreeParameter)));
        }

        public void StopRandomizingBlendtreeOverTime(string BlendTreeParameter) {
            if (!randomizeBlendtreeOverTime) {
                Log.E($"Tried to use blendtree randomization, but it's disabled for this {nameof(AnimationUtilities)}. Enabled it first.");
                return;
            }
            if (blendtreeRandomizations == null)
                return;
            var coroutine = blendtreeRandomizations[BlendTreeParameter];
            blendtreeRandomizations.Remove(BlendTreeParameter);
            StopCoroutine(coroutine);
        }

        /// <summary>
        /// Plays a specific state at reverse speed.
        /// </summary>
        /// <param name="StateName">The name of the state to play</param>
        public void PlayReversed(string StateName) {
            animator.Play(StateName, 0, 1f);
            animator.speed = -1f;
        }

        /// <summary>
        /// Plays the current state in reverse.
        /// </summary>
        public void PlayCurrentStateReversed() {
            var stateNameHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            animator.Play(stateNameHash, 0, 1f);
            animator.speed = -1f;
        }

        private void Awake() {
            animator = GetComponent<Animator>();
            StartAnimatorAtParameters();
        }

        /// <summary>
        /// Starts the animator at the specific point defined in the parameters
        /// </summary>
        private void StartAnimatorAtParameters() {
            if (startingAnimationName == null)
                return;

            animator.Play(startingAnimationName, startingAnimationLayer, startingAnimationNormalizedTime);
            animator.speed = startingAnimationSpeed;
        }

        private IEnumerator BlendtreeRandomizer(string BlendTreeName) {
            while (true) {
                var currentBlendTreeValue = animator.GetFloat(BlendTreeName);
                var targetBlendTreeValue = Random.Range(0f, 1f);
                var distance = Mathf.Abs(currentBlendTreeValue - targetBlendTreeValue);
                float t = 0;
                float blendTreeValue = float.NegativeInfinity;

                while (!Mathf.Approximately(blendTreeValue, targetBlendTreeValue)) {
                    t += blendTreeInterpolationSpeed * Time.deltaTime;
                    blendTreeValue = Mathf.Lerp(currentBlendTreeValue, targetBlendTreeValue, t / distance);
                    animator.SetFloat(BlendTreeName, blendTreeValue);
                    yield return null;
                }
            }
        }

    }

}