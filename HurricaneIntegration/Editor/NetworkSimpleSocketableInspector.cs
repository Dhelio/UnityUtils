using Castrimaris.Core.Editor;
using Castrimaris.Core.Extensions;
using UnityEditor;
using UnityEngine;

namespace Castrimaris.HurricaneIntegration {

    /// <summary>
    /// Inspector for the <see cref="NetworkSimpleSocketable"/> component. Adds a button and reference with wich easily setup the <see cref="NetworkSimpleSocket"/> position.
    /// </summary>
    [CustomEditor(typeof(NetworkSimpleSocketable))]
    public class NetworkSimpleSocketableInspector : Editor {

        #region Private Variables
        
        private NetworkSimpleSocketable socketable;
        private Vector3 originalPosition = Vector3.zero;
        private Quaternion originalRotation = Quaternion.identity;

        //Values from target
        private bool startSocketed;
        private NetworkSimpleSocket socket;
        #endregion

        private void OnEnable() {
            //Retrieve Socketable reference
            socketable = target as NetworkSimpleSocketable;

            //Retrieve serialized values of socketable
            startSocketed = socketable.GetFieldValue<bool>(nameof(startSocketed));
            socket = socketable.GetFieldValue<NetworkSimpleSocket>(nameof(socket));
        }

        public override void OnInspectorGUI() {
            Layout.BoldLabelField("Parameters");
            Layout.BoolField("Start Socketed", ref startSocketed);
            Layout.ObjectField<NetworkSimpleSocket>("Socketable Refernece", ref socket);

            if (socket == null) {
                Layout.Button("Find Closest Socket", FindClosestSocket);
            } else {
                Layout.Button("Move to Socket", MoveSocketableToSocket);
                Layout.Button("Place Using Socket Values", PlaceUsingSocketValues);
                Layout.Button("Save Pose", SavePose);
                Layout.Button("Return Socketable to Origin", ReturnSocketable);
            }

            socketable.SetFieldValue<bool>(nameof(startSocketed), startSocketed);
            socketable.SetFieldValue<NetworkSimpleSocket>(nameof(socket), socket);
        }

        private void ReturnSocketable() {
            Undo.RecordObject(socketable.transform, "Return to Original Position");
            socketable.transform.position = originalPosition;
            socketable.transform.rotation = originalRotation;
            socket = null;
        }

        private void SavePose() {
            var previousParent = socketable.transform.parent;
            socketable.transform.SetParent(socket.transform, true);
            socket.SaveSocketTransformValues(socketable.transform);
            socketable.transform.SetParent(previousParent, true);
        }

        private void PlaceUsingSocketValues() {
            Undo.RecordObject(socketable.transform, "Place on Socket");

            var posRot = socket.GetSocketableTransformValues();
            socketable.transform.position = posRot.Item1;
            socketable.transform.rotation = posRot.Item2;
        }

        private void MoveSocketableToSocket() {
            Undo.RecordObject(socketable.transform, "Move to Socket");
            originalPosition = socketable.transform.position;
            originalRotation = socketable.transform.rotation;


            socketable.transform.position = socket.transform.position;
            socketable.transform.rotation = socket.transform.rotation;
        }

        private void FindClosestSocket() {
            var dist = float.MaxValue;
            var closest = (NetworkSimpleSocket)null;

            foreach (var socket in FindObjectsOfType<NetworkSimpleSocket>()) {

                var d = Vector3.Distance(socketable.transform.position, socket.transform.position);
                if (d < dist) {
                    dist = d;
                    closest = socket;
                }
            }

            socket = closest;
        }
    }
}
