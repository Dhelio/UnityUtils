using DG.Tweening;
using Castrimaris.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.Vehicles 
{
    /// <summary>
    /// Server-side managed lift
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkLift : NetworkVehicle {

        [Header("Parameters")]
        [Range(1.0f, 10.0f),SerializeField] private float speed = 1.0f;
        [Range(1, 10),SerializeField] private int minimumWaitTime = 5;

        [Header("References")]
        [SerializeField] private NetworkLiftFloor[] floors;

        [Header("Events")]
        public UnityEvent OnMoving = new UnityEvent();
        public UnityEvent OnStopping = new UnityEvent();

        [Header("ReadOnly Parameters")]
        [ReadOnly, SerializeField] private int currentFloor = -1;
        [ReadOnly, SerializeField] private int targetFloor = -1;

        private Queue<NetworkLiftFloor> floorsQueue = new Queue<NetworkLiftFloor>();
        private Coroutine movement;

        public void RequestFloor(int FloorIndex) {
            RequestFloorServerRpc(FloorIndex);
        }

        public void RequestFloor(NetworkLiftFloor Floor) {
            RequestFloorServerRpc(Floor.Index);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestFloorServerRpc(int FloorIndex) {
            if (floorsQueue.Contains(floors[FloorIndex]))
                return;

            floorsQueue.Enqueue(floors[FloorIndex]);

            //TODO floorseQueue.OrderBy

            if (movement == null)
                movement = StartCoroutine(Movement());
        }

        private IEnumerator Movement() {
            var minimumWaitTime = new WaitForSeconds(this.minimumWaitTime);
            var checkTime = new WaitForSeconds(.1f);
            while (floorsQueue.Count > 0) {
                var floor = floorsQueue.Dequeue();
                var movingTween = this.transform
                    .DOMove(floor.Position, speed)
                    .OnStart(OnMoving.Invoke)
                    .OnComplete(OnStopping.Invoke)
                    .SetSpeedBased(true)
                    .Play();
                yield return movingTween;
                floor.OnLiftArrival.Invoke();
                yield return this.minimumWaitTime;
                floor.OnLiftDeparture.Invoke();
                while (!floor.CanLiftLeave) //HACK
                    yield return checkTime;
            }
            movement = null;
        }
    }
}