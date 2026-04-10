using UnityEngine;

/// <summary>
/// Mueve una nube de izquierda a derecha en loop infinito.
/// Cuando sale por la derecha, reaparece por la izquierda.
///
/// CÓMO USAR:
/// 1. Importa nube1.png, nube2.png, nube3.png como Sprites
/// 2. Crea 3 objetos UI > Image dentro del Canvas (uno por nube)
/// 3. Asigna un sprite distinto a cada uno
/// 4. Añade este script a cada objeto nube
/// 5. Ajusta la velocidad y posición Y desde el Inspector
/// </summary>
public class NubeAnimada : MonoBehaviour
{
    [Header("=== MOVIMIENTO ===")]
    [Tooltip("Velocidad de movimiento (px/seg). Más alto = más rápida")]
    public float velocidad = 60f;

    [Tooltip("Retraso inicial antes de que empiece a moverse")]
    public float retrasoInicial = 0f;

    [Header("=== LÍMITES DE PANTALLA ===")]
    [Tooltip("Posición X donde reaparece (fuera por la izquierda)")]
    public float xInicio = -400f;

    [Tooltip("Posición X donde desaparece (fuera por la derecha)")]
    public float xFin = 1600f;

    [Header("=== FLOTACIÓN VERTICAL ===")]
    [Tooltip("Activar pequeña flotación arriba/abajo mientras se mueve")]
    public bool flotacionActivada = true;

    [Tooltip("Velocidad de la flotación")]
    public float velocidadFlotacion = 0.6f;

    [Tooltip("Amplitud de la flotación en píxeles")]
    public float amplitudFlotacion = 6f;

    // ── privadas ──
    private RectTransform rt;
    private Vector2 posOriginal;
    private bool moviendose = false;
    private float timerRetraso = 0f;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        posOriginal = rt.anchoredPosition;
    }

    void Start()
    {
        timerRetraso = retrasoInicial;
        if (retrasoInicial <= 0f)
            moviendose = true;
    }

    void Update()
    {
        // Esperar el retraso inicial
        if (!moviendose)
        {
            timerRetraso -= Time.deltaTime;
            if (timerRetraso <= 0f)
                moviendose = true;
            return;
        }

        // Mover hacia la derecha
        Vector2 pos = rt.anchoredPosition;
        pos.x += velocidad * Time.deltaTime;

        // Flotación vertical suave
        if (flotacionActivada)
        {
            pos.y = posOriginal.y + Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion;
        }

        // Si sale por la derecha → vuelve por la izquierda
        if (pos.x > xFin)
        {
            pos.x = xInicio;
            posOriginal.x = xInicio;
        }

        rt.anchoredPosition = pos;
    }

    /// <summary>
    /// Llama esto para reiniciar la nube a su posición inicial
    /// </summary>
    public void Reiniciar()
    {
        rt.anchoredPosition = posOriginal;
    }
}