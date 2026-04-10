using UnityEngine;

public class RotadorCocheAuto : MonoBehaviour
{
    public float velocidadRotacion = 20f;

    void Update()
    {
        // Rotación sobre el eje Y local (como la Tierra)
        transform.Rotate(0, velocidadRotacion * Time.deltaTime, 0, Space.Self);
    }
}
