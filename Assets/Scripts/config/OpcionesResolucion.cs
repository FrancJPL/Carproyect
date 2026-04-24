using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class OpcionesResolucion : MonoBehaviour
{
    public TMP_Dropdown dropdownResolucion;
    
    private List<string> resolucionesMostrar = new List<string>()
    {
        "1600 x 900",
        "1920 x 1080",
        "2560 x 1440"
    };
    
    void Start()
    {
        dropdownResolucion.ClearOptions();
        dropdownResolucion.AddOptions(resolucionesMostrar);
        
        string actual = Screen.width + " x " + Screen.height;
        int indiceActual = resolucionesMostrar.IndexOf(actual);
        if (indiceActual == -1) indiceActual = 1;
        
        dropdownResolucion.value = indiceActual;
        dropdownResolucion.RefreshShownValue();
        
        // 🔥 ESTO CONFIGURA EL EVENTO AUTOMÁTICAMENTE (no necesitas hacerlo manual)
        dropdownResolucion.onValueChanged.RemoveAllListeners();
        dropdownResolucion.onValueChanged.AddListener(CambiarResolucion);
    }
    
    public void CambiarResolucion(int indice)
    {
        Debug.Log("🟢 Cambiando resolución a índice: " + indice);
        
        string[] partes = resolucionesMostrar[indice].Split('x');
        int ancho = int.Parse(partes[0].Trim());
        int alto = int.Parse(partes[1].Trim());
        
        Screen.SetResolution(ancho, alto, Screen.fullScreen);
    }
}    