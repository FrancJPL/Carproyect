using UnityEngine;
using UnityEngine.SceneManagement;

public class GestorMusica : MonoBehaviour
{
    public AudioClip musicaMenu;
    public AudioClip musicaPersonalizar;
    public AudioClip musicaPista;

    private AudioSource audioSource;
    private static GestorMusica instancia;

    void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = musicaMenu;
            audioSource.loop = true;
            audioSource.Play();

            SceneManager.sceneLoaded += CambiarMusicaSegunEscena;
        }
        else
        {
            Destroy(gameObject); // evita duplicados
        }

        

    }

    void CambiarMusicaSegunEscena(Scene escena, LoadSceneMode modo)
    {
        if (escena.name == "PersonalizarCoches")
        {
            CambiarMusica(musicaPersonalizar);
        }
        else if (escena.name == "Pista nevada")
        {
            CambiarMusica(musicaPista);
        }
        else if (escena.name == "menu" || escena.name == "Opciones")
        {
            CambiarMusica(musicaMenu);
        }
    }

    void CambiarMusica(AudioClip nuevaMusica)
    {
        if (audioSource.clip != nuevaMusica)
        {
            audioSource.clip = nuevaMusica;
            audioSource.Play();
        }
    }
}
