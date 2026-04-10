using UnityEngine;
using UnityEngine.SceneManagement;

public class IrAOpciones : MonoBehaviour
{
    public void CargarOpciones()
    {
        SceneManager.LoadScene("Opciones"); // ← asegúrate de que el nombre de la escena es exacto
    }
}
