using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class RaceUI : MonoBehaviour
{
    public static RaceUI Instance { get; private set; }

    [Header("HUD en carrera")]
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI currentLapTimeText;
    public TextMeshProUGUI bestLapTimeText;
    public TextMeshProUGUI totalTimeText;
    public GameObject waitingText;

    [Header("Mensaje dirección contraria")]
    public TextMeshProUGUI wrongWayText;

    [Header("Pantalla final")]
    public GameObject finishPanel;
    public TextMeshProUGUI finishLapTimesText;
    public TextMeshProUGUI finishBestLapText;
    public TextMeshProUGUI finishTotalTimeText;
    public Button restartButton;
    public Button saveTimeButton;
    public Button mainMenuButton;

    [Header("Guardado")]
    public TimeSaver timeSaver;

    private float currentBestLap;
    private float currentTotalTime;
    private string currentCarName;
    private string currentMapName;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        finishPanel.SetActive(false);
        
        if (waitingText != null)
            waitingText.SetActive(true);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartRace);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        
        if (saveTimeButton != null)
            saveTimeButton.onClick.AddListener(OnSaveTimeButtonPressed);
        
        // Ocultar mensaje de dirección contraria al inicio
        if (wrongWayText != null)
            wrongWayText.gameObject.SetActive(false);
    }

    public void UpdateUI(int lap, int totalLaps, float lapTime, float bestLap, float total, bool raceActive)
    {
        if (waitingText != null)
            waitingText.SetActive(!raceActive);

        if (!raceActive) return;

        if (lapText != null)
            lapText.text = $"Vuelta  {lap} / {totalLaps}";
        
        if (currentLapTimeText != null)
            currentLapTimeText.text = $"Vuelta actual  {LapManager.FormatTime(lapTime)}";
        
        if (bestLapTimeText != null)
            bestLapTimeText.text = bestLap > 0
                ? $"Mejor vuelta  {LapManager.FormatTime(bestLap)}"
                : "Mejor vuelta  --:--.---";
        
        if (totalTimeText != null)
            totalTimeText.text = $"Tiempo total  {LapManager.FormatTime(total)}";
    }

    public void ShowWrongWayMessage(bool show)
    {
        if (wrongWayText != null)
        {
            wrongWayText.gameObject.SetActive(show);
            
            if (show)
            {
                wrongWayText.text = " ¡DIRECCIÓN CONTRARIA! ";
                wrongWayText.color = Color.red;
                Debug.Log("Mostrando mensaje de dirección contraria");
            }
            else
            {
                Debug.Log("Ocultando mensaje de dirección contraria");
            }
        }
        else
        {
            Debug.LogWarning("WrongWayText no está asignado en el Inspector");
        }
    }

    public void ShowFinishScreen(List<float> lapTimes, float bestLap, float total, string carName, string mapName)
    {
        currentBestLap = bestLap;
        currentTotalTime = total;
        currentCarName = carName;
        currentMapName = mapName;
        
        if (finishPanel != null)
            finishPanel.SetActive(true);
        
        // Mostrar cursor al terminar la carrera
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        string lapLines = "";
        for (int i = 0; i < lapTimes.Count; i++)
            lapLines += $"Vuelta {i + 1}:  {LapManager.FormatTime(lapTimes[i])}\n";

        if (finishLapTimesText != null)
            finishLapTimesText.text = lapLines;
        
        if (finishBestLapText != null)
            finishBestLapText.text = $"Mejor vuelta:  {LapManager.FormatTime(bestLap)}";
        
        if (finishTotalTimeText != null)
            finishTotalTimeText.text = $"Tiempo total:  {LapManager.FormatTime(total)}";
        
        if (saveTimeButton != null)
            saveTimeButton.gameObject.SetActive(true);
    }
    
    public void OnSaveTimeButtonPressed()
    {
        if (timeSaver == null)
        {
            Debug.LogError("RaceUI: TimeSaver no asignado");
            return;
        }
        
        timeSaver.AbrirPanelGuardar(
            LapManager.FormatTime(currentBestLap),
            LapManager.FormatTime(currentTotalTime),
            currentCarName,
            currentMapName
        );
    }

    void RestartRace()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    void GoToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("menu");
    }
}