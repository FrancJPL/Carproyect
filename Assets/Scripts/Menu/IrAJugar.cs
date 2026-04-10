using UnityEngine;
using UnityEngine.SceneManagement;

public class IrAJugar : MonoBehaviour
{
    public void CargarEscenaJuego()
    {
        SceneManager.LoadScene("Pista nevada"); // ← asegúrate de que el nombre de la escena es exacto
    }
}
