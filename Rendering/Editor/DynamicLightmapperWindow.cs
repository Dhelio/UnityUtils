using Castrimaris.Core.Editor;
using Castrimaris.Core.Monitoring;
using Castrimaris.ScriptableObjects;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AzureSky;
using UnityEngine.SceneManagement;

namespace Castrimaris.Rendering.Editor {

    /// <summary>
    /// Editor tool to bake several lightmaps in a single scene & saving them in the Resources folder for later loading them at runtime.
    /// </summary>
    public class DynamicLightmapperWindow : EditorWindow {

        private Light targetLight;
        private DynamicLightmapScenario[] scenarios; //The various lighting scenarios for this scene
        private bool isBaking = false;
        private Vector2 lightmapParametersScrollGroupPosition = Vector2.zero; //Needed to correctly show the editor window.
        private List<float> azureValues = new List<float>();
        private bool useAzure = false; //Wheter or not to use AzureSkybox.
        private AzureTimeController azureTimeController; //HACK: since the previous in house developed skybox had rendering problems, I've added Azure Skybox temporarily™; it needs a reference here because it treats lighting a bit differently

        //BakingGUI Variables. BakingGUI is the interface that is shown when the light baking is in progress.
        private int currentBake = -1;
        private int totalBakes = -1;
        private const float y = 70, height = 40;

        private void OnGUI() {
            if (!isBaking) {
                Layout.BoolField("Use Azure", ref useAzure); 
                DrawReferences();
                DrawLightmapScenarios();
                DrawCommands();
            } else {
                DrawBakingGUI();
            }
        }

        private void OnEnable() {
            azureValues = new List<float>(); //HACK for the same reason stated above
            Lightmapping.bakeCompleted += OnBakeCompleted;
            Initialize();
        }

        private void OnDisable() {
            Lightmapping.bakeCompleted -= OnBakeCompleted;
        }

        private void OnBakeCompleted() {
            isBaking = false;
        }

        /// <summary>
        /// Opens the window of the Lightmapper
        /// </summary>
        [MenuItem("Tools/Rendering/Dynamic Lightmapper")]
        private static void OpenWindow() {
            GetWindow<DynamicLightmapperWindow>("Dynamic Lightmapper");
        }

        private void Initialize() {
            //Retrieve a baking light
            var lights = FindObjectsOfType<Light>();
            var directionalLights = (from light in lights
                                     where light.type == LightType.Directional
                                     select light).ToList();
            if (directionalLights.Count <= 0) {
                Log.E($"Tried to look for light, but no directional lights were found! Add one.");
                return;
            } else if (directionalLights.Count >= 2) {
                Log.E($"Tried to look for light, but multiple directional lights were found! Using first one...");
            }
            targetLight = directionalLights.First();

            //Retrieve all scene specific scenarios
            var result = new List<DynamicLightmapScenario>();
            var scenariosGuids = AssetDatabase.FindAssets($"t:{nameof(DynamicLightmapScenario)}");
            foreach (var guid in scenariosGuids) {
                var scenario = AssetDatabase.LoadAssetAtPath<DynamicLightmapScenario>(AssetDatabase.GUIDToAssetPath(guid));
                if (scenario.TargetSceneName == EditorSceneManager.GetActiveScene().name)
                    result.Add(scenario);
            }
            scenarios = result.ToArray();

            azureTimeController = FindObjectOfType<AzureTimeController>();

            azureValues = new List<float>() { 0f, 6.4f, 10f, 12f, 16f, 17.5f }; //Hardcoded values for lazyness, but they may be changed in the window.
        }

        private void DrawBakingGUI() {
            Layout.LabelField("Baking in progress, please wait...");
            Layout.BoldLabelField("DO NOT MOVE LIGHTS / BAKE / MOVE STUFF AROUND!"); //This is because Unity stops the baking process while updating the Scene, and this system doesn't retry to bake automatically.
            EditorGUI.ProgressBar(new Rect(0, y, this.position.width, height), (float)currentBake/(float)totalBakes, $"Bake {currentBake} of {totalBakes}");
        }

        private void DrawReferences() {
            Layout.LabelField("ReadOnly References");
            Layout.ReadOnlyObjectField<Light>("Target Light", ref targetLight, true);
        }

        private void DrawLightmapScenarios() {
            Layout.ScrollGroup(ref lightmapParametersScrollGroupPosition, () => {
                for (int i = 0; i < scenarios.Length; i++) {
                    var scenario = scenarios[i];
                    Layout.LabelField($"Parameter {scenario.Id}");

                    Layout.Vector3Field("Rotation", ref scenario.Light.Rotation);

                    Layout.HorizontalGroup(() => {
                        Layout.FloatField("Intesity", ref scenario.Light.Intensity);
                        Layout.FloatField("Indirect", ref scenario.Light.IndirectIntensity);
                    });

                    Layout.HorizontalGroup(() => {
                        Layout.BoolField("Uses Optional Emissives", ref scenario.UsesOptionalEmissives);
                        if (useAzure) {
                            if (azureValues.Count < scenarios.Length)
                                azureValues.Add(-1f);
                            float tmp = azureValues[i];
                            Layout.FloatField("Azure Value", ref tmp);
                            azureValues[i] = tmp;
                        }
                    });

                    Layout.HorizontalGroup(() => {
                        Layout.Button("Save Light Data", () => {
                            scenario.Light.Rotation = targetLight.transform.rotation.eulerAngles;
                            scenario.Light.Intensity = targetLight.intensity;
                            scenario.Light.IndirectIntensity = targetLight.bounceIntensity;
                            if (useAzure) {
                                azureValues[i] = azureTimeController.GetTimeline();
                            }

                        });
                        Layout.Button("Set Light Data", () => {
                            targetLight.transform.rotation = Quaternion.Euler(scenario.Light.Rotation);
                            targetLight.intensity = scenario.Light.Intensity;
                            targetLight.bounceIntensity = scenario.Light.IndirectIntensity;
                            if (useAzure) {
                                azureTimeController.SetTimeline(azureValues[i]);
                            }
                        });
                    });

                    Layout.HorizontalGroup(() => {
                        Layout.Button("Bake Async", async () => {
                            totalBakes = 1;
                            currentBake = 0;
                            await BakeAsync(scenario, i: i);
                        });
                    });

                    Layout.Space();
                }
            });
        }

        private void DrawCommands() {
            EditorGUILayout.Space();
            Layout.Button("Bake All", async () => {

                RegenerateDynamicLightmapsGuids();

                totalBakes = scenarios.Length;
                currentBake = 0;

                //Bake each scenario
                for (int i = 0; i < scenarios.Length; i++) {
                    var scenario = scenarios[i];
                    await BakeAsync(scenario, i: i);
                }

            });
        }

        private List<DynamicLightmap> FindAllDynamicLightmaps() {
            var dynamicLightmaps = GameObject.FindObjectsOfType<DynamicLightmap>().ToList();
            return dynamicLightmaps;
        }

        private List<DynamicLightmap> FindAllOptionallyEmissiveDynamicLightmaps(List<DynamicLightmap> DynamicLightmaps) {
            var emissiveEnabledDynamicLightmaps = (from dynamicLightmap in DynamicLightmaps
                                                   where dynamicLightmap.IsOptionallyEmissive
                                                   select dynamicLightmap).ToList();
            return emissiveEnabledDynamicLightmaps;
        }

        private async Task BakeAsync(DynamicLightmapScenario Scenario, List<DynamicLightmap> DynamicLightmaps = null, List<DynamicLightmap> OptionallyEmissiveDynamicLightmaps = null, int i = -1) {
            //Editor tools
            currentBake++;

            //Clear previous lightmaps and set lighting for bake.
            Lightmapping.Clear();
            targetLight.shadows = LightShadows.Soft;
            targetLight.lightmapBakeType = LightmapBakeType.Baked;

            //Get dynamic Lightmaps
            if (DynamicLightmaps == null) {
                DynamicLightmaps = FindAllDynamicLightmaps();
            }
            if (OptionallyEmissiveDynamicLightmaps == null) {
                OptionallyEmissiveDynamicLightmaps = FindAllOptionallyEmissiveDynamicLightmaps(DynamicLightmaps);
            }

            //Set light to position
            if (useAzure) {
                azureTimeController.SetTimeline(azureValues[i]);
            } else {
                targetLight.transform.rotation = Quaternion.Euler(Scenario.Light.Rotation);
            }
            targetLight.intensity = Scenario.Light.Intensity;
            targetLight.bounceIntensity = Scenario.Light.IndirectIntensity;

            //Enable optional emissive materials
            OptionallyEmissiveDynamicLightmaps.ForEach(dynamicLightmap => dynamicLightmap.SetEmissive(Scenario.UsesOptionalEmissives)); //If the optional emissives are enabled for this parameter, use them!

            //Bake
            isBaking = true;
            Lightmapping.BakeAsync();
            while (isBaking) {
                await Task.Delay(1000);
            }

            //Create Resources folder
            try {
                Directory.Delete(Scenario.EditorScenarioResourcesFolderPath, true);
            } catch { /* If it gets here, then there's no folder */ }
            try {
                Directory.CreateDirectory(Scenario.EditorScenarioResourcesFolderPath);
            } catch { Log.E("Fatal error while trying to create directory!"); }

            //Save Dynamic Lightmaps data, Light Probes data and Lightmaps.
            SaveDynamicLightmaps(Scenario, DynamicLightmaps);
            SaveLightProbes(Scenario);

            //Save Lightmaps
            MoveLightmapsToResourcesFolder(Scenario);

            //Refresh asset database to show the changes.
            AssetDatabase.Refresh();

            //Done
            await Task.CompletedTask;
        }

        private void SaveDynamicLightmaps(DynamicLightmapScenario Scenario, List<DynamicLightmap> DynamicLightmaps) {
            var dynamicLightmapsContainer = ScriptableObject.CreateInstance<DynamicLightmapsContainer>();
            foreach (var dynamicLightmap in DynamicLightmaps) {
                dynamicLightmapsContainer.Data.Add(dynamicLightmap.Guid, dynamicLightmap.Data);
            }
            var savePath = $"Assets/Resources/{Scenario.ResourcesPath}{Scenario.DynamicLightmapsFileName}";
            AssetDatabase.Refresh();
            AssetDatabase.CreateAsset(dynamicLightmapsContainer, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void SaveLightProbes(DynamicLightmapScenario Scenario) {
            var sphericalHarmonicsContainer = ScriptableObject.CreateInstance<SphericalHarmonicsContainer>();
            sphericalHarmonicsContainer.Harmonics = LightmapSettings.lightProbes.bakedProbes;
            var savePath = $"Assets/Resources/{Scenario.ResourcesPath}{Scenario.LightProbesFileName}";
            AssetDatabase.Refresh();
            AssetDatabase.CreateAsset(sphericalHarmonicsContainer, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void MoveLightmapsToResourcesFolder(DynamicLightmapScenario Scenario) {
            //Preventive refresh, otherwise Unity laments of missing guids
            AssetDatabase.Refresh();

            var shortPath = SceneManager.GetActiveScene().path.Replace(".unity", "/");
            shortPath = shortPath.Remove(0, "Assets".Count());
            var completePath = Application.dataPath + shortPath;

            foreach (var file in Directory.GetFiles(completePath)) {
                var fileName = Path.GetFileName(file);
                if (!fileName.Contains("Lightmap") && !fileName.Contains("ReflectionProbe")) //Copy just colour lightmaps, directional lightmaps and reflection probes
                    continue;
                var destFileName = Path.Combine(Scenario.EditorScenarioResourcesFolderPath, fileName);
                File.Copy(file, destFileName, true);
            }
        }

        [MenuItem("Tools/Rendering/Regenerate All Dynamic Lightmaps GUIDs")]
        private static void RegenerateDynamicLightmapsGuids() {
            var lightmaps = GameObject.FindObjectsOfType<DynamicLightmap>().ToList();
            Log.D($"Regenerating GUIDs for {lightmaps.Count} {nameof(DynamicLightmap)}");
            for (int i = 0; i < lightmaps.Count; i++) {
                lightmaps[i].Guid = i;
            }
        }
    }

}
