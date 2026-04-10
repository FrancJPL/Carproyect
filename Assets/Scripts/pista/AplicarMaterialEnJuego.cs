using UnityEngine;

public class AplicarMaterialEnJuego : MonoBehaviour
{
    public Renderer cocheRenderer;
    public Material[] materialesDisponibles;

    void Start()
    {
        int indice = PlayerPrefs.GetInt("materialElegido", 0); // ← por defecto el primero

        if (materialesDisponibles.Length > indice && cocheRenderer != null)
        {
            Material[] materialesActuales = cocheRenderer.materials;
            materialesActuales[0] = materialesDisponibles[indice];
            cocheRenderer.materials = materialesActuales;
        }
    }
}
