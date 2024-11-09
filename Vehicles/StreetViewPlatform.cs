using Castrimaris.Core;
using Castrimaris.Core.Monitoring;
using Castrimaris.Singletons;
using Castrimaris.Vehicles;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.Interactables {

    /// <summary>
    /// This controls the platform in the assistant scene that allows the user to see Google Street views.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class StreetViewPlatform : Vehicle {

        #region Private Variables
        private const string GLOBAL_ID = "Global Transition"; //id of the transition to black of the whole world in the TransitionManager
        private const string LOCAL_ID = "StreetView Transition"; //id of the transition to black of streetview overlay objects in the TransitionManager

        [Header(nameof(StreetViewPlatform)+" Parameters")]
        [Tooltip("Wheter the platform is enabled or not")]
        [SerializeField] private bool isEnabled = false;

        [Header(nameof(StreetViewPlatform) + " Events")]
        [Tooltip("Events called when the isEnabled flag goes from false to true")]
        [SerializeField] private UnityEvent onEnabled = new UnityEvent();
        [Tooltip("Events called when the isEnabled flag goes from true to false")]
        [SerializeField] private UnityEvent onDisabled = new UnityEvent();
        [Tooltip("Events called when the player enters the vehicle")]
        [SerializeField] private UnityEvent onPlayerEntered = new UnityEvent();
        [Tooltip("Events called when the player exits the vehicle")]
        [SerializeField] private UnityEvent onPlayerExited = new UnityEvent();
        #endregion

        #region Properties
        public UnityEvent OnEnabled => onEnabled;
        public UnityEvent OnDisabled => onDisabled;
        public UnityEvent OnPlayerEntered => onPlayerEntered;
        public UnityEvent OnPlayerExited => onPlayerExited;
        #endregion

        #region Public Methods

        public void SetEnable(bool Value) {
            if (isEnabled == Value)
                return;

            isEnabled = Value;
            if (isEnabled)
                onEnabled.Invoke();
            else
                onDisabled.Invoke();
        }

        public void Enable() => SetEnable(true);
        public void Disable() => SetEnable(false);

        #endregion

        #region Unity Overrides
        protected override void OnTriggerEnter(Collider collider) {
            if (!isEnabled)
                return;

            if (!base.CheckTriggerRequirements(collider, out _)) //TODO this can probably be deleted safely, as it already happens in base.OnTriggerEnter()
                return;

            base.OnTriggerEnter(collider);

            TransitionManager.Instance.FadeIn(id: GLOBAL_ID);
            TransitionManager.Instance.DelayedFadeIn(id: LOCAL_ID, delay: 2.0f, duration: 1.0f);

            onPlayerEntered.Invoke();
        }

        protected override void OnTriggerExit(Collider collider) {
            if (!isEnabled)
                return;

            if (!base.CheckTriggerRequirements(collider, out _))
                return;

            base.OnTriggerExit(collider);

            TransitionManager.Instance.FadeOut(id: GLOBAL_ID, duration: 1f);
            TransitionManager.Instance.FadeOut(id: LOCAL_ID, duration: 1f);

            onPlayerExited.Invoke();
        }
        #endregion
    }

}
