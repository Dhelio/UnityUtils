using Castrimaris.Core;
using Castrimaris.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Castrimaris.Core.Monitoring;
using Castrimaris.Attributes;
using System;
using Castrimaris.Math;




//TODO Move in another class
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

namespace Castrimaris.Rendering {

    [Obsolete("The functionality of this class has been moved to "+nameof(SkyboxManager)+". This is here only for historic reference.")]
    public class DynamicLightmapsManager : SingletonMonoBehaviour<DynamicLightmapsManager> {

        [Header("References")]
        [SerializeField] private Light bakedLight;
        [SerializeField] private List<DynamicLightmapScenario> scenarios;
        [SerializeField] private List<DynamicLightmap> dynamicLightmaps;

        private int activeDynamicLightmapIndex = -1;

        public void SetScenario(DynamicLightmapScenario scenario) {
            
        }

        public void SetLightingScenario(int ScenarioIndex) {
            if (ScenarioIndex < 0 || ScenarioIndex > scenarios.Count) {
                Log.E($"Index outside range of Scenarios!");
                return;
            }
            if (activeDynamicLightmapIndex == ScenarioIndex)
                return;
            activeDynamicLightmapIndex = ScenarioIndex;
            var scenario = scenarios[ScenarioIndex];
            scenario.LoadLightmaps();
            scenario.LoadDynamicLightmaps(dynamicLightmaps);
            scenario.LoadLightProbes();
            //scenario.ApplyLightParameters(bakedLight);
        }

        public void SetLightingScenario(string ScenarioName) {
            var scenario = scenarios.Where(scenario => scenario.name == ScenarioName).FirstOrDefault();
            if (scenario == null)
                return;
            if (scenario == scenarios[activeDynamicLightmapIndex])
                return;
            activeDynamicLightmapIndex = scenarios.IndexOf(scenario);
            scenario.LoadLightmaps();
            scenario.LoadDynamicLightmaps(dynamicLightmaps);
            scenario.LoadLightProbes();
            //scenario.ApplyLightParameters(bakedLight);
        }

        protected override void Awake() {
            base.Awake();
            SetLightingScenario(0);
        }

        [ExposeInInspector]
        private void CycleScenario() {
            int targetIndex = FMath.EllipticClamp(activeDynamicLightmapIndex + 1, 0, scenarios.Count - 1);
            Log.D($"Current active lightmap index: {targetIndex}");
            SetLightingScenario(targetIndex);
        }

        //TODO Move in another class
#if UNITY_EDITOR

        private void Reset() {
            var result = new List<DynamicLightmapScenario>();
            var parametersGuids = AssetDatabase.FindAssets($"t:{nameof(DynamicLightmapScenario)}");
            foreach (var parameterGuid in parametersGuids) {
                var parameter = AssetDatabase.LoadAssetAtPath<DynamicLightmapScenario>(AssetDatabase.GUIDToAssetPath(parameterGuid));
                if (parameter.TargetSceneName == EditorSceneManager.GetActiveScene().name)
                    result.Add(parameter);
            }
            scenarios = result.ToList();

            dynamicLightmaps = GameObject.FindObjectsOfType<DynamicLightmap>().ToList();

            bakedLight = GameObject.FindObjectsOfType<Light>().Where(x => x.lightmapBakeType == LightmapBakeType.Baked).First();
        }
#endif
    }

}
