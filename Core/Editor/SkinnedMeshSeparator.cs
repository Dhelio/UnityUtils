using Castrimaris.Core.Monitoring;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Castrimaris.Core.Editor {

    /// <summary>
    /// Utility for separating all single <see cref="SkinnedMeshRenderer"/> in a rigged avatar prefab into multiple prefabs.
    /// Useful when making clothing/customization systems that need single skinned meshes.
    /// </summary>
    public class SkinnedMeshSeparator : EditorWindow {

        private GameObject originalPrefab;
        private string outputFolder = "Assets/Prefabs/Variants";
        private string rootBoneName = "root";

        [MenuItem("Tools/Castrimaris/Skinned Mesh Separator")]
        public static void ShowWindow() {
            GetWindow<SkinnedMeshSeparator>("Skinned Mesh Separator");
        }

        void OnGUI() {
            Layout.BoldLabelField("Create Prefab Variants");
            Layout.ObjectField<GameObject>("Original Prefab", ref originalPrefab, allowSceneObjects: false);
            Layout.TextField("Output Folder", ref outputFolder);
            Layout.TextField("Root Bone Name", ref rootBoneName);
            Layout.Button("Create Prefab Variants", CreatePrefabVariants);
        }

        private void CreatePrefabVariants() {
            //Sanity Checks
            if (originalPrefab == null) {
                Log.E("Original Prefab is not assigned.");
                return;
            }
            if (!AssetDatabase.IsValidFolder(outputFolder)) {
                Log.E("Output folder is not valid.");
                return;
            }

            var prefabPath = AssetDatabase.GetAssetPath(originalPrefab);
            var prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(originalPrefab);
            var childs = new List<Transform>();
            for (int i = 0; i < prefabInstance.transform.childCount; i++) {
                if (prefabInstance.transform.GetChild(i).name == rootBoneName)
                    continue;
                childs.Add(prefabInstance.transform.GetChild(i));
            }

            foreach (var child in childs) {
                var variantName = child.name + ".prefab";
                var variantPath = System.IO.Path.Combine(outputFolder, variantName);

                var variantPrefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(originalPrefab);
                var objectToDestroyNames = (from childTransform in childs
                                       where childTransform != child
                                       select childTransform.name).ToArray();

                var objectsToDestroy = from variantChild in variantPrefabInstance.transform.GetComponentsInChildren<Transform>()
                                        where objectToDestroyNames.Contains(variantChild.name)
                                        select variantChild.gameObject;

                foreach (var objectToDestroy in objectsToDestroy) {
                    DestroyImmediate(objectToDestroy);
                }

                // Create the Prefab Variant
                PrefabUtility.SaveAsPrefabAsset(variantPrefabInstance, variantPath);
                Log.D("Created Prefab Variant: " + variantPath);
            }

            //Cleanup
            DestroyImmediate(prefabInstance);
            AssetDatabase.Refresh();
        }
    }
}