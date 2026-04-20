using UnityEngine;
using System.Collections.Generic;

public class NitroRespawnManager : MonoBehaviour
{
    [Header("Referencias")]
    public LapManager lapManager;
    
    private List<NitroPickup> allNitroPickups = new List<NitroPickup>();
    
    void Start()
    {
        // Buscar todos los nitro pickups en la escena
        NitroPickup[] pickups = FindObjectsOfType<NitroPickup>();
        allNitroPickups.AddRange(pickups);
        
        // Suscribirse al evento de vuelta completada
        if (lapManager != null)
        {
            // Nota: Necesitas agregar un evento en LapManager para cuando se completa una vuelta
            // Por ahora lo haremos manual o puedes agregar un evento
        }
        else
        {
            lapManager = FindObjectOfType<LapManager>();
        }
    }
    
    // Llamar este metodo cuando se complete una vuelta
    public void OnLapCompleted()
    {
        foreach (NitroPickup pickup in allNitroPickups)
        {
            if (pickup != null)
            {
                pickup.ForceRespawn();
            }
        }
        Debug.Log("Todos los nitros han reaparecido");
    }
}