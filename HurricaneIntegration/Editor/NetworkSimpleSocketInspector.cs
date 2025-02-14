#if HVR_OCULUS

using Castrimaris.Core.Editor;
using UnityEditor;

namespace Castrimaris.HurricaneIntegration {

    [CustomEditor(typeof(NetworkSimpleSocket))]
    public class NetworkSimpleSocketInspector : Editor {

        private bool showCopyTools = false;
        private NetworkSimpleSocket localSocket;
        private NetworkSimpleSocket socketToCopy;

        private void OnEnable() {
            localSocket = target as NetworkSimpleSocket;
        }

        public override void OnInspectorGUI() {
            Layout.BoldLabelField("Editor Tools");
            Layout.BoolField("Show Copy Tools", ref showCopyTools);
            if (showCopyTools) {
                Layout.ObjectField<NetworkSimpleSocket>("Reference Socket", ref socketToCopy);
                Layout.Button("Copy parameters", CopyParameters);
            }

            base.OnInspectorGUI();
        }

        private void CopyParameters() => localSocket.SaveSocketTransformValues(socketToCopy.GetSocketableTransformValues());

    }

}

#endif