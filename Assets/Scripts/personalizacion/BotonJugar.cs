using UnityEngine;
using UnityEngine.SceneManagement;

public class BotonJugar : MonoBehaviour
{
    public void IrALaPista()
    {
        SceneManager.LoadScene("Llanura Soleada"); // Usa el nombre exacto de la escena
        Debug.Log("¡Vamos a correr pocho style!");
    }
}
