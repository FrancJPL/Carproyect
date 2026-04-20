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

    [Header("Mensajes y progreso")]
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI checkpointProgressText;
    public TextMeshProUGUI wrongWayText;

    [Header("Boost UI")]
    public Image boostBarFill;
    public TextMeshProUGUI boostPercentageText;

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
    private Coroutine wrongWayCoroutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (finishPanel != null)
            finishPanel.SetActive(false);

        if (waitingText != null)
            waitingText.SetActive(true);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartRace);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        if (saveTimeButton != null)
            saveTimeButton.onClick.AddListener(OnSaveTimeButtonPressed);

        if (messageText != null)
            messageText.gameObject.SetActive(false);

        if (wrongWayText != null)
            wrongWayText.gameObject.SetActive(false);

        if (checkpointProgressText != null)
            checkpointProgressText.text = "";
    }

    public void UpdateUI(int lap, int totalLaps, float lapTime, float bestLap, float total, bool raceActive)
    {
        if (waitingText != null)
            waitingText.SetActive(!raceActive);

        if (!raceActive) return;

        if (lapText != null)
            lapText.text = $"Vuelta {lap} / {totalLaps}";

        if (currentLapTimeText != null)
            currentLapTimeText.text = $"Vuelta actual {LapManager.FormatTime(lapTime)}";

        if (bestLapTimeText != null)
            bestLapTimeText.text = bestLap > 0
                ? $"Mejor vuelta {LapManager.FormatTime(bestLap)}"
                : "Mejor vuelta --:--.---";

        if (totalTimeText != null)
            totalTimeText.text = $"Tiempo total {LapManager.FormatTime(total)}";
    }

    public void UpdateBoostUI(float fillAmount, float currentBoost, float maxBoost)
    {
        if (boostBarFill != null)
        {
            boostBarFill.fillAmount = fillAmount;
        }
        
        if (boostPercentageText != null)
        {
            int percentage = Mathf.RoundToInt(fillAmount * 100f);
            boostPercentageText.text = $"{percentage}%";
        }
    }

    public void ShowWrongWayMessage(float duration = 5f)
    {
        if (wrongWayText == null) return;

        if (wrongWayCoroutine != null)
            StopCoroutine(wrongWayCoroutine);

        wrongWayText.gameObject.SetActive(true);
        wrongWayText.text = "DIRECCION CONTRARIA";
        wrongWayText.color = Color.red;
        
        wrongWayCoroutine = StartCoroutine(HideWrongWayAfterDelay(duration));
    }

    IEnumerator HideWrongWayAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (wrongWayText != null)
        {
            wrongWayText.gameObject.SetActive(false);
        }
    }

    public void ShowMessage(string message, float duration = 2f)
    {
        if (messageText == null) return;
        
        messageText.gameObject.SetActive(true);
        messageText.text = message;
        StartCoroutine(HideMessageAfterDelay(duration));
    }

    IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    public void UpdateCheckpointProgress(int completed, int total)
    {
        if (checkpointProgressText == null) return;
        checkpointProgressText.text = $"Checkpoints: {completed}/{total}";
        
        if (completed == total)
            checkpointProgressText.color = Color.green;
        else if (completed > total / 2)
            checkpointProgressText.color = Color.yellow;
        else
            checkpointProgressText.color = Color.white;
    }

    public void ShowFinishScreen(List<float> lapTimes, float bestLap, float total, string carName, string mapName)
    {
        currentBestLap = bestLap;
        currentTotalTime = total;
        currentCarName = carName;
        currentMapName = mapName;

        if (finishPanel != null)
            finishPanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        string lapLines = "";
        for (int i = 0; i < lapTimes.Count; i++)
            lapLines += $"Vuelta {i + 1}: {LapManager.FormatTime(lapTimes[i])}\n";

        if (finishLapTimesText != null)
            finishLapTimesText.text = lapLines;

        if (finishBestLapText != null)
            finishBestLapText.text = $"Mejor vuelta: {LapManager.FormatTime(bestLap)}";

        if (finishTotalTimeText != null)
            finishTotalTimeText.text = $"Tiempo total: {LapManager.FormatTime(total)}";

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