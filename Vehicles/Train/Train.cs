using Castrimaris.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.Vehicles {

    /// <summary>
    /// Non-physics based train system
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public partial class Train : NetworkBehaviour {

        [Header("Parameters")]
        [Tooltip("Starting position of the train on the path, which will be subsequently updated with the actual position on the path. This is Server authoritative, and it will be updated by the Server only.")]
        [SerializeField] private NetworkVariable<float> currentPathPosition = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [Tooltip("Offsets for the wagons, relative to the locomotive.")]
        [SerializeField] private float wagonsOffset = 0;
        [Tooltip("List of stations along the path. The train will stop at each station.")]
        [SerializeField] private List<float> stations = null;
        [Tooltip("Max speed reacheable by the train")]
        [SerializeField] private float maxSpeed = 10.0f;
        [Tooltip("Minimum speed that the train has to mantain")]
        [SerializeField] private float minSpeed = 1.0f;
        [Tooltip("When the train should start decelerating in the current section.\nA section is the path between two stations. This value is normalized.")]
        [Range(0, 1)]
        [SerializeField] private float normalizedDecerelationThreshold = .5f; //TODO change with non-normalized value
        [Tooltip("Acceleration value of the train. Also used to slow down the train when reaching stations")]
        [SerializeField] private float acceleration = 1.0f;
        [Tooltip("How long the train should stop at each station")]
        [SerializeField] private float stopDuration = 3.0f;
        [Tooltip("Name of the trigger in all children animators that must be updated when the train arrives/leaves a station")]
        [SerializeField] private string animatorsTriggerName = "Station";

        [Header("References")]
        [Tooltip("Locomotive reference")]
        [SerializeField] private Locomotive locomotive;
        [Tooltip("Wagons references. If autofill is checked, then the Editor will find the references of the children wagons automatically.")]
        [SerializeField] private List<Wagon> wagons;

        [Header("Events")]
        [Tooltip("Called by the server as an RPC on the clients when the train is arrived at station")]
        [SerializeField] private UnityEvent OnStationArrival = new UnityEvent();
        [Tooltip("Called by the server as an RPC on the clients when the train is leaving a station")]
        [SerializeField] private UnityEvent OnStationDeparture = new UnityEvent();

        private bool canMove = true;
        private ISplineMesh path = null;
        private int nextStationIndex = 1;
        private float currentSpeed = 0.0f;
        private List<Animator> animators;
        private float longestAnimatorDuration = 0.0f;

        #region UNITY OVERRIDES
        private void Awake() {
            //Sanity Checks
            path = GetComponent<ISplineMesh>();
            if (path == null) {
                path = GetComponentInChildren<ISplineMesh>();
                if (path == null) {
                    throw new ApplicationException("Fatal error during application execution.", new NullReferenceException("ISplineMesh cannot be null. There must be an ISplineMesh component attached to Train or to any of its children."));
                }
            }

            locomotive = (locomotive == null) ? locomotive.GetComponentInChildren<Locomotive>() : locomotive;
            if (locomotive == null) {
                throw new ApplicationException("Fatal error during application execution.", new NullReferenceException("Locomotive cannot be null. There must be a child with the Locomotive component attached to Train, or a reference must be provided."));
            }

            int currentStation = (nextStationIndex == 0) ? stations.Count - 1 : nextStationIndex - 1;
            currentPathPosition.Value = stations[currentStation];

            //Re-order stations, because unity uses a reordable list that may scramble stations order (and we don't want to skip stations)
            stations = stations.OrderBy(station => station).ToList();
        }

        private void Update() {
            if (IsServer) {
                if (!canMove)
                    return;
                UpdateAccelerations();
                UpdateCurrentSplinePosition();
                CheckIfIsArrivedAtStation();
            }

            UpdateLocomotiveAndWagonsPositions();
        }

        #endregion

        #region NETWORK OVERRIDES

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();

            if (IsServer) {
                //Get the longest animation in the various animators, so that we can wait the animation duration before leaving a station.
                animators = GetComponentsInChildren<Animator>().ToList();
                var tempAnimators = new List<Animator>(animators); //We need a temp list because collections may not be changed during foreach!
                foreach (var animator in tempAnimators) {
                    //Remove this animator from the list if it doesn't have a specific trigger
                    if (!animator.GetBool(animatorsTriggerName)) {
                        animators.Remove(animator);
                        continue;
                    }
                    var clipLength = animator.runtimeAnimatorController.animationClips.OrderByDescending(clip => clip.length).First().length;
                    if (clipLength > longestAnimatorDuration)
                        longestAnimatorDuration = clipLength;
                }
            }
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Waiting behaviour when reaching a station
        /// </summary>
        private IEnumerator StationArrivalCoroutine() {
            if (IsServer) {
                //Stop the train
                canMove = false;

                //OnStationArrival callbacks
                StationArrivalClientRpc();

                //Start station arrival animations
                foreach (var animator in animators)
                    animator.SetTrigger(animatorsTriggerName);

                //Wait stop time
                yield return new WaitForSeconds(stopDuration);

                //Start station departure animations
                foreach (var animator in animators)
                    animator.SetTrigger(animatorsTriggerName);

                //Wait for the longest closing animation to finish
                yield return new WaitForSeconds(longestAnimatorDuration);

                //OnStationDeparture callbacks
                StationDepartureClientRpc();

                //Start moving the train again
                canMove = true;
            }
        }

        [ClientRpc]
        private void StationArrivalClientRpc() {
            OnStationArrival.Invoke();
        }

        [ClientRpc]
        private void StationDepartureClientRpc() {
            OnStationDeparture.Invoke();
        }

        /// <summary>
        /// Checks if the locomotive has reached a station
        /// </summary>
        private void CheckIfIsArrivedAtStation() {
            if (nextStationIndex <= 0) {
                float totalSectionLength = (path.GetSplineLength() - stations[stations.Count - 1]) + stations[0];
                float currentRelativeSplinePosition = ((path.GetSplineLength() - currentPathPosition.Value) + stations[0]) - totalSectionLength;
                if (currentRelativeSplinePosition > totalSectionLength) {
                    StationArrivalBehaviour();
                }
            } else {
                if (currentPathPosition.Value > stations[nextStationIndex]) {
                    StationArrivalBehaviour();
                }
            }
        }

        private void StationArrivalBehaviour() {
            currentPathPosition.Value = stations[nextStationIndex];
            if (nextStationIndex + 1 >= stations.Count) {
                nextStationIndex = 0;
            } else {
                nextStationIndex += 1;
            }
            currentSpeed = 0;
            StartCoroutine(StationArrivalCoroutine());
        }

        /// <summary>
        /// Purely cosmetical update of the gameobjects.
        /// </summary>
        private void UpdateLocomotiveAndWagonsPositions() {
            (Vector3, Quaternion) worldPosRos = path.GetWorldPositionAndRotation(currentPathPosition.Value);
            locomotive.gameObject.transform.position = worldPosRos.Item1;
            locomotive.gameObject.transform.rotation = worldPosRos.Item2;
            if (wagons.Count > 0) {
                for (int i = 0; i < wagons.Count; i++) {
                    worldPosRos = path.GetWorldPositionAndRotation(currentPathPosition.Value - (wagonsOffset * (i + 1)));
                    wagons[i].gameObject.transform.position = worldPosRos.Item1;
                    wagons[i].gameObject.transform.rotation = worldPosRos.Item2;
                }
            }
        }

        private void UpdateCurrentSplinePosition() {
            currentPathPosition.Value += currentSpeed * Time.deltaTime;
            if (currentPathPosition.Value > path.GetSplineLength()) {
                currentPathPosition.Value -= path.GetSplineLength();
            }
        }

        private void UpdateAccelerations() {
            float normalizedRelativeSplinePosition;
            if (nextStationIndex <= 0) {
                float totalSectionLength = (path.GetSplineLength() - stations[stations.Count - 1]) + stations[0];
                float currentRelativeSplinePosition = ((path.GetSplineLength() - currentPathPosition.Value) + stations[0]) - totalSectionLength;
                normalizedRelativeSplinePosition = currentRelativeSplinePosition / totalSectionLength;
            } else {
                normalizedRelativeSplinePosition = (currentPathPosition.Value - stations[nextStationIndex]) / (stations[nextStationIndex] - stations[nextStationIndex - 1]);
            }

            if (normalizedRelativeSplinePosition < normalizedDecerelationThreshold) {
                //Accelerate til max
                if (currentSpeed < maxSpeed) {
                    currentSpeed += acceleration;
                } else {
                    currentSpeed = maxSpeed;
                }
            } else {
                //Decelerate til min
                if (currentSpeed > minSpeed) {
                    currentSpeed -= acceleration;
                } else {
                    currentSpeed = minSpeed;
                }
            }
        }

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (path != null && stations != null) {
                Gizmos.color = Color.red;
                foreach (var station in stations) {
                    var pos = path.GetWorldPosition(station);
                    Gizmos.DrawSphere(pos, 0.5f);
                }
            }
        }
#endif
    }
}