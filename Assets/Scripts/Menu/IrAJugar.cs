using UnityEngine;
using UnityEngine.SceneManagement;

public class IrAJugar : MonoBehaviour
{
    public void CargarEscenaJuego()
    {
        SceneManager.LoadScene("Trak1"); // ← asegúrate de que el nombre de la escena es exacto
    }
}
