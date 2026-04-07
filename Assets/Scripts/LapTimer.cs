
using UnityEngine;
using TMPro;


public class LapTimer : MonoBehaviour
{
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI timeText;
    public int totalLaps = 3;


    private int currentLap = 1;
    private float lapTime = 0f;
    private bool raceFinished = false;


    void Update()
    {
        if (raceFinished) return;
        lapTime += Time.deltaTime;
        timeText.text = FormatTime(lapTime);
    }


    // Llama esto desde un Checkpoint Trigger
    public void CompleteLap()
    {
        Debug.Log($"Vuelta {currentLap} - Tiempo: {FormatTime(lapTime)}");
        lapTime = 0f;
        currentLap++;


        if (currentLap > totalLaps)
        {
            raceFinished = true;
            lapText.text = "¡CARRERA TERMINADA!";
            return;
        }
        lapText.text = $"Vuelta {currentLap} / {totalLaps}";
    }


    string FormatTime(float t)
    {
        int min = (int)(t / 60);
        int sec = (int)(t % 60);
        int ms = (int)((t * 1000) % 1000);
        return $"{min:00}:{sec:00}.{ms:000}";
    }
}