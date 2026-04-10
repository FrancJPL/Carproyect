using UnityEngine;
using UnityEngine.SceneManagement;

public class CambioEscena : MonoBehaviour
{
    public void VolverAlMenu()
    {
        SceneManager.LoadScene("menu"); // Usa el nombre exacto de la escena
        Debug.Log("Volviendo al menú principal pocho.");
    }
}
