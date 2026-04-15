using UnityEngine;

public class CameraEffectsManager : MonoBehaviour
{
    [Header("Componentes de Efectos")]
    public MonoBehaviour[] cameraEffects; // Arrastra aquí los scripts de efectos (MotionBlur, etc.)
    
    private bool effectsEnabled = true;
    
    void Start()
    {
        // Cargar preferencia guardada
        int savedValue = PlayerPrefs.GetInt("CameraEffects", 1);
        SetEffectsEnabled(savedValue == 1);
    }
    
    public void SetEffectsEnabled(bool enabled)
    {
        effectsEnabled = enabled;
        
        // Activar/desactivar cada efecto
        foreach (MonoBehaviour effect in cameraEffects)
        {
            if (effect != null)
                effect.enabled = enabled;
        }
        
        Debug.Log($"Efectos de cámara: {(enabled ? "Activados" : "Desactivados")}");
    }
    
    public bool AreEffectsEnabled() => effectsEnabled;
}