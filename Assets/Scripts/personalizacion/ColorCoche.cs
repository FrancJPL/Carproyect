using UnityEngine;

public class MaterialCoche : MonoBehaviour
{
    public Renderer cocheRenderer; // ← arrastra aquí el objeto con el MeshRenderer del coche
    public Material[] materialesDisponibles; // ← lista de materiales (color base en índice 0)

    // Método general para cambiar material por índice
    public void CambiarMaterial(int indice)
    {
        if (cocheRenderer != null && materialesDisponibles.Length > indice)
        {
            Material[] materialesActuales = cocheRenderer.materials;
            materialesActuales[0] = materialesDisponibles[indice]; // ← cambia solo el Element 0
            cocheRenderer.materials = materialesActuales;

            PlayerPrefs.SetInt("materialElegido", indice); // ← guarda el índice
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogWarning("No se pudo cambiar el material: índice fuera de rango o renderer no asignado");
        }
    }

    // Botón “Color Inicial” (color base)
    public void AplicarColorInicial()
    {
        CambiarMaterial(0); // ← color base
    }

    // Botones personalizados
    public void AplicarColor1()
    {
        CambiarMaterial(1);
    }

    public void AplicarColor2()
    {
        CambiarMaterial(2);
    }

    public void AplicarColor3()
    {
        CambiarMaterial(3);
    }

    public void AplicarColor4()
    {
        CambiarMaterial(4);
    }
}

