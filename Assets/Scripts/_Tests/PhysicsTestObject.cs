using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PhysicsTestObject : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Vector3 torque;
    [SerializeField] private Vector3 updateForce;
    [SerializeField] private Vector3 startForce;

    private void Start()
    {
        rb.AddForce(startForce);
    }

    private void FixedUpdate()
    {
        rb.AddTorque(torque);
        if (updateForce != Vector3.zero) rb.AddForce(updateForce);
    }
}
