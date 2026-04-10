using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Anima el cartel "Coches Pochos" con:
/// 1. Animación de entrada: aparece de golpe con rebote (pop-in)
/// 2. Animación continua: oscila suavemente (swing)
/// 
/// Cómo usar:
/// - Añade este script al GameObject del cartel (la Image del bocadillo)
/// - Ajusta los valores desde el Inspector
/// </summary>
public class AnimarCartel : MonoBehaviour
{
    [Header("=== ANIMACIÓN DE ENTRADA ===")]
    [Tooltip("Tiempo que tarda en aparecer el cartel")]
    public float tiempoEntrada = 0.5f;

    [Tooltip("Cuánto rebota al aparecer (1 = normal, 1.3 = mucho rebote)")]
    public float fuerzaRebote = 1.25f;

    [Tooltip("Retraso antes de que empiece a aparecer")]
    public float retraso = 0.2f;

    [Header("=== ANIMACIÓN CONTINUA (SWING) ===")]
    [Tooltip("Activar oscilación continua")]
    public bool swingActivado = true;

    [Tooltip("Velocidad de la oscilación")]
    public float velocidadSwing = 1.2f;

    [Tooltip("Ángulo máximo de oscilación en grados")]
    public float anguloSwing = 3f;

    [Header("=== ANIMACIÓN CONTINUA (FLOAT) ===")]
    [Tooltip("Activar flotación arriba/abajo")]
    public bool floatActivado = true;

    [Tooltip("Velocidad de la flotación")]
    public float velocidadFloat = 0.8f;

    [Tooltip("Distancia de flotación en píxeles")]
    public float distanciaFloat = 8f;

    // ── privadas ──────────────────────────────────────────
    private Vector3 posicionOriginal;
    private Vector3 escalaOriginal;
    private bool entradaCompleta = false;

    void Awake()
    {
        // Empieza invisible y a escala 0
        transform.localScale = Vector3.zero;
        posicionOriginal = transform.localPosition;
        escalaOriginal   = Vector3.one;
    }

    void Start()
    {
        StartCoroutine(AnimacionEntrada());
    }

    void Update()
    {
        if (!entradaCompleta) return;

        // Swing (rotación)
        if (swingActivado)
        {
            float rotZ = Mathf.Sin(Time.time * velocidadSwing) * anguloSwing;
            transform.localRotation = Quaternion.Euler(0f, 0f, rotZ);
        }

        // Float (posición Y)
        if (floatActivado)
        {
            float offsetY = Mathf.Sin(Time.time * velocidadFloat) * distanciaFloat;
            transform.localPosition = posicionOriginal + new Vector3(0f, offsetY, 0f);
        }
    }

    IEnumerator AnimacionEntrada()
    {
        yield return new WaitForSeconds(retraso);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / tiempoEntrada;
            t  = Mathf.Clamp01(t);

            // Curva con rebote (overshoot)
            float escala = EaseOutBack(t, fuerzaRebote);

            transform.localScale = escalaOriginal * escala;

            // Pequeña rotación durante la entrada
            float rot = Mathf.Sin(t * Mathf.PI) * -10f * (1f - t);
            transform.localRotation = Quaternion.Euler(0f, 0f, rot);

            yield return null;
        }

        transform.localScale    = escalaOriginal;
        transform.localRotation = Quaternion.identity;
        entradaCompleta = true;
    }

    // Función matemática de rebote (equivale al cubic-bezier del CSS)
    float EaseOutBack(float t, float overshoot = 1.70158f)
    {
        t -= 1f;
        return t * t * ((overshoot + 1f) * t + overshoot) + 1f;
    }
}