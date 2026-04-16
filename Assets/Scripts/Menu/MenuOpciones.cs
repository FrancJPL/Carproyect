using UnityEngine;

public class MenuOpciones : MonoBehaviour
{
    public GameObject panelOpciones;
    private bool opcionesAbiertas = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (opcionesAbiertas)
                CerrarOpciones();
            else
                AbrirOpciones();
        }
    }

    public void AbrirOpciones()
    {
        panelOpciones.SetActive(true);
        opcionesAbiertas = true;
    }

    public void CerrarOpciones()
    {
        panelOpciones.SetActive(false);
        opcionesAbiertas = false;
    }
}
