using Castrimaris.Core.Monitoring;
using Castrimaris.Network;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;

namespace Castrimaris.Core.Editor {

    /// <summary>
    /// This class contains miscellaneous tools for quickly modifying tedious stuff in-editor.
    /// The tools are all grouped here because they might be cumbersome or pollute the file count in the project.
    /// </summary>
    public class MiscToolsWindow : EditorWindow {

        [MenuItem("Tools/Castrimaris/Misc Tools Window")]
        public static void OpenWindow() {
            var window = GetWindow<MiscToolsWindow>();
            window.titleContent = new GUIContent(nameof(MiscToolsWindow));
        }

        private void OnGUI() {
            Layout.BoldLabelField("Single click commands");
            Layout.HorizontalGroup(() => {
                //Layout.LabelField("Enable UseUnreliableDeltas on all NetworkTransforms");
                //Layout.Button("Enable All", EnableAllUseUnreliableDeltas);
            });
        }

        //private void EnableAllUseUnreliableDeltas() {
        //    Log.D($"Enabling all Unreliable Deltas in all NetworkTransform components and ClientNetworkTransform components...");
        //
        //    var networkTransforms = FindObjectsOfType<NetworkTransform>();
        //    var clientNetworkTransforms = FindObjectsOfType<ClientNetworkTransform>();
        //
        //    Log.D($"Found {networkTransforms.Length} {nameof(NetworkTransform)} and {clientNetworkTransforms.Length} {nameof(ClientNetworkTransform)}. Enabling deltas for everyone...");
        //
        //    foreach (var networkTransform in networkTransforms) {
        //        networkTransform.UseUnreliableDeltas = true;
        //    }
        //
        //    foreach (var clientNetworkTransform in clientNetworkTransforms) {
        //        clientNetworkTransform.UseUnreliableDeltas = true;
        //    }
        //
        //    Log.D($"Done. Enabled deltas for {networkTransforms.Length + clientNetworkTransforms.Length} elements.");
        //}

    }

}
