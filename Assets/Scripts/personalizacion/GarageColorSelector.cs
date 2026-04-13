using UnityEngine;

public class GarageColorSelector : MonoBehaviour
{
    public MeshRenderer carRenderer;   // Aquí va el Mesh Renderer del CarBody
    public Material[] materiales;      // Lista de colores disponibles

    public void CambiarColor(int indice)
    {
        if (carRenderer == null) return;

        // Cambiar solo el material principal (Element 0)
        Material[] mats = carRenderer.materials;
        mats[0] = materiales[indice];
        carRenderer.materials = mats;

        // Guardar para el circuito
        PlayerPrefs.SetInt("colorElegido", indice);
        PlayerPrefs.Save();

        Debug.Log("Color guardado: " + indice);
    }
}
