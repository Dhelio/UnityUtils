using UnityEngine;
using System.Linq;
using Unity.Netcode;
using Castrimaris.Math;

namespace Castrimaris.Vehicles {
    public partial class Train : NetworkBehaviour {

#if UNITY_EDITOR

        [Header("Editor Parameters")]
        [SerializeField] private bool autoFillReferences = true;
        [Range(.1f, 10f)]
        [SerializeField] private float gizmoSize = 1.0f;
        [SerializeField] private Color gizmoColor = Color.cyan;

        private void OnDrawGizmosSelected() {

            //Sanity check for path
            if (path == null) {
                path = GetComponent<ISplineMesh>();
                if (path == null) {
                    path = GetComponentInChildren<ISplineMesh>();
                    if (path == null) {
                        Debug.LogError($"Cannot find a component of type {nameof(ISplineMesh)} attached to this Train or its children. Please, add one.");
                        return;
                    }
                }
            }

            if (stations.Count > 0) {

                //Place stations gizmos
                for (int i = 0; i < stations.Count; i++) {
                    Gizmos.color = gizmoColor;
                    Gizmos.DrawSphere(path.GetWorldPosition(stations[i]), gizmoSize);
                }

                //Sanity check for testing
                if (nextStationIndex > stations.Count - 1 || nextStationIndex < 0)
                    nextStationIndex = 0;

                //Place locomotive
                if (autoFillReferences) {
                    if (locomotive == null) {
                        locomotive.GetComponentInChildren<Locomotive>();
                    }
                }
                if (locomotive != null) {
                    int targetStation = (nextStationIndex == 0) ? stations.Count - 1 : nextStationIndex - 1;
                    (Vector3, Quaternion) worldPosRos = path.GetWorldPositionAndRotation(stations[targetStation]);
                    locomotive.transform.position = worldPosRos.Item1;
                    locomotive.transform.rotation = worldPosRos.Item2;
                }

                //Place wagons
                if (wagons.Count > 0) {
                    if (autoFillReferences) {
                        wagons = this.gameObject.GetComponentsInChildren<Wagon>().ToList();
                    }
                    (Vector3, Quaternion) worldPosRos;
                    int targetStation = (nextStationIndex == 0) ? stations.Count - 1 : nextStationIndex - 1;
                    for (int i = 0; i < wagons.Count; i++) {
                        worldPosRos = path.GetWorldPositionAndRotation(stations[targetStation] - (wagonsOffset * (i + 1)));
                        wagons[i].gameObject.transform.position = worldPosRos.Item1;
                        wagons[i].gameObject.transform.rotation = worldPosRos.Item2;
                    }
                }
            }
        }

#endif

    }
}