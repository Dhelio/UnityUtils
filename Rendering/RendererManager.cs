using Castrimaris.Attributes;
using Castrimaris.Core.Monitoring;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Castrimaris.Rendering {

    /// <summary>
    /// Swaps a renderer at runtime
    /// </summary>
    public class RendererManager : MonoBehaviour {
        [Header("Parameters")]
        [SerializeField] private int targetRendererIndex = 0;
        [SerializeField] private InitializationTypes initializationType = InitializationTypes.OnAwake;
        [SerializeField] private bool enablePostProcessing = false;

        [Header("Antialiasing Parameters")]
        [SerializeField] private bool overrideAntialiasingSettings = false;

        [ConditionalField(nameof(overrideAntialiasingSettings), true, DisablingTypes.Hidden)]
        [SerializeField] private bool useMSAA = false;

        [ConditionalField(nameof(overrideAntialiasingSettings), true, DisablingTypes.Hidden)]
        [ConditionalField(nameof(useMSAA), true, DisablingTypes.Hidden)]
        [SerializeField, Range(0, 4)] private int msaaSampleCount = 0;

        [ConditionalField(nameof(overrideAntialiasingSettings), true, DisablingTypes.Hidden)]
        [ConditionalField(nameof(useMSAA), false, DisablingTypes.Hidden)]
        [SerializeField] private AntialiasingMode antialiasingMode = AntialiasingMode.None;

        [ConditionalField(nameof(overrideAntialiasingSettings), true, DisablingTypes.Hidden)]
        [ConditionalField(nameof(useMSAA), false, DisablingTypes.Hidden)]
        [SerializeField] private AntialiasingQuality antialiasingQuality = AntialiasingQuality.Medium;

        [Header("References")]
        [SerializeField] private Camera targetCamera = null;

        private UniversalAdditionalCameraData data;

        public Camera TargetCamera {
            get {
                if (targetCamera == null)
                    targetCamera = Camera.main;
                return targetCamera;
            }
        }

        [ExposeInInspector]
        public void SetupRenderer() {
            //Skip setup in case this is a server build, since there will be no camera rendering
            try {
                if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsHost)
                    return;
            } catch {
                Log.W("No NetworkManager found, assuming this is single player mode.");
            }

            RetrieveData();
            SetRenderer();
            SetPostProcessing();
            SetAntialiasing();
        }

        public void RetrieveData() {
            data = TargetCamera.GetComponent<UniversalAdditionalCameraData>();
        }

        public void SetPostProcessing() {
            //Enable the post proc flag on the camera
            data.renderPostProcessing = enablePostProcessing;

            //Enable the post proc flag in the Renderer //TODO
            //var pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            //var renderer = pipeline.GetRenderer(targetRendererIndex);
        }

        public void SetRenderer() {
            data.SetRenderer(targetRendererIndex);
        }

        public void SetAntialiasing() {
            if (!overrideAntialiasingSettings)
                return;

            TargetCamera.allowMSAA = useMSAA;

            if (!useMSAA) {
                data.antialiasing = antialiasingMode;
                data.antialiasingQuality = antialiasingQuality;
            } else {
                var rp = GraphicsSettings.defaultRenderPipeline;
                if (rp is not UniversalRenderPipelineAsset urpRp)
                    throw new System.Exception("");
                urpRp.msaaSampleCount = msaaSampleCount;
            }
        }


        private void Awake() {
            if (initializationType != InitializationTypes.OnAwake)
                return;

            SetupRenderer();
        }

        private void Start() {
            if (initializationType != InitializationTypes.OnStart) { 
                StartCoroutine(ActiveInitialization());
                return;
            }

            SetupRenderer();
        }

        private IEnumerator ActiveInitialization() {
            if (initializationType != InitializationTypes.OnDemand)
                yield break;

            while(TargetCamera == null)
                yield return null;

            SetupRenderer();
        }
        
    }

}
