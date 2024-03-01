using System;
using System.Linq;
using UnityEngine;

[Serializable]
public struct FrictionCurve
{
    public float ExtremumSlip;
    public float ExtremumValue;
    public float AsymptoteSlip;
    public float AsymptoteValue;
    public float Stiffness;
    
    public FrictionCurve(WheelFrictionCurve wheelFrictionCurve)
    {
        ExtremumSlip = wheelFrictionCurve.extremumSlip;
        ExtremumValue = wheelFrictionCurve.extremumValue;
        AsymptoteSlip = wheelFrictionCurve.asymptoteSlip;
        AsymptoteValue = wheelFrictionCurve.asymptoteValue;
        Stiffness = wheelFrictionCurve.stiffness;
    }
    
    public FrictionCurve(float extremumSlip, float extremumValue, float asymptoteSlip, float asymptoteValue, float stiffness)
    {
        ExtremumSlip = extremumSlip;
        ExtremumValue = extremumValue;
        AsymptoteSlip = asymptoteSlip;
        AsymptoteValue = asymptoteValue;
        Stiffness = stiffness;
    }
    
    public WheelFrictionCurve ToWheelFrictionCurve()
    {
        return new WheelFrictionCurve
        {
            extremumSlip = ExtremumSlip,
            extremumValue = ExtremumValue,
            asymptoteSlip = AsymptoteSlip,
            asymptoteValue = AsymptoteValue,
            stiffness = Stiffness
        };
    }
}

[Serializable]
public class Wheel
{
    public WheelCollider wheelCollider;
    public Transform wheelTransform;
    public bool isLeftWheel;
    public FrictionCurve forwardFriction = new (0.4f, 1f, 0.8f, 0.5f, 1f);
    public FrictionCurve sidewaysFriction = new (0.4f, 1f, 0.8f, 0.5f, 1f);
    
    public FrictionCurve driftForwardFriction = new (0.4f, 1f, 0.8f, 0.5f, 1f);
    public FrictionCurve driftSidewaysFriction = new (0.4f, 1f, 0.8f, 0.5f, 1f);
}

public class CarController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] Transform centerOfMass;
    [SerializeField] Rigidbody rb;

    [Header("General Settings")] [SerializeField]
    private float maxNormalVelocity = 10;
    [SerializeField] private float maxSteerAngle = 30f;
    [SerializeField] private float motorForce = 50f;
    [SerializeField] private float brakeForce = 50f;

    [Header("Drift")]
    [SerializeField] private float driftForce = 50f;
    [SerializeField] private float driftAngle = 30f;
    [SerializeField] private float driftBrakeForce = 50f;
    [SerializeField] private float friction = 0.5f;
    
    [SerializeField] private Wheel[] frontWheels = new Wheel[2];
    [SerializeField] private Wheel[] rearWheels = new Wheel[2];
    
    Wheel frontLeftWheel;
    Wheel frontRightWheel;
    Wheel rearLeftWheel;
    Wheel rearRightWheel;
    
    Wheel[] wheels = new Wheel[4];
    
    Vector2 moveInput;
    float currentSteerAngle;
    float forwardInput;
    float reverseInput;
    float sumInput;
    
    bool isBraking;
    bool isDrifting;
    
    void Start()
    {
        wheels = frontWheels.Concat(rearWheels).ToArray();
        
        /*InputListener.Instance.OnMoveEvent += OnMove;
        InputListener.Instance.OnDriveEvent += OnDrive;
        InputListener.Instance.OnReverseEvent += OnReverse;
        InputListener.Instance.OnDriftEvent += OnDrift;*/
        
        frontLeftWheel = frontWheels.First(wheel => wheel.isLeftWheel);
        frontRightWheel = frontWheels.First(wheel => !wheel.isLeftWheel);
        rearLeftWheel = rearWheels.First(wheel => wheel.isLeftWheel);
        rearRightWheel = rearWheels.First(wheel => !wheel.isLeftWheel);
        
        // fix the car sliding when turning
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.ConfigureVehicleSubsteps(5f, 12, 15);
        }

        rb.centerOfMass = centerOfMass.localPosition;
    }

    private void OnDrift(bool press,bool hold, bool release)
    {
        
    }

    private void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();

        Vector2 velocity = new Vector2(rb.velocity.x, rb.velocity.z);
        velocity = Vector2.ClampMagnitude(velocity, maxNormalVelocity / 3.6f);
        if (velocity.magnitude > (maxNormalVelocity / 3.6f))
        {
            rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.y);
        }
    }
    
    private void Update()
    {
        UpdateWheels();
    }

    private void UpdateWheels()
    {
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            wheel.wheelTransform.position = pos;
            wheel.wheelTransform.rotation = rot;
        }
    }
    
    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * moveInput.x;
        foreach (var wheel in frontWheels)
        {
            wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, currentSteerAngle, Time.time * 0.01f);
        }
    }

    private void HandleMotor()
    {
        sumInput = forwardInput + reverseInput;
        // Rear wheels
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = sumInput * motorForce;
        }
    }


    #region Input

    private void OnMove(Vector2 value)
    {
        moveInput = value;
    }
    
    private void OnReverse(float value)
    {
        reverseInput = -value;
    }

    private void OnDrive(float value)
    {
        forwardInput = value;
    }

    #endregion
}
