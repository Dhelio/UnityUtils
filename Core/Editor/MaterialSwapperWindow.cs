using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Castrimaris.Core.Editor {

    /// <summary>
    /// Utility window to swap same materials in an object's children with another one.
    /// </summary>
    public class MaterialSwapperWindow : EditorWindow {

        private GameObject target;
        private Material desiredMaterial;
        private Material targetMaterial;

        /// <summary>
        /// Opens the Material Swapper Window
        /// </summary>
        [MenuItem("Tools/Castrimaris/Material Swapper Window")]
        public static void OpenWindow() {
            var window = GetWindow<MaterialSwapperWindow>();
            window.titleContent = new GUIContent(nameof(MaterialSwapperWindow));
        }

        private void OnGUI() {
            Layout.BoldLabelField("Parameters");
            Layout.ObjectField<GameObject>("Target GameObject", ref target);
            Layout.ObjectField<Material>("Target Material", ref targetMaterial);
            Layout.ObjectField<Material>("Desired Material", ref desiredMaterial);
            if (Layout.Button("Change")) {
                Change();   
            }
        }

        private void Change() {
            int counter = 0;
            Log.D($"Changing mats");
            var meshRenderers = target.GetComponentsInChildren<MeshRenderer>();
            Undo.RecordObjects(meshRenderers, "prevMeshRenderersState");
            for (int i = 0; i < meshRenderers.Length; i++) {
                if (meshRenderers[i].sharedMaterial == targetMaterial) {
                    counter++;
                    meshRenderers[i].sharedMaterial = desiredMaterial;
                }
            }
            Log.D($"Changed {counter} materials in meshes");
        }
    }

}
