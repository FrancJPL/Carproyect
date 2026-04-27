using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button restartButton;
    public Button mainMenuButton;
    public Toggle cameraEffectsToggle;
    public TextMeshProUGUI cameraEffectsLabel;

    [Header("Sliders de Audio")]
    public Slider sliderMusica;
    public Slider sliderMotor;

    [Header("Audio Sources")]
    public AudioSource musicaNivel;
    public AudioSource audioMotor;
    public AudioSource audioTurbo;

    [Header("Opciones")]
    public bool startWithCameraEffects = true;

    private bool isPaused = false;
    private CarController carController;
    private KartCamera kartCamera;
    private LapManager lapManager;

    // 🔥 Variable global para que el CarController respete el volumen
    public static float volumenMotorGlobal = 1f;

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        carController = FindObjectOfType<CarController>();
        kartCamera = FindObjectOfType<KartCamera>();
        lapManager = FindObjectOfType<LapManager>();

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartRace);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        // --- SLIDER MÚSICA ---
        if (sliderMusica != null && musicaNivel != null)
        {
            sliderMusica.value = musicaNivel.volume;
            sliderMusica.onValueChanged.AddListener(delegate { CambiarVolumenMusica(); });
        }

        // --- SLIDER MOTOR ---
        if (sliderMotor != null && audioMotor != null)
        {
            sliderMotor.value = audioMotor.volume;
            sliderMotor.onValueChanged.AddListener(delegate { CambiarVolumenMotor(); });
        }

        // --- EFECTOS DE CÁMARA ---
        if (cameraEffectsToggle != null)
        {
            int savedValue = PlayerPrefs.GetInt("CameraEffects", startWithCameraEffects ? 1 : 0);
            cameraEffectsToggle.isOn = savedValue == 1;
            cameraEffectsToggle.onValueChanged.AddListener(OnCameraEffectsToggled);
            ApplyCameraEffects(cameraEffectsToggle.isOn);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    void PauseGame()
    {
        if (lapManager != null && lapManager.IsRaceFinished())
            return;

        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 🔥 NO PAUSAR EL AUDIO GLOBAL
        // AudioListener.pause = true;
    }

    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // 🔥 NO REANUDAR AUDIO GLOBAL
        // AudioListener.pause = false;
    }

    void RestartRace()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("menu");
    }

    // --- AUDIO ---

    public void CambiarVolumenMusica()
    {
        if (musicaNivel != null)
            musicaNivel.volume = sliderMusica.value;
    }

    public void CambiarVolumenMotor()
    {
        volumenMotorGlobal = sliderMotor.value;

        if (audioMotor != null)
            audioMotor.volume = volumenMotorGlobal;

        if (audioTurbo != null)
            audioTurbo.volume = volumenMotorGlobal;
    }

    // --- EFECTOS DE CÁMARA ---

    void OnCameraEffectsToggled(bool isOn)
    {
        PlayerPrefs.SetInt("CameraEffects", isOn ? 1 : 0);
        PlayerPrefs.Save();
        ApplyCameraEffects(isOn);
    }

    void ApplyCameraEffects(bool enabled)
    {
        UnityEngine.Rendering.Volume volume = FindObjectOfType<UnityEngine.Rendering.Volume>();

        if (volume != null)
            volume.enabled = enabled;

        CameraEffectsManager effectsManager = FindObjectOfType<CameraEffectsManager>();
        if (effectsManager != null)
            effectsManager.SetEffectsEnabled(enabled);
    }

    public bool IsPaused() => isPaused;

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
