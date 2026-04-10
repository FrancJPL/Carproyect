using UnityEngine;
using UnityEngine.UI;

public class ControlVolumenOpciones : MonoBehaviour
{
    public Slider sliderVolumen;
    private AudioSource musicaFondo;

    void Start()
    {
        // Busca el objeto que vino del menú
        GameObject musicaGO = GameObject.Find("musica");
        if (musicaGO != null)
        {
            musicaFondo = musicaGO.GetComponent<AudioSource>();
            sliderVolumen.onValueChanged.AddListener(CambiarVolumen);
            sliderVolumen.value = musicaFondo.volume; // sincroniza al inicio
        }
        else
        {
            Debug.LogWarning("No se encontró el objeto 'musica'");
        }
    }

    void CambiarVolumen(float valor)
    {
        if (musicaFondo != null)
        {
            musicaFondo.volume = valor;
        }
    }
}
