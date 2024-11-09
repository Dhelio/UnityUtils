using Castrimaris.Attributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Castrimaris.Rendering {

    [RequireComponent(typeof(MeshRenderer))]
    [DisallowMultipleComponent]
    public partial class DynamicLightmap : MonoBehaviour {

        [Header("Parameters")]
        [SerializeField] private bool isOptionallyEmissive = false;
        [ConditionalField(nameof(isOptionallyEmissive), true, DisablingTypes.Hidden)]
        [SerializeField] private List<string> keywordsToLookFor = new List<string> { "_EMISSION" };

        [Header("ReadOnly Parameters")]
        [ReadOnly, SerializeField] private int id = -1;


        private MeshRenderer meshRenderer;

        public int Guid { get => id; set => id = value; }
        public bool IsOptionallyEmissive => isOptionallyEmissive;
        public (int, Vector4) Data {
            get {
                return (Renderer.lightmapIndex, Renderer.lightmapScaleOffset);
            }
            set {
                Renderer.lightmapIndex = value.Item1;
                Renderer.lightmapScaleOffset = value.Item2;
            }
        }
        public MeshRenderer Renderer { get {
                if (meshRenderer == null)
                    meshRenderer = GetComponent<MeshRenderer>();
                return meshRenderer;
            }
        }

        public void SetEmissive(bool ShouldBeEnabled) {
            var emissiveMaterials = Renderer.sharedMaterials.Where(material => IsMaterialEmissive(material)).ToArray();

            foreach (var emissiveMaterial in emissiveMaterials) {
                if (ShouldBeEnabled) {
                    //It should not be a problem to try and enable multiple keywords, as long as the material is set properly!
                    //That means that materials that should not be emissive must have emission set to black and no emissive map.
                    foreach (var keyword in keywordsToLookFor) {
                        emissiveMaterial.EnableKeyword(keyword);
                    }
                } else {
                    foreach (var keyword in keywordsToLookFor) {
                        emissiveMaterial.DisableKeyword(keyword);
                    }
                }
            }
        }

        public void RegenerateGuid() {
            id = GetHashCode();
        }

        private bool IsAnyKeywordEnabledFor(Material material) {
            foreach (var key in keywordsToLookFor) {
                if (material.IsKeywordEnabled(key))
                    return true;
            }
            return false;
        }

        private bool IsMaterialEmissive(Material material) {
            if (material == null)
                return false;

            // Check if it has a vlaid emission color
            if (material.HasProperty("_EmissionColor")) {
                Color emissionColor = material.GetColor("_EmissionColor");
                if (emissionColor.maxColorComponent > 0.0f)
                    return true;
            }

            // Check if it has a valid emission map
            if (material.HasProperty("_EmissionMap")) {
                Texture emissionMap = material.GetTexture("_EmissionMap");
                if (emissionMap != null)
                    return true;
            }

            return false;
        }

        private void Reset() {
            meshRenderer = GetComponent<MeshRenderer>();
            id = (id == -1) ? GetHashCode() : id;
            isOptionallyEmissive = false;

            foreach (var material in meshRenderer.sharedMaterials) {
                if (IsMaterialEmissive(material))
                    isOptionallyEmissive = true;
            }
        }

    }

}
