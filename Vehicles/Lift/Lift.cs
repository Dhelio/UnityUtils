#if DOTWEEN

using DG.Tweening;
using Castrimaris.Attributes;
using Castrimaris.Core.Monitoring;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.Vehicles {
    public class Lift : Vehicle {

        [Header("Parameters")]
        [Range(0.1f, 10.0f), SerializeField] private float speed = 1.0f;
        [Range(1, 10), SerializeField] private int minimumWaitTime = 3;
        [SerializeField] private int startingFloor = 0;
        [SerializeField] private Ease movementEase = Ease.Linear;
        [SerializeField] private UpdateType movementUpdateType = UpdateType.Normal;
        
        [Header("References")]
        [SerializeField] private LiftFloor[] floors;

        [Header("Events")]
        public UnityEvent OnMoving = new UnityEvent();
        public UnityEvent OnStopping = new UnityEvent();

        [Header("ReadOnly Parameters")]
        [ReadOnly, SerializeField] private int currentFloor = -1;
        [ReadOnly, SerializeField] private int targetFloor = -1;
        [ReadOnly, SerializeField] private bool isMoving = false;

        private Queue<LiftFloor> floorsQueue = new Queue<LiftFloor>();

        [ExposeInInspector]
        public void RequestFloor(int FloorIndex) {
            if (floorsQueue.Contains(floors[FloorIndex])) {
                Log.D($"Floor {FloorIndex} already in queue. Skipping.");
                return;
            }

            floorsQueue.Enqueue(floors[FloorIndex]);

            if (isMoving)
                return;

            isMoving = true;
            StartCoroutine(Movement());
        }

        private void Start() {
            RequestFloor(startingFloor);
        }

        /// <summary>
        /// Movement coroutine for the lift
        /// </summary>
        private IEnumerator Movement() {
            //The minimum time the elevator should wait between each trip
            var minimumWaitTime = new WaitForSeconds(this.minimumWaitTime);
            //Whilst we have floors, keep popping the queue
            while (floorsQueue.Count > 0) {
                var floor = floorsQueue.Dequeue();
                targetFloor = floor.Index;
                //We're using dotween to move the elevator on update. We move it to the desired position with a speed value instead of time, and with an ease
                var movingTween = this.transform
                    .DOMove(floor.Position, speed)
                    .SetSpeedBased(true)
                    .SetEase(movementEase)
                    .SetUpdate(movementUpdateType)
                    .Play();
                OnMoving.Invoke(); //Starting events
                yield return movingTween.WaitForCompletion(); //Wait for the dotween animation to complete.
                OnStopping.Invoke(); //Stopping events
                floor.OnLiftArrival.Invoke(); //floor arrival events
                currentFloor = floor.Index; //update floor
                yield return minimumWaitTime; //Wait at least the minimum time before leaving again
                floor.OnLiftDeparture.Invoke();
                //TODO wait floor animations
            }
            isMoving = false;
        }
    }
}

#endif