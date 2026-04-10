using UnityEngine;

public class SalirManager : MonoBehaviour
{
    public GameObject panelConfirmacion;

    public void MostrarConfirmacion()
    {
        panelConfirmacion.SetActive(true);
    }

    public void CancelarSalir()
    {
        panelConfirmacion.SetActive(false);
    }

    public void ConfirmarSalir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
