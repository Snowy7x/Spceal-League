using System;
using NaughtyAttributes;
using Player;
using UnityEngine;

namespace _Tests
{
    [RequireComponent(typeof(Rigidbody))]
    public class ReCustomController : MonoBehaviour
    {
        [Foldout("Friction"), SerializeField]
        private float forwardFriction = 1f, sidewaysFriction = 1f, angularFriction = 1f;

        [Foldout("Settings"), SerializeField] private float torque = 10f, steer = 10f;

        [Foldout("Jumps"), SerializeField, MinMaxSlider(100, 1000)] private Vector2 jumpForceRange;
        [Foldout("Jumps"), SerializeField] private float jumpForceIncreaseRate = 1f;
        [Foldout("Jumps"), SerializeField] private int maxJumps = 2;

        [Foldout("References"), SerializeField] Rigidbody rb;
        [Foldout("References"), SerializeField] InputListener inputListener;
        [Foldout("References"), SerializeField] Trigger groundTrigger;


        private CarInput carInput = new CarInput();
        private bool pressingJump = false;
        private float jumpForce = 0f;
        private int jumps = 0;
        
        float _appliedSidewaysFriction = 0f;
        
        private bool IsGrounded { 
            get
            {
                if (!groundTrigger) return false;
                return groundTrigger.isTriggered;
            }
        }
        
        private void OnJump(bool press, bool hold, bool release)
        {
            if (jumps >= maxJumps) return;
            if (press) jumpForce = jumpForceRange.x;
            if (hold) pressingJump = true;
            if (release)
            {
                Jump();
                pressingJump = false;
            }
        }

        private void Awake()
        {
            if (!rb) rb = GetComponent<Rigidbody>();
            if (!inputListener) inputListener = GetComponent<InputListener>();

            inputListener.RegisterInputClass(carInput);
            inputListener.OnJumpEvent += OnJump;
        }

        private void Update()
        {
            if (IsGrounded)
                jumps = 0;

            if (pressingJump)
            {
                jumpForce += jumpForceIncreaseRate * Time.deltaTime;
                jumpForce = Mathf.Clamp(jumpForce, jumpForceRange.x, jumpForceRange.y);
            }
        }

        private void FixedUpdate()
        {
            Move();
            Steer();
            ApplyFriction();
        }

        private void ApplyFriction()
        {
            // Apply Friction on velocity
            Vector3 velocity = rb.velocity;
            // get the velocity in the local space
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            // apply friction
            localVelocity.x -= localVelocity.x * _appliedSidewaysFriction * Time.fixedDeltaTime;
            localVelocity.z -= localVelocity.z * forwardFriction * Time.fixedDeltaTime;
            // convert back to world space
            velocity = transform.TransformDirection(localVelocity);
            rb.velocity = velocity;

            // Apply Friction on angular velocity
            Vector3 angularVelocity = rb.angularVelocity;
            // apply friction
            angularVelocity.x -= angularVelocity.x * angularFriction * Time.fixedDeltaTime;
            angularVelocity.y -= angularVelocity.y * angularFriction * Time.fixedDeltaTime;
            angularVelocity.z -= angularVelocity.z * angularFriction * Time.fixedDeltaTime;
            // convert back to world space
            rb.angularVelocity = angularVelocity;
        }

        private void Steer()
        {
            // Simulate car physics
            rb.AddTorque(transform.up * (carInput.SteerInput * steer), ForceMode.Acceleration);
            
            if (Mathf.Abs(carInput.SteerInput) > 0.1f)
            {
                // make the friction higher when steering so it doesn't slide
                _appliedSidewaysFriction = Mathf.Lerp(sidewaysFriction, sidewaysFriction * 10f, Mathf.Abs(carInput.SteerInput));
            }else
            {
                // reset friction
                _appliedSidewaysFriction = sidewaysFriction;
            }
        }

        private void Move()
        {
            // Simulate car physics
            rb.AddForce(transform.forward * (carInput.DriveInput * torque), ForceMode.Acceleration);
        }
        
        private void Jump()
        {
            jumps++;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpForce = 0;
        }
    }
}