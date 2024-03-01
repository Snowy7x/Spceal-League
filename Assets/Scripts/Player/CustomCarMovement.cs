using System;
using System.Collections;
using System.Linq;
using General;
using Unity.Netcode;
using UnityEngine;

enum Axis
{
    X,
    Y,
    Z
}

[RequireComponent(typeof(Rigidbody))]
public class CustomCarMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector3 bounds = new (1, 1, 1);
    [SerializeField] private float maxVelocity = 10f;
    [SerializeField] private float maxAngularVelocity = 10f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float brake = 10f;
    [SerializeField] private float turnSpeed = 10f;
    //[SerializeField] private float inputThreshold = 0.1f;
    [SerializeField] private float friction = 0.1f;
    [SerializeField] private float sideFriction = 10f;
    [SerializeField] private float angularFriction = 10f;
    [SerializeField] private float airAngularFriction = 10f;
    [SerializeField] private float airFrictionMultiplayer = 100f;

    [Header("Drift")] [SerializeField] private float driftBrake = 0.05f;
    [SerializeField] private float driftTurnSpeed = 3000f;
    [SerializeField] private float driftFriction = 0.5f;
    [SerializeField] private float driftSideFriction = 5f;
    [SerializeField] private float driftFrictionMultiplayer = 2f;
    [SerializeField] private float driftMaxTurnAngle = 50f;

    [Header("Jump")] [SerializeField] private float airRollSpeed = 5f;
    [SerializeField] private float flipPower = 500f;
    [SerializeField] private float flipSpeed = 1f;
    [SerializeField] private float minJumpForce = 500f;
    [SerializeField] private float maxJumpForce = 1000f;
    [SerializeField] private float jumpForceMultiplier = 10;
    [SerializeField] private int jumpCount = 1;
    [SerializeField] private Vector3 flickForce = new Vector3(100f, 0, 100f);
    [SerializeField] private float flickRotateSpeed = 100f;
    [SerializeField] private float flickInputThreshold = 0.1f;

    [Header("Wheels")] [SerializeField] private float wheelSpinSpeed = 10f;
    [SerializeField] private float wheelSpeed = 500f;
    [SerializeField] private float minTurnAngle = 20f;
    [SerializeField] private float maxTurnAngle = 40f;
    [SerializeField] private Axis wheelTurnAxis = Axis.Y;
    [SerializeField] private Axis wheelSpinAxis = Axis.Z;

    [Header("References")] 
    [SerializeField] Rigidbody rb;
    [SerializeField] Trigger groundTrigger;
    [SerializeField] Trigger[] sidesTriggers = new Trigger[2];
    [SerializeField] Transform centerOfMass;
    //[SerializeField] private Transform body;
    [SerializeField] Transform[] frontWheels = new Transform[2];
    [SerializeField] Transform[] rearWheels = new Transform[2];
    InputListener inputListener;
    private IPlayer player => GetComponentInParent<IPlayer>();
    private bool IsOwner => player.IsMe;
    
    private Transform[] wheels;
    Vector2 moveInput;

    float forwardInput;
    float reverseInput;
    float sumInput;
    float jumpForce;

    private float driftSideFric = 1f;

    int jumpCounter;
    bool isJumping;
    bool isDrifting;
    private bool isFlipping;
    private readonly bool doMaxVelocity = true;

    #region Input Events

    private void OnDrift(bool press, bool hold, bool release)
    {
        if (isFlipping) return;
        
        if (press || hold)
        {
            if (press)
            {
                driftSideFric = driftSideFriction;
            }else
            {
                driftSideFric = driftSideFriction * driftFrictionMultiplayer;
            }
            
            isDrifting = IsGrounded();
        }
        else if (release)
        {
            isDrifting = false;
        }
    }

    private void OnReverse(float value)
    {
        reverseInput = -value;
    }

    private void OnDrive(float value)
    {
        forwardInput = value;
    }

    private void OnMove(Vector2 obj)
    {
        Vector2 lastMoveInput = moveInput;
        moveInput = obj;
        
        // check if changed direction
        if (lastMoveInput.x == 0 && moveInput.x != 0 ||
            lastMoveInput.x > 0 && moveInput.x < 0 ||
            lastMoveInput.x < 0 && moveInput.x > 0)
        {
            // If grounded reset angular velocity.
            if (IsGrounded())
            {
                // Debug.Log("Reset angular velocity");
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void OnJump(bool press, bool hold, bool release)
    {

        if (isFlipping) return;

        if (press)
        {
            if (IsOnSide() && !IsGrounded())
            {
                Flip();
                return;
            }

            jumpForce = minJumpForce;
            isJumping = true;
        }
        else if (hold)
        {
            isJumping = true;
        }
        else if (release)
        {
            Jump();
        }
    }
    

    #endregion

    #region MonoBehaviour Events

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition;

        inputListener = GetComponent<InputListener>();
        inputListener.OnMoveEvent += OnMove;
        inputListener.OnDriveEvent += OnDrive;
        inputListener.OnReverseEvent += OnReverse;
        inputListener.OnJumpEvent += OnJump;
        inputListener.OnDriftEvent += OnDrift;

        wheels = new Transform[frontWheels.Length + rearWheels.Length];
        Array.Copy(frontWheels, wheels, frontWheels.Length);
        Array.Copy(rearWheels, 0, wheels, frontWheels.Length, rearWheels.Length);
    }
    
    void Update()
    {
        UpdateWheels();
        if (!IsOwner) return;
        if (IsGrounded())
        {
            jumpCounter = 0;
        }

        if (isJumping)
        {
            jumpForce += maxJumpForce * Time.deltaTime * jumpForceMultiplier;
            jumpForce = Mathf.Clamp(jumpForce, minJumpForce, maxJumpForce);
        }

        if (!IsGrounded())
        {
            AirRoll();
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        Move();
        Turn();
        if (isDrifting) Drift();

        // Limit the velocity
        if (rb.velocity.magnitude > maxVelocity && doMaxVelocity)
        {
            var velocity = rb.velocity;
            rb.velocity = Vector3.Lerp(velocity, velocity.normalized * maxVelocity, Time.fixedDeltaTime);
        }

        // Limit the angular velocity
        if (rb.angularVelocity.magnitude > maxAngularVelocity)
        {
            rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, maxAngularVelocity);
        }

        // apply friction
        Vector3 vel = rb.velocity;

        float sFriction = isDrifting ? driftSideFriction : sideFriction;
        // sideways friction
        vel.x -= vel.x * sFriction * Time.fixedDeltaTime;

        if (IsGrounded())
        {
            // forward friction
            float fFriction = isDrifting ? driftFriction : friction;
            vel.z -= vel.z * fFriction * Time.fixedDeltaTime;

        }
        else if (IsOnSide())
        {
            vel.z -= vel.z * airFrictionMultiplayer * Time.fixedDeltaTime;
        }

        rb.velocity = vel;

        // apply angular friction when not grounded
        Vector3 angVel = rb.angularVelocity;
        float angFriction = isDrifting ? driftSideFric : angularFriction;

        if (!IsGrounded())
        {
            angVel.x -= angVel.x * angFriction * Time.fixedDeltaTime;
            angVel.z -= angVel.z * angFriction * Time.fixedDeltaTime;
            angVel.y -= angVel.y * angFriction * Time.fixedDeltaTime;
        }
        else
        {
            angVel.x -= angVel.x * airAngularFriction * Time.fixedDeltaTime;
            angVel.z -= angVel.z * airAngularFriction * Time.fixedDeltaTime;
            angVel.y -= angVel.y * airAngularFriction * Time.fixedDeltaTime;
        }

        rb.angularVelocity = angVel;
    }
    
    #endregion
    
    private void Move()
    {
        if (!IsGrounded() || IsOnSide() || isDrifting) return;
        sumInput = forwardInput + reverseInput;
        rb.AddForce(transform.forward * (sumInput * acceleration * Time.fixedDeltaTime), ForceMode.VelocityChange);
        Debug.DrawRay(transform.position, transform.forward * (sumInput * acceleration * Time.fixedDeltaTime), Color.red);
    }

    private void Turn()
    {
        float tSpeed = isDrifting ? driftTurnSpeed : turnSpeed;
        
        float finalTurnSpeed = tSpeed * (rb.velocity.magnitude / maxVelocity);
        float turn = moveInput.x * (sumInput >= 0 ? 1 : -1);
        turn *= finalTurnSpeed * Time.fixedDeltaTime;
        if (!IsGrounded()) return;
        // only rotate when the car is moving forward or backward
        if (rb.velocity.magnitude > 0.1f)
        {
            // affect by the velocity
            turn /= rb.velocity.magnitude;
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            
            // rotate without affecting the velocity
            transform.rotation = Quaternion.Lerp(transform.rotation, transform.rotation * turnRotation, Time.fixedDeltaTime * finalTurnSpeed);
        }
    }
    
    private void Drift()
    {
        Brake();
    }

    private void Brake()
    {
        // apply brake
        float fBrake = isDrifting ? driftBrake : brake;
        Vector3 brakeForceVector = -rb.velocity.normalized * fBrake;
        rb.AddForce(brakeForceVector, ForceMode.Acceleration);
    }
    
    private void AirRoll()
    {
        // air roll sideways and forward
        var t = transform;
        Vector3 airRoll = t.right * moveInput.y + -t.forward * moveInput.x;
        airRoll *= airRollSpeed * Time.deltaTime;
        // Debug.Log(airRoll);
        if (airRoll != Vector3.zero) rb.AddTorque(airRoll, ForceMode.VelocityChange);
    }
    
    private void Jump()
    {
        if (jumpCounter >= jumpCount) return;
        
        // if there is no input, jump up otherwise flip to the direction of the input
        if (IsGrounded())
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
        else
        {
            if (moveInput.magnitude < flickInputThreshold)
            {
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            }
            else
            {
                // flick
                var t = transform;
                var forward = t.forward;
                var right = t.right;
                Vector3 flickF = (forward * moveInput.y + right * moveInput.x);
                flickF.x *= flickForce.x;
                flickF.y *= flickForce.y;
                flickF.z *= flickForce.z;
                rb.AddForce(flickF, ForceMode.Impulse);
                
                // rotate
                Vector3 flickTorque = (right * moveInput.y + -forward * moveInput.x) * flickRotateSpeed;
                rb.AddTorque(flickTorque, ForceMode.Impulse);
            }
        }

        jumpCounter++;
    }
    
    private void Flip()
    {
        // Calculate the desired rotation to flip the car upright
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isFlipping = true;
        StartCoroutine(FlipC());
    }

    IEnumerator FlipC()
    {
        // Add force to flip the car
        rb.AddForce(Vector3.up * flipPower, ForceMode.Impulse);
        
        // Check which side is up
        Vector3 up = transform.up;

        // Calculate the rotation needed to flip the car
        Quaternion targetRotation = Quaternion.FromToRotation(up, Vector3.up) * transform.rotation;
        // Debug.Log($"up: {up}, targetRotation: {targetRotation}");

        // Rotate the car
        float t = 0;
        while (t < 1 && transform.rotation != targetRotation)
        {
            t += Time.deltaTime * flipSpeed;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, t);
            
            // if the rotation is close enough to the target rotation, stop rotating
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
                break;
            
            
            yield return null;
        }

        isFlipping = false;
    }

    private void UpdateWheels()
    {
        /*// spin wheels
        foreach (Transform wheel in wheels)
        {
            float speed = rb.velocity.magnitude;
            float spinRot = speed * wheelSpinSpeed * speed;

            Vector3 currentRot = wheel.localRotation.eulerAngles;
            Vector3 newRot = Vector3.zero;
            if (frontWheels.Contains(wheel))
            {
                // turn the front wheels while spinning
                float turnRot = moveInput.x;
                // calculate the turn angle based on the speed
                float wheelTurnAngle = isDrifting ? driftMaxTurnAngle : Mathf.Lerp(maxTurnAngle, minTurnAngle, speed / maxVelocity);

                switch (wheelTurnAxis)
                {
                    case Axis.X:
                        newRot.x = turnRot * wheelTurnAngle; 
                        break;
                    case Axis.Y:
                        newRot.y = turnRot * wheelTurnAngle;
                        break;
                    case Axis.Z:
                        newRot.z = turnRot * wheelTurnAngle;
                        break;
                }
                
                // Debug.Log($"turnRot: {turnRot}, wheelTurnAngle: {wheelTurnAngle}, newRot: {newRot}");
                
                // Debug.Log($"currentRot: {currentRot}, newRot: {newRot}, velocity: {speed}");
            }

            // spin the wheels
            switch (wheelSpinAxis)
            {
                case Axis.X:
                    newRot.x = currentRot.x + spinRot;
                    break;
                case Axis.Y:
                    newRot.y = currentRot.y + spinRot;
                    break;
                case Axis.Z:
                    newRot.z = currentRot.z + spinRot;
                    break;
            }
            
            wheel.localRotation = Quaternion.Lerp(wheel.localRotation, Quaternion.Euler(newRot), Time.time * wheelSpeed);
        }*/
    }

    private bool IsGrounded()
    {
        return groundTrigger.isTriggered;
    }

    private bool IsOnSide()
    {
        return sidesTriggers.Any(t => t.isTriggered);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.forward, bounds);
    }
}
