using HurricaneVR.Framework.Core.Stabbing;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TargetHandler : NetworkBehaviour {

    [Header("Parameters")]
    [Range(1f, 100f), SerializeField] private float forceMagnitude = 1.0f;

    private Rigidbody rb;

    public void Push(StabArgs args) {
        var position = args.Point;
        var direction = args.Normal * -1;
        rb.AddForceAtPosition(direction * forceMagnitude, position);
    }

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

}

