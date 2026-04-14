using UnityEngine;
using TMPro;

public class KartHUD : MonoBehaviour
{
    public Rigidbody kartRigidbody;
    public TextMeshProUGUI velocidadText;

    void Update()
    {
        // Convierte m/s a km/h y lo muestra
        float kmh = kartRigidbody.linearVelocity.magnitude * 3.6f;
        velocidadText.text = $"{Mathf.RoundToInt(kmh)} km/h";
    }
}