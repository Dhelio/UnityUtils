#if UNITY_EDITOR

using Castrimaris.Attributes;
using Castrimaris.Core;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Castrimaris.Utilities {

    public class ModelToSprite : MonoBehaviour {

        public enum EncodingType { 
            [StringValue(".png")] PNG,
            [StringValue(".jpg")] JPG,
            [StringValue(".exr")] EXR,
            [StringValue(".tga")] TGA 
        }

        private const string fileNamePrepend = "ModelToSprite_";

        [Header("Parameters")]
        [SerializeField] private Vector2Int size = new Vector2Int(256, 256);
        [SerializeField] private LayerMask targetLayer;
        [SerializeField, Range(-5, 5)] private float cameraPadding = 0.0f;

        [Header("File Writing Parameters")]
        [SerializeField] private bool useObjectName = false;
        [SerializeField] private bool appendFilesCount = false;
        [SerializeField] private EncodingType encodingType = EncodingType.PNG;

        [Header("Readonly Parameters")]
        [SerializeField, ReadOnly] private bool isInitialized = false;
        [SerializeField, ReadOnly] private int filesCount = -1;

        [Header("References")]
        [SerializeField] private Camera camera;

        private void Start() {
            Destroy(this);
        }

        private void Reset() {
            isInitialized = false;
            camera = null;
            filesCount = -1;
            cameraPadding = 0.0f;
            size = new Vector2Int(256,256);
        }

        [ContextMenu("Take Screenshot of All Models")]
        public void TakeScreenshotOfAllModels() {
            Log.I("Running screenshots...");

            if (!isInitialized)
                Initialize();

            var meshRenderers = this.gameObject.GetComponentsInChildren<MeshRenderer>();
            var skinnedMeshRenderers = this.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();

            Log.I($"Found {meshRenderers.Count()} meshRenderers and {skinnedMeshRenderers.Count()} skinned mesh renderers to screenshot.");

            //Get original states of the models
            var meshRenderersActiveStates = new List<bool>();
            var skinnedMeshRenderersActiveStates = new List<bool>();

            foreach (var meshRenderer in meshRenderers) {
                meshRenderersActiveStates.Add(meshRenderer.gameObject.activeSelf);
                meshRenderer.gameObject.SetActive(false);
            }

            foreach (var skinnedMeshRenderer in skinnedMeshRenderers) {
                skinnedMeshRenderersActiveStates.Add(skinnedMeshRenderer.gameObject.activeSelf);
                skinnedMeshRenderer.gameObject.SetActive(false);
            }

            //Screens
            foreach (var meshRenderer in meshRenderers) {
                meshRenderer.gameObject.SetActive(true);
                Log.I($"Screenshotting mesh {meshRenderer.name}");
                var originalLayer = meshRenderer.gameObject.layer;
                meshRenderer.gameObject.layer = targetLayer;
                var screen = TakeScreenshot(meshRenderer.bounds);
                SaveToDisk(screen, meshRenderer.name);
                meshRenderer.gameObject.layer = originalLayer;
                meshRenderer.gameObject.SetActive(false);
            }

            foreach (var skinnedMeshRenderer in skinnedMeshRenderers) {
                skinnedMeshRenderer.gameObject.SetActive(true);
                Log.I($"Screenshotting skinned mesh {skinnedMeshRenderer.name}");
                var originalLayer = skinnedMeshRenderer.gameObject.layer;
                skinnedMeshRenderer.gameObject.layer = targetLayer;
                var screen = TakeScreenshot(skinnedMeshRenderer.bounds);
                SaveToDisk(screen, skinnedMeshRenderer.name);
                skinnedMeshRenderer.gameObject.layer = originalLayer;
                skinnedMeshRenderer.gameObject.SetActive(false);
            }

            //Reset original state of the models
            for (int i = 0; i < meshRenderers.Length; i++) {
                meshRenderers[i].gameObject.SetActive(meshRenderersActiveStates[i]);
            }

            for (int i = 0; i < skinnedMeshRenderers.Length; i++) {
                skinnedMeshRenderers[i].gameObject.SetActive(skinnedMeshRenderersActiveStates[i]);
            }

            AssetDatabase.Refresh();
        }

        private byte[] TakeScreenshot(Bounds modelBounds) {

            camera.WrapToBounds(modelBounds, cameraPadding);

            var originalActiveRenderTexture = RenderTexture.active;
            var renderTexture = new RenderTexture(size.x, size.y, 32);

            camera.targetTexture = renderTexture;

            RenderTexture.active = camera.targetTexture;

            camera.Render();

            var frame = new Texture2D(camera.targetTexture.width, camera.targetTexture.height, TextureFormat.ARGB32, false);
            frame.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
            frame.Apply();
            RenderTexture.active = originalActiveRenderTexture;

            byte[] tex;

            switch (encodingType) {
                case EncodingType.JPG:
                    tex = frame.EncodeToJPG();
                    break;
                case EncodingType.TGA:
                    tex = frame.EncodeToTGA();
                    break;
                case EncodingType.EXR:
                    tex = frame.EncodeToEXR();
                    break;
                case EncodingType.PNG:
                default:
                    tex = frame.EncodeToPNG();
                    break;
            }

            return tex;
        }

        private void SaveToDisk(byte[] ScreenShot, string FileName) {
            var path = FindProjectModelToSpriteFolderPath();
            var fileName = $"{ ((useObjectName) ? FileName : fileNamePrepend) }";
            var appendName = (appendFilesCount) ? $"_{filesCount}" : "";
            var extensionName = encodingType.GetStringValue();
            var filePath = $"{path}/{fileName}{appendName}{extensionName}";
            File.WriteAllBytes(filePath, ScreenShot);

            filesCount++;
        }

        [ContextMenu("Initialize")]
        private void Initialize() {
            camera = FindObjectOfType<Camera>();
            var path = FindProjectModelToSpriteFolderPath();
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
                filesCount = 0;
            } else {
                filesCount = 0;
                var files = Directory.GetFiles(path, "*.png", SearchOption.TopDirectoryOnly);
                foreach (var file in files) {
                    int value = int.Parse(file.Substring(file.Length-4).Split('_').Last());
                    if (value > filesCount)
                        filesCount = value;
                }
            }

            isInitialized = true;
        }

        private string FindProjectModelToSpriteFolderPath() {
            var folder = FindProjectTextureFolderPath();
            return folder += $"/{nameof(ModelToSprite)}";
        }

        private string FindProjectTextureFolderPath() {
            var currentFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            do {
                currentFolderPath = Directory.GetParent(currentFolderPath).FullName;
            } while (!currentFolderPath.EndsWith("Assets") && !currentFolderPath.EndsWith("Library"));
            currentFolderPath = Directory.GetParent(currentFolderPath).FullName;
            currentFolderPath += $"/Assets/Textures";
            return currentFolderPath;
        }

    }

}

#endif