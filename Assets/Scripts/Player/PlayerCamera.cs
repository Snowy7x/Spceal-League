using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

[ExecuteInEditMode]
public class PlayerCamera : MonoBehaviour
{
    enum CameraMode
    {
        FreeLook,
        BallCam
    }
    
    [SerializeField] private CameraMode cameraMode;
    [SerializeField] private Transform player;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private float yOff = .5f;
    [SerializeField] private int height = 10;
    [SerializeField] private int distance = 10;
    
    private Transform lookAt;

    private void Start()
    {
        // Find ball with tag
        Transform ball = GameObject.FindGameObjectWithTag("Ball").transform;
        if (ball)
        {
            SetLookAt(ball);
            cameraMode = CameraMode.BallCam;
        }else {
            cameraMode = CameraMode.FreeLook;
        }
    }

    private void LateUpdate()
    {
        if (!player) return;
        
        if (cameraMode == CameraMode.BallCam)
        {
            if (!lookAt)
            {
                cameraMode = CameraMode.FreeLook;
                return;
            }

            var playerPosition = player.position;
            var lookAtPosition = lookAt.position;
            var position = transform.position;
            
            Vector2 flatP = new Vector2(playerPosition.x, playerPosition.z);
            Vector2 flatL = new Vector2(lookAtPosition.x, lookAtPosition.z);
            Vector2 flatDir = (flatP - flatL).normalized;
            Vector2 newPos2D = flatP + flatDir * distance;
            Vector3 newPos = new Vector3(newPos2D.x, playerPosition.y + height, newPos2D.y);
            
            transform.position = Vector3.Lerp(position, newPos, 5f);
            
            // Look between player and ball
            Vector3 midPoint = new Vector3((playerPosition.x + lookAtPosition.x) / 2, (playerPosition.y + lookAtPosition.y) / 2,
                (playerPosition.z + lookAtPosition.z) / 2);
            Vector3 dir = midPoint - position;
            Quaternion lookRot = Quaternion.LookRotation(dir);
            Debug.DrawRay(position, dir);
            
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, smoothSpeed);
            
        }else if (cameraMode == CameraMode.FreeLook)
        {
            transform.position = player.position + new Vector3(0, height, -distance);
        }
    }
    
    public void SetLookAt(Transform target)
    {
        lookAt = target;
    }
}
