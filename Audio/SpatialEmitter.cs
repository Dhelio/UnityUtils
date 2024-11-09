using Castrimaris.Attributes;
using Castrimaris.Core.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Castrimaris.Audio {

    //TODO make more "probes" around the object; each probe raycasts the AudioListener, and each occluded probe reduces audio of a percentage of the occluder value
    //TODO see https://developer.atmoky.com/true-spatial-unity/docs/audiosource-properties
    /// <summary>
    /// Simple spatial implementation of an <see cref="AudioSource"/>.
    /// If enabled, raycasts towards the <see cref="AudioListener"/> and checks if there are any <see cref="SpatialOccluder">SpatialOccluders</see>, reducing the volume contextually
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(SphereCollider))]
    public class SpatialEmitter : MonoBehaviour {

        #region Private Variables

        [Header("References")]
        [SerializeField] private List<SpatialOccluder> occluders = new List<SpatialOccluder>();

        [Header("ReadOnly Parameters")]
        [SerializeField, ReadOnly] private bool isEnabled = false;
        [SerializeField, ReadOnly, Range(0f,1f)] private float currentOcclusionPercentage = 0f;

        [Header("ReadOnly References")]
        [SerializeField, ReadOnly] private SpatialListener listener = null;
        [SerializeField, ReadOnly] private AudioSource source = null;
        [SerializeField, ReadOnly] private SphereCollider sphereCollider;

        #endregion

        #region Public Methods

        public void Activate(SpatialListener targetListener) {
            listener = targetListener;
            isEnabled = true;


            //HACK: we could disable/enable the audiosource, but then Dissonance would stop working; we opt to set the volume to 0 when disabled, and to let the Update function set the volume when enabled
            //source.enabled = true; 
        }

        public void Deactivate() {
            listener = null;
            isEnabled = false;


            //HACK: we could disable/enable the audiosource, but then Dissonance would stop working; we opt to set the volume to 0 when disabled, and to let the Update function set the volume when enabled
            //source.enabled = false;
            source.volume = 0; 
        }

        #endregion

        #region Unity Overrides

        private void Awake() {
            sphereCollider = GetComponent<SphereCollider>();
            source = GetComponent<AudioSource>();

            sphereCollider.isTrigger = true;
            source.volume = 0f;
            sphereCollider.radius = source.maxDistance;
        }

        private void Update() {
            if (!isEnabled)
                return;

            if (listener == null)
                return;

            var direction = listener.transform.position - this.transform.position;
            var distance = Vector3.Distance(listener.transform.position, this.transform.position);
            var ray = new Ray(this.transform.position, direction);
            var hits = Physics.RaycastAll(ray, distance);
            if (hits.Length <= 0)
                return;

            occluders = (from hit in hits
                             where hit.collider.GetComponent<SpatialOccluder>() != null
                             select hit.collider.GetComponent<SpatialOccluder>()).ToList();

            var occlusionPercentage = 0f;
            foreach (var occluder in occluders) {
                occlusionPercentage += occluder.OcclusionPercentage;
            }

            var volume = Mathf.Clamp(100f - occlusionPercentage, 0f, 100f) / 100f;
            occlusionPercentage = volume;
            source.volume = volume;

            var lineColor = (volume > .66f) ? Color.green : (volume > .33f) ? Color.yellow : Color.red;
            Debug.DrawLine(ray.origin, listener.transform.position, lineColor);
        }

        #endregion
    }
}