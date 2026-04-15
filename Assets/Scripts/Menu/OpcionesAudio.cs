using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OpcionesAudio : MonoBehaviour
{
    public AudioMixer audioMixer;   // El mixer donde controlas el volumen
    public Slider sliderVolumen;

    void Start()
    {
        float volumenGuardado = PlayerPrefs.GetFloat("volume", 0.75f);
        sliderVolumen.value = volumenGuardado;
        audioMixer.SetFloat("Volume", Mathf.Log10(volumenGuardado) * 20);
    }

    public void CambiarVolumen(float valor)
    {
        audioMixer.SetFloat("Volume", Mathf.Log10(valor) * 20);
        PlayerPrefs.SetFloat("volume", valor);
    }
}
