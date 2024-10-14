using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Unity.Netcode;

// public class BouncingBall : NetworkBehaviour
public class BouncingBall : MonoBehaviour

{
    public Rigidbody rb;
    public float speedMagnitude = 1;
    public Vector3 speedToApplyAfterOwnership;

    private void Start()
    {
        rb.velocity = Random.onUnitSphere * speedMagnitude;
        // if (IsOwner)
            // rb.velocity = Random.onUnitSphere * speedMagnitude;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (rb.velocity.magnitude != speedMagnitude)
        {
            rb.velocity = rb.velocity.normalized * speedMagnitude;
        }
    }
}
