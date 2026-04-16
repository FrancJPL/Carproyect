using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class RaceUI : MonoBehaviour
{
    public static RaceUI Instance { get; private set; }

    [Header("HUD en carrera")]
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI currentLapTimeText;
    public TextMeshProUGUI bestLapTimeText;
    public TextMeshProUGUI totalTimeText;
    public GameObject waitingText;
    
    [Header("NUEVO: Mensajes y progreso")]
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI checkpointProgressText;
    public TextMeshProUGUI wrongWayText;

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
    private Coroutine messageCoroutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        finishPanel.SetActive(false);
        if (waitingText != null) waitingText.SetActive(true);
        if (restartButton != null) restartButton.onClick.AddListener(RestartRace);
        
        if (saveTimeButton != null)
            saveTimeButton.onClick.AddListener(OnSaveTimeButtonPressed);
            
        // Ocultar textos de mensajes al inicio
        if (messageText != null) messageText.gameObject.SetActive(false);
        if (wrongWayText != null) wrongWayText.gameObject.SetActive(false);
        if (checkpointProgressText != null) checkpointProgressText.text = "";
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
    
    // NUEVO: Mostrar mensaje temporal
    public void ShowMessage(string message, float duration = 2f)
    {
        if (messageText == null) return;
        
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);
        
        messageText.gameObject.SetActive(true);
        messageText.text = message;
        messageCoroutine = StartCoroutine(HideMessageAfterDelay(duration));
    }
    
    IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }
    
    // NUEVO: Mostrar mensaje de dirección contraria
    public void ShowWrongWayMessage(bool show)
    {
        if (wrongWayText != null)
        {
            wrongWayText.gameObject.SetActive(show);
            if (show)
            {
                wrongWayText.text = "⚠️ ¡DIRECCIÓN CONTRARIA! ⚠️";
            }
        }
    }
    
    // NUEVO: Actualizar progreso de checkpoints
    public void UpdateCheckpointProgress(int completed, int total)
    {
        if (checkpointProgressText != null)
        {
            checkpointProgressText.text = $"Checkpoints: {completed}/{total}";
            
            // Cambiar color según progreso
            if (completed == total)
                checkpointProgressText.color = Color.green;
            else if (completed > total / 2)
                checkpointProgressText.color = Color.yellow;
            else
                checkpointProgressText.color = Color.white;
        }
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