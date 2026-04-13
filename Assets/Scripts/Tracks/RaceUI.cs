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

    [Header("Pantalla final")]
    public GameObject finishPanel;
    public TextMeshProUGUI finishLapTimesText;
    public TextMeshProUGUI finishBestLapText;
    public TextMeshProUGUI finishTotalTimeText;
    public Button restartButton;
    public Button saveTimeButton;

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
        if (waitingText != null) waitingText.SetActive(true);
        restartButton.onClick.AddListener(RestartRace);
        
        if (saveTimeButton != null)
            saveTimeButton.onClick.AddListener(OnSaveTimeButtonPressed);
    }

    public void UpdateUI(int lap, int totalLaps, float lapTime, float bestLap, float total, bool raceActive)
    {
        if (waitingText != null)
            waitingText.SetActive(!raceActive);

        if (!raceActive) return;

        lapText.text = $"Vuelta  {lap} / {totalLaps}";
        currentLapTimeText.text = $"Vuelta actual  {LapManager.FormatTime(lapTime)}";
        bestLapTimeText.text = bestLap > 0
            ? $"Mejor vuelta  {LapManager.FormatTime(bestLap)}"
            : "Mejor vuelta  --:--.---";
        totalTimeText.text = $"Tiempo total  {LapManager.FormatTime(total)}";
    }

    public void ShowFinishScreen(List<float> lapTimes, float bestLap, float total, string carName, string mapName)
    {
        currentBestLap = bestLap;
        currentTotalTime = total;
        currentCarName = carName;
        currentMapName = mapName;
        
        finishPanel.SetActive(true);

        string lapLines = "";
        for (int i = 0; i < lapTimes.Count; i++)
            lapLines += $"Vuelta {i + 1}:  {LapManager.FormatTime(lapTimes[i])}\n";

        finishLapTimesText.text = lapLines;
        finishBestLapText.text = $"Mejor vuelta:  {LapManager.FormatTime(bestLap)}";
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}