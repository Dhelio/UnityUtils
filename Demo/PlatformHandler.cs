using DG.Tweening;
using Castrimaris.Core.Monitoring;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class PlatformHandler : NetworkBehaviour {

    [Header("Parameters")]
    [SerializeField] private Vector3 targetPosition = Vector3.zero;
    [SerializeField] private float duration = 20.0f;

    private float maxDistance;
    private Vector3 originalPosition;
    private const string id = "platformHandler";
    private bool? isMovingUp = null;

    public void RequestMovement(bool IsUp) {
        RequestMovementServerRpc(IsUp);
    }

    public void RequestOwnership() {
        RequestOwnershipServerRpc();
    }

    private void Awake() {
        originalPosition = this.transform.localPosition;
        maxDistance = Vector3.Distance(originalPosition, targetPosition);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestOwnershipServerRpc(ServerRpcParams serverParams = default) {
        NetworkObject.ChangeOwnership(serverParams.Receive.SenderClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestMovementServerRpc(bool isUp) {
        if (isUp)
            MoveUp();
        else 
            MoveDown();
    }

    private void MoveUp() {
        if (isMovingUp != null && isMovingUp.Value) {
            Log.D("Platform is already moving up");
            return;
        }

        isMovingUp = true;
        DOTween.Kill(id);
        var distance = Vector3.Distance(this.transform.localPosition, targetPosition);
        var normalizedDistance = distance / maxDistance;
        var time = duration * normalizedDistance;
        Log.D($"Moving up, current position {this.transform.localPosition}, target position {originalPosition}, maxDistance {maxDistance}, currentDistance {distance}, normalized {normalizedDistance}, time {time}");
        this.transform.DOLocalMove(targetPosition, time).SetId(id);
    }

    private void MoveDown() {
        if (isMovingUp != null && !isMovingUp.Value) {
            Log.D("Platform is already moving down");
            return;
        }

        isMovingUp = false;
        DOTween.Kill(id);
        var distance = Vector3.Distance(this.transform.localPosition, originalPosition);
        var normalizedDistance = distance / maxDistance;
        var time = duration * normalizedDistance;
        Log.D($"Moving down, current position {this.transform.localPosition}, target position {originalPosition}, maxDistance {maxDistance}, currentDistance {distance}, normalized {normalizedDistance}, time {time}");
        this.transform.DOLocalMove(originalPosition, time).SetId(id);
    }

}

