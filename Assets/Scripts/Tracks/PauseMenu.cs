using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject pausePanel;           // Panel principal del menú de pausa
    public Button resumeButton;             // Botón para reanudar
    public Button restartButton;            // Botón para reiniciar carrera
    public Button mainMenuButton;           // Botón para volver al menú principal
    public Toggle cameraEffectsToggle;      // Checkbox para efectos de cámara
    public TextMeshProUGUI cameraEffectsLabel; // Texto opcional para el toggle
    
    [Header("Opciones")]
    public bool startWithCameraEffects = true; // Valor por defecto
    
    private bool isPaused = false;
    private CarController carController;
    private KartCamera kartCamera;
    private LapManager lapManager;
    
    void Start()
    {
        // Ocultar panel de pausa al inicio
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
        // Buscar referencias
        carController = FindObjectOfType<CarController>();
        kartCamera = FindObjectOfType<KartCamera>();
        lapManager = FindObjectOfType<LapManager>();
        
        // Configurar botones
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartRace);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        
        // Configurar toggle de efectos de cámara
        if (cameraEffectsToggle != null)
        {
            // Cargar valor guardado
            int savedValue = PlayerPrefs.GetInt("CameraEffects", startWithCameraEffects ? 1 : 0);
            cameraEffectsToggle.isOn = savedValue == 1;
            cameraEffectsToggle.onValueChanged.AddListener(OnCameraEffectsToggled);
            
            // Aplicar valor inicial
            ApplyCameraEffects(cameraEffectsToggle.isOn);
        }
        
        // Asegurar que el cursor esté visible solo en pausa
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void Update()
    {
        // Detectar tecla ESC
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
        // No pausar si la carrera ya terminó
        if (lapManager != null && lapManager.IsRaceFinished())
            return;
        
        isPaused = true;
        Time.timeScale = 0f; // Pausar el juego
        
        // Mostrar panel de pausa
        if (pausePanel != null)
            pausePanel.SetActive(true);
        
        // Mostrar cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Opcional: silenciar audio al pausar
        AudioListener.pause = true;
        
        Debug.Log("Juego pausado");
    }
    
    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Reanudar el juego
        
        // Ocultar panel de pausa
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
        // Ocultar cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        // Reanudar audio
        AudioListener.pause = false;
        
        Debug.Log("Juego reanudado");
    }
    
    void RestartRace()
    {
        // Reanudar tiempo antes de recargar
        Time.timeScale = 1f;
        AudioListener.pause = false;
        
        // Recargar la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    void GoToMainMenu()
    {
        // Reanudar tiempo antes de cambiar de escena
        Time.timeScale = 1f;
        AudioListener.pause = false;
        
        // Cargar menú principal (ajusta el nombre de la escena según tu proyecto)
        SceneManager.LoadScene("menu");
    }
    
    void OnCameraEffectsToggled(bool isOn)
    {
        // Guardar preferencia
        PlayerPrefs.SetInt("CameraEffects", isOn ? 1 : 0);
        PlayerPrefs.Save();
        
        // Aplicar efectos
        ApplyCameraEffects(isOn);
        
        Debug.Log($"Efectos de cámara: {(isOn ? "Activados" : "Desactivados")}");
    }
    
    void ApplyCameraEffects(bool enabled)
    {
        // Buscar el Volume de Post Processing
        UnityEngine.Rendering.Volume volume = FindObjectOfType<UnityEngine.Rendering.Volume>();
        
        if (volume != null)
        {
            volume.enabled = enabled;
            Debug.Log($"Volume de post-procesado {(enabled ? "activado" : "desactivado")}");
        }
        else
        {
            Debug.LogWarning("No se encontró ningún Volume en la escena");
        }
        
        // También buscar CameraEffectsManager personalizado
        CameraEffectsManager effectsManager = FindObjectOfType<CameraEffectsManager>();
        if (effectsManager != null)
        {
            effectsManager.SetEffectsEnabled(enabled);
        }
    }
    
    // Método público para saber si el juego está pausado
    public bool IsPaused() => isPaused;
    
    // Asegurar que al destruir el objeto se reanude el tiempo
    void OnDestroy()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}