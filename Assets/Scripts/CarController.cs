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
    public float steerSpeed      = 5f;       // qué rápido gira el volante

    [Header("Drift")]
    public float driftStiffness  = 0.45f;    // menos = más derrape
    public float normalStiffness = 2.2f;     // agarre normal
    public float driftBoost      = 1.4f;     // multiplicador de velocidad al salir del drift

    [Header("Sensación arcade")]
    public float downforce       = 800f;     // fuerza que pega al suelo
    public float gripTransition  = 6f;       // suavidad al recuperar agarre

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

    // ── privados ──
    private Rigidbody rb;
    private float     currentSteer;
    private float     currentStiffness;
    private bool      isDrifting;
    private bool      boostActive;
    private float     boostTimer;
    private float     speedInput;

    // ───────────────────────────────────────────
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

    // ───────────────────────────────────────────
    void FixedUpdate()
    {
        ClampSpeed();
        HandleMotor();
        HandleSteering();
        HandleDrift();
        ApplyDownforce();
        UpdateWheelMeshes();
    }

    // ── Limitar velocidad máxima ────────────────
    void ClampSpeed()
    {
        float speed = rb.linearVelocity.magnitude;
        if (speed > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
    }

    // ── Motor y frenos ──────────────────────────
    void HandleMotor()
    {
        speedInput = Input.GetAxis("Vertical");

        float torque = speedInput > 0
            ? speedInput * motorForce * (boostActive ? driftBoost : 1f)
            : speedInput * reverseForce;

        rearLeftCollider.motorTorque  = torque;
        rearRightCollider.motorTorque = torque;

        // Freno de mano
        float brake = Input.GetKey(KeyCode.Space) ? brakeForce : 0f;

        // Freno suave al soltar el gas
        if (Mathf.Approximately(speedInput, 0f))
            brake = brakeForce * 0.15f;

        frontLeftCollider.brakeTorque  = brake;
        frontRightCollider.brakeTorque = brake;
        rearLeftCollider.brakeTorque   = brake;
        rearRightCollider.brakeTorque  = brake;

        // Desactivar boost después de un tiempo
        if (boostActive)
        {
            boostTimer -= Time.fixedDeltaTime;
            if (boostTimer <= 0f) boostActive = false;
        }
    }

    // ── Dirección con interpolación suave ───────
    void HandleSteering()
    {
        float targetSteer = maxSteerAngle * Input.GetAxis("Horizontal");

        // Reducir ángulo de giro a alta velocidad (como Mario Kart)
        float speedFactor = Mathf.Clamp01(rb.linearVelocity.magnitude / maxSpeed);
        targetSteer *= Mathf.Lerp(1f, 0.5f, speedFactor);

        currentSteer = Mathf.Lerp(currentSteer, targetSteer, Time.fixedDeltaTime * steerSpeed);

        frontLeftCollider.steerAngle  = currentSteer;
        frontRightCollider.steerAngle = currentSteer;
    }

    // ── Sistema de Drift ────────────────────────
    void HandleDrift()
    {
        bool driftInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.X);
        bool isTurning  = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.3f;
        float speed     = rb.linearVelocity.magnitude;

        // Activar drift: debe estar girando, ir rápido y pulsar el botón
        isDrifting = driftInput && isTurning && speed > 8f;

        float targetStiffness = isDrifting ? driftStiffness : normalStiffness;
        currentStiffness = Mathf.Lerp(currentStiffness, targetStiffness,
                                       Time.fixedDeltaTime * gripTransition);

        // Aplicar fricción a las 4 ruedas
        SetSidewaysFriction(frontLeftCollider,  currentStiffness);
        SetSidewaysFriction(frontRightCollider, currentStiffness);
        SetSidewaysFriction(rearLeftCollider,   currentStiffness * 0.85f); // traseras deslizan más
        SetSidewaysFriction(rearRightCollider,  currentStiffness * 0.85f);

        // Al soltar el drift activa boost breve (mini-turbo)
        if (!driftInput && isDrifting == false && boostTimer <= 0f)
        {
            boostActive = true;
            boostTimer  = 0.6f;
        }
    }

    void SetSidewaysFriction(WheelCollider col, float stiffness)
    {
        WheelFrictionCurve curve = col.sidewaysFriction;
        curve.stiffness          = stiffness;
        col.sidewaysFriction     = curve;
    }

    // ── Downforce: pega el coche al suelo ───────
    void ApplyDownforce()
    {
        rb.AddForce(-transform.up * downforce * rb.linearVelocity.magnitude);
    }

    // ── Sincronizar meshes visuales ─────────────
    void UpdateWheelMeshes()
    {
        UpdateWheelPose(frontLeftCollider,  frontLeftMesh);
        UpdateWheelPose(frontRightCollider, frontRightMesh);
        UpdateWheelPose(rearLeftCollider,   rearLeftMesh);
        UpdateWheelPose(rearRightCollider,  rearRightMesh);
    }

    void UpdateWheelPose(WheelCollider col, Transform mesh)
    {
        if (mesh == null) return;
        Vector3 pos; Quaternion rot;
        col.GetWorldPose(out pos, out rot);
        mesh.SetPositionAndRotation(pos, rot);
    }
}
