using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Motor")]
    public float motorForce      = 2500f;
    public float maxSpeed        = 30f;
    public float brakeForce      = 5000f;
    public float reverseForce    = 800f;

    [Header("Dirección")]
    public float maxSteerAngle   = 28f;
    public float steerSpeed      = 5f;

    [Header("Drift (mejorado)")]
    public bool enableDrift      = true;          // Activar/desactivar drift
    public float driftStiffness  = 0.35f;         // Derrape (menos = más deslizamiento)
    public float normalStiffness = 2.2f;          // Agarre normal
    public float driftSteerReduction = 0.6f;      // Reducción de giro al driftear
    public float driftBoostForce = 500f;          // Empuje extra al salir del drift
    public float minDriftSpeed   = 8f;             // Velocidad mínima para driftear

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

    // Privados
    private Rigidbody rb;
    private float currentSteer;
    private float currentStiffness;
    private bool isDrifting;
    private float driftTimer;
    private Vector3 lastVelocity;

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
    }

    void FixedUpdate()
    {
        ClampSpeed();
        HandleMotor();
        HandleSteering();
        HandleDrift();
        ApplyDownforce();
        UpdateWheelMeshes();
        lastVelocity = rb.linearVelocity;
    }

    void ClampSpeed()
    {
        float speed = rb.linearVelocity.magnitude;
        if (speed > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
    }

    void HandleMotor()
    {
        float rawInput = Input.GetAxis("Vertical");
        float currentSpeed = rb.linearVelocity.magnitude;
        bool isMovingForward = Vector3.Dot(transform.forward, rb.linearVelocity) > 0;
        
        float torque = 0f;
        float brake = 0f;
        
        // Caso 1: Quiere ir adelante (W)
        if (rawInput > 0)
        {
            if (!isMovingForward && currentSpeed > 0.5f)
            {
                // Va marcha atrás pero quiero adelante → frenar primero
                brake = brakeForce * 0.8f;
                torque = 0;
            }
            else
            {
                // Normal: acelerar adelante
                torque = rawInput * motorForce;
                brake = 0;
            }
        }
        // Caso 2: Quiere frenar o ir atrás (S)
        else if (rawInput < 0)
        {
            if (isMovingForward && currentSpeed > 1f)
            {
                // Va adelante y freno → frenar, NO ir atrás todavía
                brake = brakeForce * Mathf.Clamp01(Mathf.Abs(rawInput));
                torque = 0;
            }
            else
            {
                // Ya está parado o yendo atrás → ir marcha atrás
                torque = rawInput * reverseForce;
                brake = 0;
            }
        }
        // Caso 3: Sin input (sin W ni S)
        else
        {
            // Frenado suave al soltar acelerador
            if (currentSpeed > 0.5f)
                brake = brakeForce * 0.15f;
        }
        
        // Aplicar torque (solo a ruedas traseras para tracción trasera)
        rearLeftCollider.motorTorque = torque;
        rearRightCollider.motorTorque = torque;
        
        // Aplicar freno (a todas las ruedas)
        frontLeftCollider.brakeTorque = brake;
        frontRightCollider.brakeTorque = brake;
        rearLeftCollider.brakeTorque = brake;
        rearRightCollider.brakeTorque = brake;
    }

    void HandleSteering()
    {
        float targetSteer = maxSteerAngle * Input.GetAxis("Horizontal");
        float speedFactor = Mathf.Clamp01(rb.linearVelocity.magnitude / maxSpeed);
        
        // Reducción de giro en drift
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
            // Modo sin drift - agarre normal siempre
            if (currentStiffness != normalStiffness)
            {
                currentStiffness = Mathf.Lerp(currentStiffness, normalStiffness, Time.fixedDeltaTime * gripTransition);
                SetAllWheelsFriction(currentStiffness);
            }
            isDrifting = false;
            return;
        }

        bool driftInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.X);
        bool isTurning = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f;
        float speed = rb.linearVelocity.magnitude;
        
        // Detectar drift
        bool shouldDrift = driftInput && isTurning && speed > minDriftSpeed;
        
        // Activar/desactivar drift
        if (shouldDrift && !isDrifting)
        {
            // Iniciar drift
            isDrifting = true;
            driftTimer = 0f;
        }
        else if (!shouldDrift && isDrifting)
        {
            // Terminar drift - dar boost
            EndDrift();
        }
        
        if (isDrifting)
        {
            driftTimer += Time.fixedDeltaTime;
            float targetStiffness = driftStiffness;
            currentStiffness = Mathf.Lerp(currentStiffness, targetStiffness, Time.fixedDeltaTime * gripTransition);
            
            // Empuje lateral durante el drift (más control)
            if (shouldDrift)
            {
                Vector3 driftForce = transform.right * Input.GetAxis("Horizontal") * 100f * (speed / maxSpeed);
                rb.AddForce(driftForce);
            }
        }
        else
        {
            // Volver a agarre normal
            currentStiffness = Mathf.Lerp(currentStiffness, normalStiffness, Time.fixedDeltaTime * gripTransition);
        }
        
        // Aplicar fricción
        SetSidewaysFriction(frontLeftCollider, currentStiffness);
        SetSidewaysFriction(frontRightCollider, currentStiffness);
        SetSidewaysFriction(rearLeftCollider, currentStiffness * 0.8f);
        SetSidewaysFriction(rearRightCollider, currentStiffness * 0.8f);
    }
    
    void EndDrift()
    {
        if (driftTimer > 0.3f) // Solo dar boost si drift duró suficiente
        {
            // Mini turbo al salir del drift
            Vector3 boostDirection = transform.forward;
            rb.AddForce(boostDirection * driftBoostForce, ForceMode.Impulse);
            
            // Efecto visual opcional (puedes agregar partículas aquí)
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
    
    // Método público para consultar si está drifteando
    public bool IsDrifting() => isDrifting;
    
    // Método público para obtener fuerza de drift (para efectos visuales)
    public float GetDriftIntensity() => isDrifting ? Mathf.Clamp01(driftTimer / 2f) : 0f;
}