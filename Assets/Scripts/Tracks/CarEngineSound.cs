using UnityEngine;

public class CarEngineSound : MonoBehaviour
{
    [Header("Audio Sources - Motor")]
    public AudioSource acceleratingSource;

    [Header("Audio Sources - Turbo")]
    public AudioSource blowOffSource;

    [Header("Referencias")]
    public Rigidbody carRigidbody;
    public float maxSpeed = 200f;

    [Header("Ajustes del motor")]
    public float minPitch = 0.8f;
    public float maxPitch = 1.5f;

    [Header("Ajustes del turbo")]
    public float turboPitchMultiplier = 1.3f;
    public KeyCode turboKey = KeyCode.LeftShift;

    private bool isTurboActive = false;
    private bool wasTurboActive = false;

    void Start()
    {
        acceleratingSource.loop = true;
        acceleratingSource.Play();
    }

    void Update()
    {
        isTurboActive = Input.GetKey(turboKey);

        float currentSpeed = carRigidbody.linearVelocity.magnitude * 3.6f;
        float t = Mathf.Clamp01(currentSpeed / maxSpeed);

        // -----------------------------
        // 🔥 VOLUMEN DEL MOTOR CONTROLADO POR EL SLIDER
        // -----------------------------
        acceleratingSource.volume = Mathf.Lerp(0.3f, 0.9f, t) * PauseMenu.volumenMotorGlobal;

        // Pitch base del motor
        float motorPitch = Mathf.Lerp(minPitch, maxPitch, t);

        // Turbo activo → pitch aumentado
        if (isTurboActive && currentSpeed > 30f)
        {
            acceleratingSource.pitch = motorPitch * turboPitchMultiplier;
        }
        else
        {
            acceleratingSource.pitch = motorPitch;
        }

        // -----------------------------
        // 🔥 SONIDO DEL TURBO CONTROLADO POR EL MISMO SLIDER
        // -----------------------------
        if (wasTurboActive && !isTurboActive && blowOffSource != null && currentSpeed > 30f)
        {
            blowOffSource.PlayOneShot(blowOffSource.clip, 0.7f * PauseMenu.volumenMotorGlobal);
        }

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
