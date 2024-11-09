using Castrimaris.Attributes;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Castrimaris.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AzureSky;
using Castrimaris.Math;
using System;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

//std Parameters
//minNightTime =            19f;
//maxNightTime =            6.4f;
//minDawnTime =             6.4f;
//maxDawnTime =             10f;
//minEarlyMorningTime =     10f;
//maxEarlyMorningTime =     12f;
//minMorningTime =          12f;
//maxMorningTime =          16f;
//minEveningTime =          16f;
//maxEveningTime =          17.5f;
//minSunsetTime =           17.5f;
//maxSunsetTime =           19f;

namespace Castrimaris.Rendering {

    [RequireComponent(typeof(AzureTimeController))]
    public class SkyboxManager : NetworkBehaviour {

        [System.Serializable]
        public struct Scenario {
            [HideInInspector] public string id;
            public DynamicLightmapScenario dynamicLightmapScenario;
            [Range(0, 24)] public float minTime;
            [Range(0, 24)] public float maxTime;
        }

        [Header("Parameters")]
        [SerializeField] private Scenario[] scenarios;
        [SerializeField] private bool useRealTime = false;
        [SerializeField] private NetworkVariable<float> currentSkyTime = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [SerializeField] private NetworkVariable<int> currentActiveScenario = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        [Header("References")]
        [SerializeField] private DynamicLightmap[] dynamicLightmaps;

        private AzureTimeController timeController;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();

            //If it's the server then update the sky variable with the first value in the scenarios
            if (IsServer) {
                if (!useRealTime)
                    currentSkyTime.Value = scenarios.First().minTime;
                else {
                    var utcTime = DateTime.UtcNow;
                    var romeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"); //HACK: european timezone SHOULD be pretty stable, but there is an evident problem here: timezones ids change with governments regulations. What is here today might not be here tomorrow. Maybe find a way to change this to be more "consistent"...or maybe not. No program is eternal, everything is subject to entropy.
                    var romeTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, romeTimeZone);
                    currentSkyTime.Value = romeTime.Hour + (romeTime.Minute / 60.0f) + (romeTime.Second / 3600.0f); //Convert the datetime to a single float value between 0 and 24
                }
            }

            if (useRealTime)
                timeController.SetNewDayLength(24 * 60); //24 hours times 60 minutes

            //Update the time controller with the networked time value
            timeController.SetTimeline(currentSkyTime.Value);
            ApplyLightmaps(currentActiveScenario.Value);
        }

        private void Awake() {
            timeController = GetComponent<AzureTimeController>();
        }

        private void Update() {
            if (!IsServer)
                return;

            //Update the value, so connecting client will know the time.
            currentSkyTime.Value = timeController.GetTimeline();

            //Check if the time of day has changed, then update the lightmaps
            for (int i = 0; i < scenarios.Length; i++) {
                var scenario = scenarios[i];
                if (currentSkyTime.Value.IsEllipticallyBetween(scenario.minTime, scenario.maxTime)) {
                    if (currentActiveScenario.Value == i) //TODO might be optimizable
                        return;
                    currentActiveScenario.Value = i;
                    ChangeDynamicLightmaps(currentActiveScenario.Value);
                }
            }
        }

        /// <summary>
        /// Updates the lightmaps and lightprobes server side
        /// </summary>
        /// <param name="index">The index of the scenarios to apply</param>
        private void ChangeDynamicLightmaps(int index) {
            ChangeDynamicLightmapsClientRpc(index);
            ApplyLightmaps(index);
        }

        /// <summary>
        /// Updates the lightmaps and lightprobes client side
        /// </summary>
        /// <param name="index">The index of the scenarios to apply</param>
        [ClientRpc]
        private void ChangeDynamicLightmapsClientRpc(int index) => ApplyLightmaps(index);

        /// <summary>
        /// Changes the lightmaps and lightprobes with the indicated ones
        /// </summary>
        /// <param name="index">The index of the scenarios to apply</param>

        private void ApplyLightmaps(int index) {
            var scenario = scenarios[index].dynamicLightmapScenario;
            scenario.LoadLightmaps();
            scenario.LoadDynamicLightmaps(dynamicLightmaps);
            scenario.LoadLightProbes();
        }

#if UNITY_EDITOR
        [ExposeInInspector]
        private void CycleScenario() {
            currentActiveScenario.Value = FMath.EllipticClamp(currentActiveScenario.Value + 1, 0, scenarios.Length - 1);
            Log.D($"Current active lightmap index: {currentActiveScenario.Value}");
            ApplyLightmaps(currentActiveScenario.Value);
        }

        private void Reset() {
            var result = new List<DynamicLightmapScenario>();
            var parametersGuids = AssetDatabase.FindAssets($"t:{nameof(DynamicLightmapScenario)}");
            foreach (var parameterGuid in parametersGuids) {
                var parameter = AssetDatabase.LoadAssetAtPath<DynamicLightmapScenario>(AssetDatabase.GUIDToAssetPath(parameterGuid));
                if (parameter.TargetSceneName == EditorSceneManager.GetActiveScene().name)
                    result.Add(parameter);
            }

            scenarios = new Scenario[result.Count];
            for (int i = 0; i < scenarios.Length; i++) {
                scenarios[i].id = result[i].Id;
                scenarios[i].dynamicLightmapScenario = result[i];
                //The following are hardcoded values for pre-defined scenario settings.
                //If light conditions during baking change, then this won't be true anymore!
                //But for the time being, we'll keep it this way to remember the lighting values
                switch (i) {
                    case 0:
                        //Night time
                        scenarios[i].minTime = 19f;
                        scenarios[i].maxTime = 6.4f;
                        break;
                    case 1:
                        //Dawn time
                        scenarios[i].minTime = 6.4f;
                        scenarios[i].maxTime = 10f;
                        break;
                    case 2:
                        //Early morning time
                        scenarios[i].minTime = 10f;
                        scenarios[i].maxTime = 12f;
                        break;
                    case 3:
                        //Late morning time
                        scenarios[i].minTime = 12f;
                        scenarios[i].maxTime = 16f;
                        break;
                    case 4:
                        //Evening time
                        scenarios[i].minTime = 16f;
                        scenarios[i].maxTime = 17.5f;
                        break;
                    case 5:
                        //Dusk time
                        scenarios[i].minTime = 17.5f;
                        scenarios[i].maxTime = 19f;
                        break;
                    default:
                        //Default time
                        scenarios[i].minTime = 0;
                        scenarios[i].maxTime = 0;
                        break;
                }
            }

            dynamicLightmaps = GameObject.FindObjectsOfType<DynamicLightmap>().ToArray();
        }
#endif
    }
}