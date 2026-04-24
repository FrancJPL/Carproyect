using UnityEngine;

public class CarEngineSound : MonoBehaviour
{
    [Header("Audio Sources - Motor")]
    public AudioSource idleSource;          // Ralentí
    public AudioSource acceleratingSource;  // Aceleración

    [Header("Audio Sources - Turbo")]
    public AudioSource turboSource;         // Sonido continuo del turbo
    public AudioSource blowOffSource;       // Sonido "psshh" al soltar turbo

    [Header("Referencias")]
    public Rigidbody carRigidbody;
    public float maxSpeed = 200f;

    [Header("Ajustes del motor")]
    public float minPitch = 0.8f;
    public float maxPitch = 1.5f;

    [Header("Ajustes del turbo")]
    public float turboPitchMultiplier = 1.3f;  // El pitch se multiplica x1.3 con turbo
    public float turboVolumeMultiplier = 1.5f; // El turbo suena más fuerte a alta velocidad
    public KeyCode turboKey = KeyCode.LeftShift;

    // Variables internas
    private bool isTurboActive = false;
    private bool wasTurboActive = false;

    void Start()
    {
        // Configurar loops
        idleSource.loop = true;
        acceleratingSource.loop = true;
        turboSource.loop = true;
        
        // Reproducir sonidos del motor (sonidos base siempre activos)
        idleSource.Play();
        acceleratingSource.Play();
        
        // El turbo empieza apagado
        turboSource.Stop();
    }

    void Update()
    {
        // Detectar estado del turbo
        isTurboActive = Input.GetKey(turboKey);
        
        // Velocidad actual en km/h
        float currentSpeed = carRigidbody.linearVelocity.magnitude * 3.6f;
        float t = Mathf.Clamp01(currentSpeed / maxSpeed);  // 0 = parado, 1 = máxima velocidad
        
        // ---- SONIDO DEL MOTOR (siempre activo) ----
        // Mezcla entre ralentí y aceleración
        idleSource.volume = Mathf.Lerp(0.8f, 0f, t);
        acceleratingSource.volume = Mathf.Lerp(0f, 0.8f, t);
        
        // Pitch base del motor
        float motorPitch = Mathf.Lerp(minPitch, maxPitch, t);
        
        // ---- TURBO: modifica el pitch si está activo ----
        if (isTurboActive && currentSpeed > 30f) // Turbo solo si vas a más de 30 km/h
        {
            // Aplica multiplicador de pitch
            acceleratingSource.pitch = motorPitch * turboPitchMultiplier;
            
            // Activar y ajustar volumen del sonido turbo
            if (!turboSource.isPlaying)
            {
                turboSource.Play();
            }
            
            // El turbo suena más fuerte cuanto más rápido vas
            float turboVolume = Mathf.Lerp(0.3f, 0.9f, t) * turboVolumeMultiplier;
            turboSource.volume = Mathf.Clamp01(turboVolume);
        }
        else
        {
            // Sin turbo: pitch normal
            acceleratingSource.pitch = motorPitch;
            
            // Apagar sonido del turbo gradualmente
            if (turboSource.isPlaying)
            {
                StartCoroutine(FadeOutTurbo());
            }
            
            // Sonido blow-off al soltar el turbo (solo la primera frame que se suelta)
            if (wasTurboActive && !isTurboActive && blowOffSource != null && currentSpeed > 30f)
            {
                blowOffSource.PlayOneShot(blowOffSource.clip, 0.7f);
            }
        }
        
        // Guardar estado anterior para detectar cuando se suelta el turbo
        wasTurboActive = isTurboActive;
    }
    
    // Apaga el turbo suavemente para que no se corte de golpe
    System.Collections.IEnumerator FadeOutTurbo()
    {
        float startVolume = turboSource.volume;
        float time = 0;
        while (time < 0.3f)
        {
            time += Time.deltaTime;
            turboSource.volume = Mathf.Lerp(startVolume, 0, time / 0.3f);
            yield return null;
        }
        turboSource.Stop();
        turboSource.volume = 0.5f; // Reset volumen
    }
    
    // Opcional: dibuja un texto en pantalla para saber si el turbo está activo (debug)
    void OnGUI()
    {
        if (isTurboActive)
        {
            GUI.Label(new Rect(10, 10, 200, 30), "🔧 TURBO ACTIVADO 🔧");
        }
    }
}