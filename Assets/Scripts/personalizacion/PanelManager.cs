using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject panelMenuPrincipal;
    public GameObject panelColor;
    public GameObject panelRuedas;

    void Start()
    {
        // Al iniciar, solo mostrar el menú principal
        panelMenuPrincipal.SetActive(true);
        panelColor.SetActive(false);
        panelRuedas.SetActive(false);
    }

    public void AbrirPanelColor()
    {
        OcultarTodos();
        panelColor.SetActive(true);
    }

    public void AbrirPanelRuedas()
    {
        OcultarTodos();
        panelRuedas.SetActive(true);
    }

    public void VolverAlMenu()
    {
        OcultarTodos();
        panelMenuPrincipal.SetActive(true);
    }

    private void OcultarTodos()
    {
        panelMenuPrincipal.SetActive(false);
        panelColor.SetActive(false);
        panelRuedas.SetActive(false);
    }
}
