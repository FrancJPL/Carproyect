using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OpcionesResolucion : MonoBehaviour
{
    public TMP_Dropdown dropdownResolucion;
    
    private Resolution[] resoluciones;
    private List<string> opcionesResoluciones = new List<string>();

    void Start()
    {
        // Cargar resolución guardada o usar 1920x1080
        CargarResolucionGuardada();
        
        // Configurar el dropdown
        ConfigurarDropdown();
    }
    
    void CargarResolucionGuardada()
    {
        // Si hay resolución guardada, usarla
        if (PlayerPrefs.HasKey("ResolucionAncho"))
        {
            int ancho = PlayerPrefs.GetInt("ResolucionAncho");
            int alto = PlayerPrefs.GetInt("ResolucionAlto");
            bool fullscreen = PlayerPrefs.GetInt("PantallaCompleta", 1) == 1;
            
            Screen.SetResolution(ancho, alto, fullscreen);
        }
        else
        {
            // Primera vez: usar 1920x1080
            Screen.SetResolution(1920, 1080, true);
            PlayerPrefs.SetInt("ResolucionAncho", 1920);
            PlayerPrefs.SetInt("ResolucionAlto", 1080);
            PlayerPrefs.SetInt("PantallaCompleta", 1);
            PlayerPrefs.Save();
        }
    }
    
    void ConfigurarDropdown()
    {
        // Obtener todas las resoluciones soportadas
        resoluciones = Screen.resolutions;
        
        // Limpiar dropdown
        dropdownResolucion.ClearOptions();
        opcionesResoluciones.Clear();
        
        int indiceSeleccionado = 0;
        
        // Agregar resoluciones manualmente (las más comunes)
        AgregarResolucion(1920, 1080, ref indiceSeleccionado);
        AgregarResolucion(1600, 900, ref indiceSeleccionado);
        AgregarResolucion(1366, 768, ref indiceSeleccionado);
        AgregarResolucion(1280, 720, ref indiceSeleccionado);
        AgregarResolucion(1024, 768, ref indiceSeleccionado);
        AgregarResolucion(800, 600, ref indiceSeleccionado);
        
        // Si no hay resoluciones manuales, usar las del sistema
        if (opcionesResoluciones.Count == 0)
        {
            for (int i = 0; i < resoluciones.Length; i++)
            {
                string opcion = resoluciones[i].width + " x " + resoluciones[i].height;
                
                // Evitar duplicados
                if (!opcionesResoluciones.Contains(opcion))
                {
                    opcionesResoluciones.Add(opcion);
                    
                    // Verificar si es la resolución actual
                    if (resoluciones[i].width == Screen.width && 
                        resoluciones[i].height == Screen.height)
                    {
                        indiceSeleccionado = opcionesResoluciones.Count - 1;
                    }
                }
            }
        }
        
        // Aplicar opciones al dropdown
        dropdownResolucion.AddOptions(opcionesResoluciones);
        dropdownResolucion.value = indiceSeleccionado;
        dropdownResolucion.RefreshShownValue();
        
        // Añadir listener para cuando cambie el valor
        dropdownResolucion.onValueChanged.RemoveAllListeners();
        dropdownResolucion.onValueChanged.AddListener(delegate {
            CambiarResolucion(dropdownResolucion.value);
        });
    }
    
    void AgregarResolucion(int ancho, int alto, ref int indiceSeleccionado)
    {
        // Verificar si la resolución es soportada
        for (int i = 0; i < resoluciones.Length; i++)
        {
            if (resoluciones[i].width == ancho && resoluciones[i].height == alto)
            {
                string opcion = ancho + " x " + alto;
                opcionesResoluciones.Add(opcion);
                
                // Si es la resolución actual, marcar este índice
                if (Screen.width == ancho && Screen.height == alto)
                {
                    indiceSeleccionado = opcionesResoluciones.Count - 1;
                }
                break;
            }
        }
    }
    
    public void CambiarResolucion(int indice)
    {
        if (indice >= 0 && indice < opcionesResoluciones.Count)
        {
            // Extraer ancho y alto del string seleccionado
            string[] dimensiones = opcionesResoluciones[indice].Split('x');
            int ancho = int.Parse(dimensiones[0].Trim());
            int alto = int.Parse(dimensiones[1].Trim());
            
            // Cambiar resolución (manteniendo el modo de pantalla actual)
            bool esFullscreen = Screen.fullScreen;
            Screen.SetResolution(ancho, alto, esFullscreen);
            
            // Guardar preferencia
            PlayerPrefs.SetInt("ResolucionAncho", ancho);
            PlayerPrefs.SetInt("ResolucionAlto", alto);
            PlayerPrefs.Save();
            
            // Pequeña pausa para que se aplique
            StartCoroutine(RefrescarDropdownUI());
            
            Debug.Log($"Resolución cambiada a: {ancho}x{alto}");
        }
    }
    
    IEnumerator RefrescarDropdownUI()
    {
        yield return new WaitForEndOfFrame();
        
        // Forzar actualización del dropdown
        dropdownResolucion.RefreshShownValue();
        Canvas.ForceUpdateCanvases();
    }
    
    // Método público para alternar pantalla completa (opcional)
    public void AlternarPantallaCompleta(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt("PantallaCompleta", fullscreen ? 1 : 0);
        PlayerPrefs.Save();
        
        // Reaplicar resolución actual
        Screen.SetResolution(Screen.width, Screen.height, fullscreen);
    }
}