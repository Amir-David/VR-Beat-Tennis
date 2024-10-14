using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncing : MonoBehaviour
{
    public Rigidbody rb;
    public float speedMagnitude = 1;
    public Vector3 speedToApplyAfterOwnership;

    private void Start()
    {
        Vector3 directionToCenter = new Vector3(-transform.position.x, 0, -transform.position.z).normalized;
        rb.velocity = directionToCenter * speedMagnitude;
        //Vector3 directionToCenter = -transform.position.normalized;
       //rb.velocity = directionToCenter * speedMagnitude;
        //Debug.Log(rb.velocity);
        // rb.velocity = Random.onUnitSphere * speedMagnitude;
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
