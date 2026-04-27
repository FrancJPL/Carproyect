using UnityEngine;

public class CarEngineSound : MonoBehaviour
{
    [Header("Audio Sources - Motor")]
    public AudioSource acceleratingSource;  // Aceleración (ÚNICO sonido de motor)

    [Header("Audio Sources - Turbo")]
    public AudioSource blowOffSource;       // Sonido "psshh" al soltar turbo

    [Header("Referencias")]
    public Rigidbody carRigidbody;
    public float maxSpeed = 200f;

    [Header("Ajustes del motor")]
    public float minPitch = 0.8f;
    public float maxPitch = 1.5f;

    [Header("Ajustes del turbo")]
    public float turboPitchMultiplier = 1.3f;
    public KeyCode turboKey = KeyCode.LeftShift;

    // Variables internas
    private bool isTurboActive = false;
    private bool wasTurboActive = false;

    void Start()
    {
        // Configurar loop del motor
        acceleratingSource.loop = true;
        
        // Reproducir sonido del motor
        acceleratingSource.Play();
    }

    void Update()
    {
        // Detectar estado del turbo
        isTurboActive = Input.GetKey(turboKey);
        
        // Velocidad actual en km/h
        float currentSpeed = carRigidbody.linearVelocity.magnitude * 3.6f;
        float t = Mathf.Clamp01(currentSpeed / maxSpeed);
        
        // ---- SONIDO DEL MOTOR ----
        // Volumen: más fuerte al ir rápido
        acceleratingSource.volume = Mathf.Lerp(0.3f, 0.9f, t);
        
        // Pitch base del motor
        float motorPitch = Mathf.Lerp(minPitch, maxPitch, t);
        
        // ---- TURBO: modifica el pitch si está activo ----
        if (isTurboActive && currentSpeed > 30f)
        {
            // Aplica multiplicador de pitch
            acceleratingSource.pitch = motorPitch * turboPitchMultiplier;
        }
        else
        {
            // Sin turbo: pitch normal
            acceleratingSource.pitch = motorPitch;
        }
        
        // ---- SONIDO BLOW-OFF (psshh) al soltar el turbo ----
        if (wasTurboActive && !isTurboActive && blowOffSource != null && currentSpeed > 30f)
        {
            blowOffSource.PlayOneShot(blowOffSource.clip, 0.7f);
        }
        
        // Guardar estado anterior
        wasTurboActive = isTurboActive;
    }
    
    void OnGUI()
    {
        if (isTurboActive)
        {
            GUI.Label(new Rect(10, 10, 200, 30), "🔧 TURBO ACTIVADO 🔧");
        }
    }
}