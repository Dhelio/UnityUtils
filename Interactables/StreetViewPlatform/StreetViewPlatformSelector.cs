using DG.Tweening;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Castrimaris.IO.GoogleStreetView;
using Castrimaris.Singletons;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Castrimaris.Interactables {

    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class StreetViewPlatformSelector : MonoBehaviour {

        #region Private Variables

        [Header("Parameters")]
        [SerializeField] private string[] tagsToCheck = new string[] { "StreetView_POI" };

        [Header("References")]
        [SerializeField] private StreetViewSphereVisualizer sphereVisualizer;
        [SerializeField] private StreetViewPlatformTimeline timeline;
        [SerializeField] private TMPro.TextMeshPro label;

        [Header("Events")]
        [SerializeField] private UnityEvent<int> onPoiSelected;

        private new Rigidbody rigidbody;
        private SphereCollider sphereCollider;
        private int lastIndex = -1;

        #endregion

        #region Public Methods

        public void SelectNearest() {
            var selectorPosition = this.transform.position;
            var poisTransform = timeline.POIsTransform;
            var nearestTransform = poisTransform.GetChild(0);
            var nearestDistance = Vector3.Distance(selectorPosition, nearestTransform.position);
            for (int i = 1; i < poisTransform.childCount; i++) {
                var child = poisTransform.GetChild(i);
                var distance = Vector3.Distance(selectorPosition, child.position);
                if (distance < nearestDistance) {
                    nearestDistance = distance;
                    nearestTransform = child;
                }
            }

            var index = nearestTransform.GetSiblingIndex();
            if (lastIndex == index)
                return;

            lastIndex = index;
            var selectable = nearestTransform.GetComponent<StreetViewPlatformSelectable>();
            onPoiSelected.Invoke(index);
            AnimateText(selectable.PoiName);
            AnimateView(index);
        }

        public void SetSelected(int index) {
            lastIndex = index;
            var selectable = timeline.POIsTransform.GetChild(index).GetComponent<StreetViewPlatformSelectable>();
            label.text = selectable.PoiName;
        }

        #endregion

        #region Unity Overrides

        private void Awake() {
            //Validation
            if (sphereVisualizer == null) {
                sphereVisualizer = GameObject.FindObjectOfType<StreetViewSphereVisualizer>();
                if (sphereVisualizer == null)
                    throw new MissingReferenceException($"No reference set or found for {nameof(StreetViewSphereVisualizer)}. Please either assign one in Editor or create one.");
            }

            if (timeline == null) {
                timeline = GameObject.FindObjectOfType<StreetViewPlatformTimeline>();
                if (timeline == null)
                    throw new MissingReferenceException($"No reference set or found for {nameof(StreetViewPlatformTimeline)}. Please assign one in Editor.");
            }

            if (label == null) {
                if (!this.gameObject.TryGetComponentRecursive<TMPro.TextMeshPro>(out label)) {
                    throw new MissingReferenceException($"No reference set or found for {nameof(label)}. Please assign one in Editor.");
                }
            }

            rigidbody = GetComponent<Rigidbody>();
            sphereCollider = GetComponent<SphereCollider>();

            //Initialize
            label.text = "";
            rigidbody.useGravity = false;
            rigidbody.constraints =
                RigidbodyConstraints.FreezePositionX |
                RigidbodyConstraints.FreezePositionY |
                RigidbodyConstraints.FreezePositionZ |
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationY |
                RigidbodyConstraints.FreezeRotationZ;
        }

        private void OnTriggerEnter(Collider other) {
            if (!timeline.IsEnabled)
                return;

            if (!other.TryGetComponent<StreetViewPlatformSelectable>(out var selectable))
                return;

            
            AnimateText(selectable.PoiName);
        }

        #endregion

        #region Private Methods

        private void AnimateText(string text) {
            DOTween.Kill(this);
            var seq = DOTween.Sequence();
            seq.SetId(this);
            seq.Append(label.material.DOFade(0.0f, .5f).OnComplete(() => label.text = text));
            seq.Append(label.material.DOFade(1.0f, .5f));
            seq.Play();
        }

        private async void AnimateView(int viewIndex) {
            TransitionManager.Instance.FadeOut("StreetView Transition", .5f);
            await Task.Delay(500);
            sphereVisualizer.SetActiveView(viewIndex);
            TransitionManager.Instance.FadeIn("StreetView Transition", .5f);
        }

        #endregion

    }
}
