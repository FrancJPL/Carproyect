using UnityEngine;
using UnityEngine.SceneManagement;

public class IrAOpcionesDesdeJuego : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // GestorEscenaAnterior.GuardarEscena(SceneManager.GetActiveScene().name);
            SceneManager.LoadScene("Opciones");
        }
    }
}
