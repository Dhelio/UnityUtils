using DG.Tweening;
using Castrimaris.Attributes;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Castrimaris.IO.GoogleDirections;
using Castrimaris.IO.GoogleStreetView;
using Castrimaris.IO.OpenAI;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;

namespace Castrimaris.Interactables {

    public class StreetViewPlatformTimeline : MonoBehaviour {

        #region Private Variables

        private readonly Vector3 startingLocalEulerAngles = new Vector3(-90.0f, 0.0f, 90.0f);

        [Header("Parameters")]
        [Tooltip("Hides the timeline on start")]
        [SerializeField] private bool startHidden = true;
        [SerializeField] private GameObject poiPrefab;
        [Tooltip("This is an HACK for Hurricane; Hurricane's Grabbable changes the layer to all children of the Grabbable. With this we revert to the needed Layer to correctly render the timeline model with the Fullscreen Pass Renderer feature active")]
        [SerializeField] private LayerMask timelineModelLayerOverride;
        [SerializeField, Range(.1f, 3.0f)] private float latchingDuration = .5f;

        [Header("References")]
        [SerializeField] private SplineContainer timelineSplineContainer;
        [SerializeField] private SplineInstantiate timelineSplineInstantiate;
        [SerializeField] private StreetViewPlatformSelector timelineSelector;
        [SerializeField] private StreetViewSphereVisualizer sphereVisualizer;
        //[SerializeField] private SplineExtrude timelineSplineExtrude; //TODO
        [SerializeField] private Transform poisTransform;

        private MeshRenderer timelineModel;
        private MeshRenderer selectorModel;
        private MeshRenderer[] poiModels;
        private float targetZRotation = 90.0f;
        private bool isEnabled = false;
        private Coroutine latchingBehaviour;
        private Collider[] colliders;

        #endregion

        #region Properties

        public int LastSelectedPOIIndex { set => targetZRotation = 90.0f - (value * (180.0f / (timelineSplineInstantiate.MinSpacing - 1.0f))); } //90 is the starting value of the timeline at its leftmost side, value the index of the sibling, 180 the max rotation achievable by the timeline, minspacing is the count of the values on the timeline minus one because we start from zero
        public bool IsEnabled { get => isEnabled; }
        public bool HasLatched { get; private set; }
        public Transform POIsTransform { get => poisTransform; }

        #endregion

        #region Public Methods

        public void CreateTimelinePOIs() => CreateTimelinePOIs(sphereVisualizer.Count);

        public void CreateTimelinePOIs(int poiCount) {
            //TODO maybe make the timeline spline bigger / shorter based on the number of POIs

            //Cleanup
            ClearPOIs();

            //Spawn
            SetupAndCreateSplineInstantiate(poiCount);
        }

        public void Show() {
            isEnabled = true;
            var poisModels = poisTransform.GetComponentsInChildren<MeshRenderer>(includeInactive: true);

            selectorModel.gameObject.SetActive(true);
            timelineModel.enabled = true;
            foreach (var poiModel in poisModels)
                poiModel.enabled = true;
        }

        public void Hide() {
            isEnabled = false;
            var poisModels = poisTransform.GetComponentsInChildren<MeshRenderer>(includeInactive: true);

            selectorModel.gameObject.SetActive(false);
            timelineModel.enabled = false;
            foreach (var poiModel in poisModels)
                poiModel.enabled = false;
        }

        public void Latch() {
            Unlatch();
            latchingBehaviour = StartCoroutine(LatchingBehaviour());
        }

        public void Unlatch() {
            if (latchingBehaviour != null)
                StopCoroutine(latchingBehaviour);
            HasLatched = false;
        }

        #endregion

        #region Unity Overrides

        private void Awake() {
            Validate();
            Initialize();
        }

        private async void Start() {
            if (startHidden)
                Hide();

            await HurricaneLayerHack(); //HACK: see the function summary.
        }

        private void Update() {
            if (!HasLatched)
                return;

            //Snap to target
            var rot = startingLocalEulerAngles;
            rot.z = targetZRotation;
            transform.localRotation = Quaternion.Euler(rot);
        }

        private void Reset() {
            Initialize();
        }

        #endregion

        #region Private Methods

        private IEnumerator LatchingBehaviour() {
            var initialRotation = this.transform.localRotation;
            var targetEulerRotation = startingLocalEulerAngles;
            targetEulerRotation.z = targetZRotation;
            var targetRotation = Quaternion.Euler(targetEulerRotation);
            var elapsedTime = 0.0f;
            do {
                elapsedTime += Time.deltaTime;
                var delta = elapsedTime / latchingDuration;
                transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, delta);
                yield return null;
            } while (elapsedTime < latchingDuration);
            transform.localRotation = targetRotation;
            HasLatched = true;
        }

        /// <summary>
        /// HACK: this function changes the timeline model layer after a set amount of time.
        /// This is necessary because Hurricane's Grabbable changes the layer to all children objects of the grabbable. It does this in case there's a collider that needs to be grabbed.
        /// Since the model GameObject doesn't have any collider we can safely change the layer of the object without affecting the inner workings of Hurricane.
        /// We change the layer because it's needed to correctly draw the object while the Fullscreen Renderer Pass Feature of URP is active.
        /// </summary>
        /// <returns></returns>
        private async Task HurricaneLayerHack() {
            await Task.Delay(2000);
            timelineModel.gameObject.layer = LayerMask.NameToLayer("Player");
        }

        private void ClearPOIs() => poisTransform.DestroyChildsImmediate();

        private void SetupAndCreateSplineInstantiate(int poiCount) {
            timelineSplineInstantiate.InstantiateMethod = SplineInstantiate.Method.InstanceCount;
            timelineSplineInstantiate.itemsToInstantiate = new SplineInstantiate.InstantiableItem[1] {
                new SplineInstantiate.InstantiableItem {
                    Prefab = poiPrefab,
                    Probability = 100
                }
            };
            timelineSplineInstantiate.ForwardAxis = SplineComponent.AlignAxis.ZAxis;
            timelineSplineInstantiate.UpAxis = SplineComponent.AlignAxis.YAxis;
            timelineSplineInstantiate.CoordinateSpace = SplineInstantiate.Space.World;
            timelineSplineInstantiate.MinSpacing = poiCount; //HACK: using the InstanceCount method this gets used as the lower limit of object to spawn
            timelineSplineInstantiate.MaxSpacing = poiCount; //HACK: using the InstanceCount method this gets used as the upper limit of object to spawn

            timelineSplineInstantiate.UpdateInstances();

            for (int i = 0; i < timelineSplineInstantiate.instances.Count; i++) {
                var poi = timelineSplineInstantiate.instances[i];
                poi.name = "POI-" + i;
                poi.hideFlags = HideFlags.None;
                poi.transform.SetParent(poisTransform);
            }

            timelineSplineInstantiate.instances.Clear(); //Is this necessary?

            var steps = sphereVisualizer.CurrentActiveContainer.Steps;

            var selectables = GetComponentsInChildren<StreetViewPlatformSelectable>();
            for (int i = 0; i < selectables.Length; i++) {
                selectables[i].Setup(steps[i].HtmlInstructions); //TODO this is wrong, HtmlInstructions or the name of the POI should be encoded in some sort of metadata
            }

            //Hide the pois
            var poisModels = poisTransform.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
            foreach (var poiModel in poisModels)
                poiModel.enabled = false;

            //Latch on the first POI
            timelineSelector.SetSelected(0);
            LastSelectedPOIIndex = 0;
            Latch();
        }

        private void Initialize() {
            if (timelineSplineContainer == null) {
                if (!this.gameObject.TryGetComponentRecursive<SplineContainer>(out timelineSplineContainer)) {
                    timelineSplineContainer = this.gameObject.AddComponent<SplineContainer>();
                }
            }

            if (timelineSplineInstantiate == null) {
                if (!this.gameObject.TryGetComponentRecursive<SplineInstantiate>(out timelineSplineInstantiate)) {
                    timelineSplineInstantiate = this.gameObject.AddComponent<SplineInstantiate>();
                }
            }

            if (this.transform.Find("POIs") == null) {
                var pois = new GameObject("POIs");
                pois.transform.position = Vector3.zero;
                pois.transform.rotation = Quaternion.identity;
                pois.transform.SetParent(this.transform);
                poisTransform = pois.transform;
            }

            timelineModel = this.GetComponentInChildren<MeshRenderer>();
            selectorModel = GameObject.FindObjectOfType<StreetViewPlatformSelector>().GetComponentInChildren<MeshRenderer>();
            colliders = GetComponentsInChildren<Collider>();
        }

        private void Validate() {
            if (sphereVisualizer == null) {
                sphereVisualizer = GameObject.FindObjectOfType<StreetViewSphereVisualizer>();
                if (sphereVisualizer == null)
                    throw new MissingReferenceException($"No reference set or found for {nameof(StreetViewSphereVisualizer)}. Please either assign one in Editor or create one.");
            }

            if (poiPrefab == null)
                throw new MissingReferenceException($"No reference set for {nameof(poiPrefab)}! Make sure to assign the Prefab to the script, otherwise it won't work."); //TODO maybe load it from resources/addressables?

            if (timelineSelector == null) {
                timelineSelector = GameObject.FindObjectOfType<StreetViewPlatformSelector>();
                if (timelineSelector == null) {
                    throw new MissingReferenceException($"No reference set for object of type {nameof(StreetViewPlatformSelector)}. Make sure to assign a reference to a selector in the Editor.");
                }
            }
        }

        #endregion
    }
}
