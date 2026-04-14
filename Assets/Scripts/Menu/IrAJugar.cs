using UnityEngine;
using UnityEngine.SceneManagement;

public class IrAJugar : MonoBehaviour
{
    public void CargarEscenaJuego()
    {
        SceneManager.LoadScene("Llanura Soleada"); // ← asegúrate de que el nombre de la escena es exacto
    }
}
