using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Castrimaris.Core {

    /// <summary>
    /// Plays looping sounds with a start and a finish, like revving motors, elevators, engines, really any sound which has a start, middle looping section and end.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class RevvingSoundSequence : MonoBehaviour {

        #region PRIVATE VARIABLES
        [Header("Parameters")]
        [Tooltip("Wheter when Playing the sounds the last element of the starting sequence should be looped")]
        [SerializeField] private bool loopStartingSequence = false;

        [Header("References")]
        [SerializeField] private AudioClip[] startingSequence;
        [SerializeField] private AudioClip[] endingSequence;

        private AudioSource audioSource;
        #endregion

        #region PUBLIC METHODS
        /// <summary>
        /// Plays the starting sounds
        /// </summary>
        public void Play() {
            StopAllCoroutines();
            StartCoroutine(PlayBehaviour(startingSequence, loopStartingSequence));
        }

        /// <summary>
        /// Plays the stopping sounds
        /// </summary>
        public void Stop() {
            StopAllCoroutines();
            StartCoroutine(PlayBehaviour(endingSequence, false)); //Last element of stopping sequence is never looped!
        }
        #endregion

        #region UNITY OVERRIDES
        private void Awake() {
            audioSource = GetComponent<AudioSource>();
        }
        #endregion

        #region PRIVATE METHODS
        /// <summary>
        /// Plays an <see cref="AudioClip"/> array sequentially until the last element. The last element can be looped to continually play until stopped.
        /// </summary>
        private IEnumerator PlayBehaviour(AudioClip[] Clips, bool LoopEnd) {
            audioSource.loop = false;
            for (int i = 0; i < Clips.Length-1; i++) {
                audioSource.clip = Clips[i];
                audioSource.Play();
                while (audioSource.isPlaying) {
                    yield return null;
                }
            }
            audioSource.clip = Clips.Last();
            audioSource.loop = LoopEnd;
            audioSource.Play();
        }
        #endregion
    }

}
