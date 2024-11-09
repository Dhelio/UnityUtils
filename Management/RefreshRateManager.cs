using Castrimaris.Core;
using UnityEngine;

namespace Castrimaris.Management {

    [DefaultExecutionOrder(0)]
    public class RefreshRateManager : SingletonMonoBehaviour<RefreshRateManager> {

        #region Private Variables

        [SerializeField] private float deviceRefreshRate;
        [SerializeField] private float refreshRate;
        [SerializeField] private float fixedTimeStep;

        #endregion

        #region Private Methods
        private void SetTimeStep() {
            if (refreshRate < 60f) {
                refreshRate = 90f;
            }
            Time.fixedDeltaTime = 1f / refreshRate;
        }
        #endregion

        #region Unity Overrides

        protected override void Awake() {
            base.Awake();
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start() {
            refreshRate = UnityEngine.XR.XRDevice.refreshRate;
            deviceRefreshRate = refreshRate;

            SetTimeStep();

            fixedTimeStep = Time.fixedDeltaTime;
        }

        #endregion

    }

}