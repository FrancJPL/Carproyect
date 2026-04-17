using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Motor")]
    public float motorForce      = 2500f;
    public float maxSpeed        = 30f;
    public float brakeForce      = 5000f;
    public float handBrakeForce  = 8000f;
    public float reverseForce    = 800f;

    [Header("Dirección")]
    public float maxSteerAngle   = 28f;
    public float steerSpeed      = 5f;

    [Header("Drift (mejorado)")]
    public bool enableDrift      = true;
    public float driftStiffness  = 0.35f;
    public float normalStiffness = 2.2f;
    public float driftSteerReduction = 0.6f;
    public float driftBoostForce = 500f;
    public float minDriftSpeed   = 8f;

    [Header("Sensación arcade")]
    public float downforce       = 800f;
    public float gripTransition  = 5f;

    [Header("Ruedas - Colliders")]
    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    [Header("Ruedas - Meshes")]
    public Transform frontLeftMesh;
    public Transform frontRightMesh;
    public Transform rearLeftMesh;
    public Transform rearRightMesh;

    [Header("Centro de masa")]
    public Transform centerOfMassTransform;

    private Rigidbody rb;
    private float currentSteer;
    private float currentStiffness;
    private bool isDrifting;
    private float driftTimer;
    private Vector3 lastVelocity;
    private bool controlsEnabled = true;
    private BoostSystem boostSystem;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass             = 800f;
        rb.linearDamping    = 0.15f;
        rb.angularDamping   = 3.5f;

        if (centerOfMassTransform != null)
            rb.centerOfMass = centerOfMassTransform.localPosition;
        else
            rb.centerOfMass = new Vector3(0f, -0.55f, 0.1f);

        currentStiffness = normalStiffness;
        boostSystem = GetComponent<BoostSystem>();
    }

    void Start()
    {
        LapManager lapManager = FindObjectOfType<LapManager>();
        if (lapManager != null)
        {
            lapManager.OnRaceFinished += DisableControls;
        }
    }

    void OnDestroy()
    {
        LapManager lapManager = FindObjectOfType<LapManager>();
        if (lapManager != null)
        {
            lapManager.OnRaceFinished -= DisableControls;
        }
    }

    void FixedUpdate()
    {
        if (!controlsEnabled)
        {
            DisableCarMovement();
            UpdateWheelMeshes();
            return;
        }

        ClampSpeed();
        HandleMotor();
        HandleSteering();
        HandleDrift();
        ApplyDownforce();
        UpdateWheelMeshes();
        lastVelocity = rb.linearVelocity;
    }

    void DisableCarMovement()
    {
        float brake = handBrakeForce;
        
        frontLeftCollider.brakeTorque = brake;
        frontRightCollider.brakeTorque = brake;
        rearLeftCollider.brakeTorque = brake;
        rearRightCollider.brakeTorque = brake;
        
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
        
        frontLeftCollider.steerAngle = 0;
        frontRightCollider.steerAngle = 0;
        
        if (rb.linearVelocity.magnitude > 0.5f)
        {
            rb.linearVelocity = rb.linearVelocity * 0.98f;
            rb.angularVelocity = rb.angularVelocity * 0.98f;
        }
    }

    public void DisableControls()
    {
        controlsEnabled = false;
        Debug.Log("Controles del coche deshabilitados - Carrera terminada");
    }

    public void EnableControls()
    {
        controlsEnabled = true;
        Debug.Log("Controles del coche habilitados");
    }

    public bool AreControlsEnabled() => controlsEnabled;

    void ClampSpeed()
    {
        float speed = rb.linearVelocity.magnitude;
        if (speed > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
    }

    void HandleMotor()
    {
        float rawInput = Input.GetAxis("Vertical");
        bool handBrake = Input.GetKey(KeyCode.Space);

        float currentSpeed = rb.linearVelocity.magnitude;
        bool isMovingForward = Vector3.Dot(transform.forward, rb.linearVelocity) > 0;
        
        float torque = 0f;
        float brake = 0f;
        
        if (handBrake)
        {
            brake = handBrakeForce;
            torque = 0;
        }
        else
        {
            if (rawInput > 0)
            {
                if (!isMovingForward && currentSpeed > 0.5f)
                {
                    brake = brakeForce * 0.8f;
                    torque = 0;
                }
                else
                {
                    torque = rawInput * motorForce;
                    brake = 0;
                }
            }
            else if (rawInput < 0)
            {
                if (isMovingForward && currentSpeed > 1f)
                {
                    brake = brakeForce * Mathf.Clamp01(Mathf.Abs(rawInput));
                    torque = 0;
                }
                else
                {
                    torque = rawInput * reverseForce;
                    brake = 0;
                }
            }
            else
            {
                if (currentSpeed > 0.5f)
                    brake = brakeForce * 0.15f;
            }
        }
        
        rearLeftCollider.motorTorque = torque;
        rearRightCollider.motorTorque = torque;
        
        frontLeftCollider.brakeTorque = brake;
        frontRightCollider.brakeTorque = brake;
        rearLeftCollider.brakeTorque = brake;
        rearRightCollider.brakeTorque = brake;
    }

    void HandleSteering()
    {
        float targetSteer = maxSteerAngle * Input.GetAxis("Horizontal");
        float speedFactor = Mathf.Clamp01(rb.linearVelocity.magnitude / maxSpeed);
        
        float steerMultiplier = (isDrifting && enableDrift) ? driftSteerReduction : 1f;
        targetSteer *= Mathf.Lerp(1f, 0.5f, speedFactor) * steerMultiplier;

        currentSteer = Mathf.Lerp(currentSteer, targetSteer, Time.fixedDeltaTime * steerSpeed);
        frontLeftCollider.steerAngle  = currentSteer;
        frontRightCollider.steerAngle = currentSteer;
    }

    void HandleDrift()
    {
        if (!enableDrift)
        {
            if (currentStiffness != normalStiffness)
            {
                currentStiffness = Mathf.Lerp(currentStiffness, normalStiffness, Time.fixedDeltaTime * gripTransition);
                SetAllWheelsFriction(currentStiffness);
            }
            isDrifting = false;
            return;
        }

        // Drift ahora con X o Control (Shift es para boost)
        bool driftInput = Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.LeftControl);
        bool isTurning = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f;
        float speed = rb.linearVelocity.magnitude;
        
        bool shouldDrift = driftInput && isTurning && speed > minDriftSpeed;
        
        if (shouldDrift && !isDrifting)
        {
            isDrifting = true;
            driftTimer = 0f;
        }
        else if (!shouldDrift && isDrifting)
        {
            EndDrift();
        }
        
        if (isDrifting)
        {
            driftTimer += Time.fixedDeltaTime;
            float targetStiffness = driftStiffness;
            currentStiffness = Mathf.Lerp(currentStiffness, targetStiffness, Time.fixedDeltaTime * gripTransition);
            
            if (shouldDrift)
            {
                Vector3 driftForce = transform.right * Input.GetAxis("Horizontal") * 100f * (speed / maxSpeed);
                rb.AddForce(driftForce);
            }
        }
        else
        {
            currentStiffness = Mathf.Lerp(currentStiffness, normalStiffness, Time.fixedDeltaTime * gripTransition);
        }
        
        SetSidewaysFriction(frontLeftCollider, currentStiffness);
        SetSidewaysFriction(frontRightCollider, currentStiffness);
        SetSidewaysFriction(rearLeftCollider, currentStiffness * 0.8f);
        SetSidewaysFriction(rearRightCollider, currentStiffness * 0.8f);
    }
    
    void EndDrift()
    {
        if (driftTimer > 0.3f)
        {
            Vector3 boostDirection = transform.forward;
            rb.AddForce(boostDirection * driftBoostForce, ForceMode.Impulse);
            Debug.Log("Drift boost! Duration: " + driftTimer.ToString("F2"));
        }
        isDrifting = false;
    }
    
    void SetAllWheelsFriction(float stiffness)
    {
        SetSidewaysFriction(frontLeftCollider, stiffness);
        SetSidewaysFriction(frontRightCollider, stiffness);
        SetSidewaysFriction(rearLeftCollider, stiffness * 0.8f);
        SetSidewaysFriction(rearRightCollider, stiffness * 0.8f);
    }

    void SetSidewaysFriction(WheelCollider col, float stiffness)
    {
        WheelFrictionCurve curve = col.sidewaysFriction;
        curve.stiffness = stiffness;
        col.sidewaysFriction = curve;
    }

    void ApplyDownforce()
    {
        rb.AddForce(-transform.up * downforce * rb.linearVelocity.magnitude);
    }

    void UpdateWheelMeshes()
    {
        UpdateWheelPose(frontLeftCollider, frontLeftMesh);
        UpdateWheelPose(frontRightCollider, frontRightMesh);
        UpdateWheelPose(rearLeftCollider, rearLeftMesh);
        UpdateWheelPose(rearRightCollider, rearRightMesh);
    }

    void UpdateWheelPose(WheelCollider col, Transform mesh)
    {
        if (mesh == null) return;
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        mesh.SetPositionAndRotation(pos, rot);
    }
    
    public bool IsDrifting() => isDrifting;
    public float GetDriftIntensity() => isDrifting ? Mathf.Clamp01(driftTimer / 2f) : 0f;
}