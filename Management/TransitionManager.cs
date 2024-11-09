using Castrimaris.Attributes;
using Castrimaris.Core;
using Castrimaris.Core.Monitoring;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Castrimaris.Singletons {

    /// <summary>
    /// Singleton class that handles transitions of both full screen effects and single objects effects.
    /// Requires a specific setup in the Universal Rendering Data asset of the URP renderer;
    /// specifically, it needs a Full Screen Pass Renderer feature for the Full Screen Transition, and one or more Render Objects as masks for that effect.
    /// </summary>
    public class TransitionManager : SingletonMonoBehaviour<TransitionManager> {

        #region Classes
        /// <summary>
        /// Data of the transition
        /// </summary>
        [System.Serializable]
        public class TransitionTuple {
            public enum TargetTypes { MATERIAL = 0, MESH_RENDERER = 1 }

            [Tooltip("Id of the transition. Can also be used to call the transition via script.")]
            public string Id;
            [Tooltip("The target of this transition")]
            public TargetTypes TargetType;
            [Tooltip("Reference to the material for the transition. Only used for TargetTypes.MATERIAL")]
            public Material MaterialTarget; //TODO add conditional field
            [Tooltip("Reference to the MeshRenderer for the transition. Only used for TargetTypes.MESH_RENDERER")]
            public MeshRenderer MeshRendererTarget; //TODO add conditional field
            [Tooltip("Keyword in the shader of the targe to manipulate. Must be a normalized float.")]
            public string Keyword;
        }
        #endregion

        #region Private Variables
        [Header("Parameters")]
        [Tooltip("If not specified, this is the time the transition will take to complete.")]
        [SerializeField, Range(0.0f, 10.0f)] private float defaultTransitionTime = 3.0f;
        [Tooltip("List of transitionable items.")]
        [SerializeField] private List<TransitionTuple> transitionables;

        private Dictionary<string, Coroutine> coroutines = new Dictionary<string, Coroutine>();
        #endregion

        #region Public Methods

        /// <summary>
        /// Plays a fade in transition.
        /// </summary>
        /// <param name="index">The index of the transitionable item to transition</param>
        /// <param name="duration">How long the transition will take. If it's less than 0, then it will apply the default value <see cref="defaultTransitionTime"/>.</param>
        [ExposeInInspector]
        public void FadeIn(int index, float duration = -1.0f) => InnerFade(index: index, startingValue: 0.0f, endingValue: 1.0f, duration: duration);

        /// <summary>
        /// Plays a fade out transition.
        /// </summary>
        /// <param name="index">The index of the transitionable item to transition</param>
        /// <param name="duration">How long the transition will take. If it's less than 0, then it will apply the default value <see cref="defaultTransitionTime"/>.</param>
        [ExposeInInspector]
        public void FadeOut(int index, float duration = -1.0f) => InnerFade(index: index, startingValue: 1.0f, endingValue: 0.0f, duration: duration);

        /// <summary>
        /// Plays a fade in transition after a set time.
        /// </summary>
        /// <param name="index">The index of the transitionable item to transition</param>
        /// <param name="delay">After how long the transition should start</param>
        /// <param name="duration">How long the transition will take. If it's less than 0, then it will apply the default value <see cref="defaultTransitionTime"/>.</param>
        public void DelayedFadeIn(int index, float delay, float duration = -1.0f) => InnerFade(index: index, startingValue: 0.0f, endingValue: 1.0f, delay: delay, duration: duration);

        /// <summary>
        /// Plays a fade out transition after a set time.
        /// </summary>
        /// <param name="index">The index of the transitionable item to transition</param>
        /// <param name="delay">After how long the transition should start</param>
        /// <param name="duration">How long the transition will take. If it's less than 0, then it will apply the default value <see cref="defaultTransitionTime"/>.</param>
        public void DelayedFadeOut(int index, float delay, float duration = -1.0f) => InnerFade(index: index, startingValue: 1.0f, endingValue: 0.0f, delay: delay, duration: duration);

        /// <summary>
        /// Plays a fade in animation.
        /// </summary>
        /// <param name="id">The id of the transitionable item to transition</param>
        /// <param name="duration">How long the transition will take. If it's less than 0, then it will apply the default value <see cref="defaultTransitionTime"/>.</param>
        public void FadeIn(string id, float duration = -1.0f) {
            if (FindMaterialIndexById(id, out var index))
                FadeIn(index: index, duration: duration);
        }

        /// <summary>
        /// Plays a fade out animation.
        /// </summary>
        /// <param name="id">The id of the transitionable item to transition</param>
        /// <param name="duration">How long the transition will take. If it's less than 0, then it will apply the default value <see cref="defaultTransitionTime"/>.</param>
        public void FadeOut(string id, float duration = -1.0f) {
            if (FindMaterialIndexById(id, out var index))
                FadeOut(index: index, duration: duration);
        }

        /// <summary>
        /// Plays a fade in animation.
        /// </summary>
        /// <param name="id">The id of the transitionable item to transition</param>
        /// <param name="delay">After how long the transition should start</param>
        /// <param name="duration">How long the transition will take. If it's less than 0, then it will apply the default value <see cref="defaultTransitionTime"/>.</param>
        public void DelayedFadeIn(string id, float delay, float duration = -1.0f) {
            if (FindMaterialIndexById(id, out var index))
                DelayedFadeIn(index: index, delay: delay, duration: duration) ;
        }

        /// <summary>
        /// Plays a fade out animation.
        /// </summary>
        /// <param name="id">The id of the transitionable item to transition</param>
        /// <param name="delay">After how long the transition should start</param>
        /// <param name="duration">How long the transition will take. If it's less than 0, then it will apply the default value <see cref="defaultTransitionTime"/>.</param>
        public void DelayedFadeOut(string id, float delay, float duration = -1.0f) {
            if (FindMaterialIndexById(id, out var index))
                DelayedFadeOut(index: index, delay: delay, duration: duration);
        }

        #endregion

        #region Private Methods
        private void InnerFade(int index, float startingValue, float endingValue, float delay = 0.0f, float duration = -1.0f) {
            if (index < 0 || index >= transitionables.Count)
                return;

            var transitionableMaterial = transitionables[index];
            if (coroutines.TryGetValue(transitionableMaterial.Keyword, out var coroutine)) {
                StopCoroutine(coroutine);
                coroutines.Remove(transitionableMaterial.Keyword);
            }

            coroutines.Add(transitionableMaterial.Keyword, StartCoroutine(Fade(transitionableMaterial, startingValue, endingValue, delay, duration)));
        }

        private bool FindMaterialIndexById(string id, out int index) {
            for (int i = 0; i < transitionables.Count; i++) {
                if (transitionables[i].Id == id) {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }

        private IEnumerator Fade(TransitionTuple transitionTuple, float startingValue, float endingValue, float delay=0.0f, float duration = -1.0f) {
            duration = (duration >= 0.0f) ? duration : defaultTransitionTime;
            //Log.D($"Starting fading for keyword {transitionTuple.Keyword} from {startingValue} to {endingValue} with delay {delay} and duration {duration}");
            var wfs = new WaitForSeconds(delay);
            yield return wfs;
            var startingTime = Time.time;
            float time = 0.0f;
            while (time < duration) {
                time = Time.time - startingTime;
                var amount = Mathf.Lerp(startingValue, endingValue, time / duration);
                switch (transitionTuple.TargetType) {
                    case TransitionTuple.TargetTypes.MATERIAL:
                        transitionTuple.MaterialTarget.SetFloat(transitionTuple.Keyword, amount);
                        break;
                    case TransitionTuple.TargetTypes.MESH_RENDERER:
                        transitionTuple.MeshRendererTarget.material.SetFloat(transitionTuple.Keyword, amount);
                        break;
                    default:
                        Log.E($"No such TargetType!");
                        break;
                }
                yield return null;
            }
        }

        [ExposeInInspector]
        [ContextMenu("Reset Transitions")]
        private void ResetTransitions() {
            for (int i = 0; i < transitionables.Count; i++) {
                FadeOut(i, 0);
            }
        }
        #endregion
    }
}
