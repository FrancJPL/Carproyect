using UnityEngine;

public class AntiRollbar : MonoBehaviour
{
    [Header("Ruedas")]
    public WheelCollider wheelLeft;
    public WheelCollider wheelRight;

    [Header("Fuerza anti-vuelco")]
    public float antiRollForce = 5000f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        WheelHit hitLeft, hitRight;
        float travelLeft  = 1f;
        float travelRight = 1f;

        bool groundedLeft  = wheelLeft.GetGroundHit(out hitLeft);
        bool groundedRight = wheelRight.GetGroundHit(out hitRight);

        if (groundedLeft)
            travelLeft = (-wheelLeft.transform.InverseTransformPoint(hitLeft.point).y
                         - wheelLeft.radius) / wheelLeft.suspensionDistance;

        if (groundedRight)
            travelRight = (-wheelRight.transform.InverseTransformPoint(hitRight.point).y
                          - wheelRight.radius) / wheelRight.suspensionDistance;

        float antiRollAmount = (travelLeft - travelRight) * antiRollForce;

        if (groundedLeft)
            rb.AddForceAtPosition(wheelLeft.transform.up  * -antiRollAmount, wheelLeft.transform.position);

        if (groundedRight)
            rb.AddForceAtPosition(wheelRight.transform.up *  antiRollAmount, wheelRight.transform.position);
    }
}