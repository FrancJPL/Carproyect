using UnityEngine;
using UnityEngine.SceneManagement;

public class BotonJugar : MonoBehaviour
{
    public void IrALaPista()
    {
        SceneManager.LoadScene("Pista nevada"); // Usa el nombre exacto de la escena
        Debug.Log("¡Vamos a correr pocho style!");
    }
}
