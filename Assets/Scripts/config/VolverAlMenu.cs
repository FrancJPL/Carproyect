using UnityEngine;
using UnityEngine.SceneManagement;

public class VolverAlMenu : MonoBehaviour
{
    public void IrAlMenu()
    {
        SceneManager.LoadScene("menu"); // Usa el nombre exacto de tu escena principal
        Debug.Log("¡Volviendo a la sala pocha!");
    }
}
