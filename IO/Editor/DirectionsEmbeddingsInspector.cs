#if OPENAI

using Castrimaris.Core.Editor;
using UnityEditor;

namespace Castrimaris.IO.OpenAI.Embeddings {

    [CustomEditor(typeof(DirectionsEmbeddingsContainer))]
    public class DirectionsEmbeddingsInspector : Editor {

        private DirectionsEmbeddingsContainer directionsEmbeddings;

        private void OnEnable() {
            directionsEmbeddings = target as DirectionsEmbeddingsContainer;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            Layout.Space(2);
            Layout.BoldLabelField("Tools");
            Layout.Button("Generate Embeddings", GenerateEmbeddings);
        }

        private void GenerateEmbeddings() {
            directionsEmbeddings.GenerateEmbeddings();
            EditorUtility.SetDirty(directionsEmbeddings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

    }

}

#endif