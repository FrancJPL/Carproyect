using UnityEngine;

public class AplicarPersonalizacion : MonoBehaviour
{
    public MaterialCoche materialCoche;
    public RuedasColores ruedasColores;

    void Start()
    {
        int cocheIdx = PlayerPrefs.GetInt("materialElegido", 0);
        int ruedasIdx = PlayerPrefs.GetInt("materialRuedasElegido", 0);

        if (materialCoche != null)
        {
            materialCoche.CambiarMaterial(cocheIdx);
        }

        if (ruedasColores != null)
        {
            ruedasColores.usarPlayerPrefs = false;
            ruedasColores.AplicarRuntime(ruedasColores.GetMaterialByIndex(ruedasIdx));
        }
    }
}
