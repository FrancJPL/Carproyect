using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Cambia resolución de render + calidad visual. 
/// La ventana siempre ocupa toda la pantalla (FullScreenWindow/Borderless).
/// El tamaño visual cambia mediante render scale en la cámara principal.
/// </summary>
public class OpcionesResolucion : MonoBehaviour
{
    [Header("Referencias de UI")]
    public TMP_Dropdown dropdownResolucion;

    // ─── Estructura de cada opción ───────────────────────────────────────────
    private struct OpcionResolucion
    {
        public int    ancho;
        public int    alto;
        public string etiqueta;
    }

    private readonly List<OpcionResolucion> opciones = new List<OpcionResolucion>()
    {
        new OpcionResolucion { ancho = 1600, alto = 900,  etiqueta = "1600 x 900  (Bajo)"  },
        new OpcionResolucion { ancho = 1920, alto = 1080, etiqueta = "1920 x 1080 (Medio)" },
        new OpcionResolucion { ancho = 2560, alto = 1440, etiqueta = "2560 x 1440 (Alto)"  }
    };

    private const int INDICE_DEFAULT = 1; // 1920x1080

    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        InicializarMenu();
    }

    private void InicializarMenu()
    {
        if (dropdownResolucion == null)
        {
            Debug.LogError("[OpcionesResolucion] Asigna el TMP_Dropdown en el Inspector.");
            return;
        }

        // Llenar dropdown
        dropdownResolucion.ClearOptions();
        var etiquetas = new List<string>();
        foreach (var o in opciones) etiquetas.Add(o.etiqueta);
        dropdownResolucion.AddOptions(etiquetas);

        // Recuperar guardado
        int anchoGuardado = PlayerPrefs.GetInt("ResolucionAncho", opciones[INDICE_DEFAULT].ancho);
        int altoGuardado  = PlayerPrefs.GetInt("ResolucionAlto",  opciones[INDICE_DEFAULT].alto);
        int indice = opciones.FindIndex(o => o.ancho == anchoGuardado && o.alto == altoGuardado);
        if (indice == -1) indice = INDICE_DEFAULT;

        // Aplicar sin disparar listener
        dropdownResolucion.SetValueWithoutNotify(indice);
        dropdownResolucion.RefreshShownValue();
        AplicarResolucion(indice);

        // Listener
        dropdownResolucion.onValueChanged.RemoveAllListeners();
        dropdownResolucion.onValueChanged.AddListener(AplicarResolucion);
    }

    // ─── Botones ◄ ► ─────────────────────────────────────────────────────────

    public void BotonSiguiente()
    {
        int siguiente = dropdownResolucion.value + 1;
        if (siguiente < opciones.Count) AplicarResolucion(siguiente);
    }

    public void BotonAnterior()
    {
        int anterior = dropdownResolucion.value - 1;
        if (anterior >= 0) AplicarResolucion(anterior);
    }

    // ─── Lógica central ──────────────────────────────────────────────────────

    private void AplicarResolucion(int indice)
    {
        if (indice < 0 || indice >= opciones.Count) return;

        OpcionResolucion op = opciones[indice];

        // ── 1. CAMBIAR RESOLUCIÓN REAL DE RENDER ─────────────────────────────
        // FullScreenWindow = borderless fullscreen, funciona bien en Windows 10/11
        // Le decimos a Unity que renderice a esa resolución concreta
        Screen.SetResolution(op.ancho, op.alto, FullScreenMode.FullScreenWindow);

        // ── 2. FORZAR RESOLUCIÓN EN LA CÁMARA ────────────────────────────────
        // Esto evita que Unity simplemente escale el contenido sin cambiar calidad
        Camera cam = Camera.main;
        if (cam != null)
        {
            // Resolución nativa del monitor del jugador
            int monitorAncho = Display.main.systemWidth;
            int monitorAlto  = Display.main.systemHeight;

            // Centrar el viewport dentro de la pantalla nativa
            float offsetX = (monitorAncho - op.ancho) / 2f;
            float offsetY = (monitorAlto  - op.alto)  / 2f;

            cam.pixelRect = new Rect(offsetX, offsetY, op.ancho, op.alto);

            Debug.Log($"[Opciones] Camera.pixelRect → x:{offsetX} y:{offsetY} w:{op.ancho} h:{op.alto}");
        }

        // ── 3. GUARDAR ────────────────────────────────────────────────────────
        PlayerPrefs.SetInt("ResolucionAncho", op.ancho);
        PlayerPrefs.SetInt("ResolucionAlto",  op.alto);
        PlayerPrefs.Save();

        // Sincronizar UI (necesario cuando viene de botón ◄ ►)
        dropdownResolucion.SetValueWithoutNotify(indice);
        dropdownResolucion.RefreshShownValue();

        Debug.Log($"[Opciones] Resolución aplicada: {op.ancho}x{op.alto}");
    }
}