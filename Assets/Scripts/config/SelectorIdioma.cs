using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectorIdioma : MonoBehaviour
{
    public TMP_Dropdown dropdownIdioma;
    public TextMeshProUGUI textoVolumen;
    public TextMeshProUGUI textoBoton;
    public TextMeshProUGUI textoFrasePersonaje;

    void Start()
    {
        dropdownIdioma.onValueChanged.AddListener(CambiarIdioma);
        CambiarIdioma(dropdownIdioma.value); // aplica idioma al iniciar
    }

    void CambiarIdioma(int indice)
    {
        switch (indice)
        {
            case 0: // Español
                textoVolumen.text = "Volumen";
                textoBoton.text = "Volver";
                break;
            case 1: // Català
                textoVolumen.text = "Volum";
                textoBoton.text = "Tornar";
                textoFrasePersonaje.text = "Això està molt pocho!";
                break;
            case 2: // English
                textoVolumen.text = "Volume";
                textoBoton.text = "Back";
                textoFrasePersonaje.text = "This is super pocho!";
                break;
        }
    }
}
