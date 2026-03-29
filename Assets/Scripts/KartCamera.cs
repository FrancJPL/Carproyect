using UnityEngine;

public class KartCamera : MonoBehaviour
{
    public Transform target;
    public float distance    = 6f;
    public float height      = 2.5f;
    public float smoothTime  = 0.08f;
    public float rotSmooth   = 5f;

    private Vector3    velocity = Vector3.zero;
    private Quaternion currentRot;

    void Start()
    {
        currentRot = transform.rotation;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Rotación suave hacia donde mira el coche
        Quaternion targetRot = Quaternion.Euler(0, target.eulerAngles.y, 0);
        currentRot = Quaternion.Slerp(currentRot, targetRot,
                                       Time.deltaTime * rotSmooth);

        // Posición detrás y arriba del coche
        Vector3 desiredPos = target.position
                           - (currentRot * Vector3.forward) * distance
                           + Vector3.up * height;

        // SmoothDamp elimina el temblor
        transform.position = Vector3.SmoothDamp(
            transform.position, desiredPos,
            ref velocity, smoothTime);

        // Mirar al coche ligeramente por encima del suelo
        transform.LookAt(target.position + Vector3.up * 1f);

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, 
                                     transform.eulerAngles.y, 
                                     0f); // fuerza Z a 0 siempre
    }
}