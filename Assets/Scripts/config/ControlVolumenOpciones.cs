using UnityEngine;
using UnityEngine.UI;

public class OpcionesAudio : MonoBehaviour
{
    public AudioSource musica;   // ← ahora sí es AudioSource
    public Slider sliderVolumen;

    void Start()
    {
        sliderVolumen.value = musica.volume; // el slider empieza al volumen actual
    }

    public void CambiarVolumen()
    {
        musica.volume = sliderVolumen.value;
    }
}
