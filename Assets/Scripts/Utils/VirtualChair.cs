using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class VirtualChair : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] bool followRotation = true;

    private void Update()
    {
        transform.position = target.position;
        if (followRotation)
        {
            transform.rotation = target.rotation;
        }
    }
    
}
