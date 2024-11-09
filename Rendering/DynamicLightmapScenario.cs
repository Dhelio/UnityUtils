using Castrimaris.Core.Collections;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Castrimaris.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Castrimaris.Core;



//TODO move this to a different class
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Castrimaris.ScriptableObjects {

    /// <summary>
    /// Scriptable Object that stores data used by <see cref="DynamicLightmap"/>s in scene.
    /// </summary>
    [CreateAssetMenu(fileName = "DynamicLightmapScenario", menuName = "Castrimaris/ScriptableObjects/DynamicLightmapScenario")]
    public class DynamicLightmapScenario : ScriptableObject {

        #region STRUCTS
        /// <summary>
        /// Parameters of the baking light for this scenario
        /// </summary>
        [System.Serializable]
        public struct LightParameters {
            public Vector3 Rotation;
            public float Intensity;
            public float IndirectIntensity;

            public LightParameters(Vector3 Rotation, float Intensity, float IndirectIntensity) {
                this.Rotation = Rotation;
                this.Intensity = Intensity;
                this.IndirectIntensity = IndirectIntensity;
            }
        }
        #endregion

        #region VARIABLES

        [Header("Parameters")]
        [Tooltip("The name of the scene where this scenario will be used")]
        public string TargetSceneName = null;
        [Tooltip("Id of the scenario.")]
        public string Id = null;
        [Tooltip("Wheter the scenario can enable emissive materials in DynamicLightmaps with the relative setting enabled.")]
        public bool UsesOptionalEmissives = false;
        [Tooltip("Parameters of the baking light for this scenario")]
        public LightParameters Light;
        #endregion

        #region PROPERTIES
        public string EditorScenarioResourcesFolderPath => $"{Application.dataPath}/Resources/DynamicLightmaps/{TargetSceneName}/{Id}/";
        public string ResourcesPath => $"DynamicLightmaps/{TargetSceneName}/{Id}/";
        public string LightProbesFileName => $"Harmonics.asset";
        public string DynamicLightmapsFileName => $"MeshRenderers.asset";
        #endregion

        #region PUBLIC METHODS

        public void LoadLightmaps() {
            //We skip loading on server app, since it's an headless instance of the application (thus not having any texture/shader)
            if (Application.platform == RuntimePlatform.LinuxServer || 
                Application.platform == RuntimePlatform.WindowsServer || 
                Application.platform == RuntimePlatform.OSXServer)
                return;

            //Load directional maps and colour maps
            var textures = Resources.LoadAll<Texture2D>(ResourcesPath);

            //Determine which is which, because at runtime it can't be determined.
            var directionalMaps = (from texture in textures
                                   where texture.name.EndsWith("comp_dir")
                                   select texture).OrderBy(texture => texture.name).ToArray();
            var colorMaps = (from texture in textures
                             where texture.name.EndsWith("comp_light")
                             select texture).OrderBy(texture => texture.name, new AlphanumericComparer()).ToArray();

            //Initialize LightmapData array to be used by the lightmapping system
            int size = colorMaps.Length; //we use color maps because it's always baked. Directional are not baked if using non-directional lightmaps
            var lightmapData = new LightmapData[size];
            for (int i = 0; i < size; i++)
                lightmapData[i] = new LightmapData() {
                    lightmapColor = colorMaps[i],
                    lightmapDir = (directionalMaps.Length > 0) ? directionalMaps[i] : null //Directional maps MIGHT be null, so we're checking accordingly.
                };

            //Apply lightmap settings
            LightmapSettings.lightmaps = lightmapData;
            
        }

        public void LoadLightProbes() {
            var lightProbes = Resources.LoadAll<SphericalHarmonicsContainer>(ResourcesPath).First().Harmonics;
            LightmapSettings.lightProbes.bakedProbes = lightProbes;
        }

        public void LoadDynamicLightmaps(List<DynamicLightmap> DynamicLightmaps) {
            return; //TODO make this work
            var dynamicLightmapsData = Resources.LoadAll<DynamicLightmapsContainer>(ResourcesPath).First().Data;
            foreach (var dynamicLightmap in DynamicLightmaps) {
                dynamicLightmap.Data = dynamicLightmapsData[dynamicLightmap.Guid]; 
            }
        }

        public void LoadDynamicLightmaps(DynamicLightmap[] dynamicLightmaps) {
            return; //TODO make this work
        }

        public void ApplyLightParameters(Light Light) {
            Light.intensity = this.Light.Intensity;
            Light.bounceIntensity = this.Light.IndirectIntensity;
            Light.transform.rotation = Quaternion.Euler(this.Light.Rotation);
        }

        #endregion

        //TODO move this to a different class
        #region EDITOR METHODS
#if UNITY_EDITOR
        private void Reset() {
            if (TargetSceneName.IsNullOrEmpty())
                TargetSceneName = EditorSceneManager.GetActiveScene().name;
            if (Id.IsNullOrEmpty())
                Id = name;
            UsesOptionalEmissives = false;
        }
#endif
        #endregion

    }

}
