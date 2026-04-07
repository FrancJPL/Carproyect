using UnityEngine;
using System.Collections.Generic;

public class LapManager : MonoBehaviour
{
    public static LapManager Instance { get; private set; }

    [Header("Configuración")]
    public int totalLaps = 3;
    public int totalCheckpoints = 4;

    [Header("Respawn")]
    public Transform[] checkpointRespawnPoints;
    public Transform finishRespawnPoint;

    private bool raceStarted = false;
    private bool raceFinished = false;
    private int currentLap = 0;
    private int nextCheckpointExpected = 0;
    private int lastCheckpointReached = -1;

    private float totalTime = 0f;
    private float currentLapTime = 0f;
    private float bestLapTime = float.MaxValue;
    private List<float> lapTimes = new List<float>();

    private Transform playerTransform;
    private Rigidbody playerRigidbody;
    private WheelCollider[] playerWheels;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Busca por tag "Player" — lo tiene KartRoot y CarBody
        // Nos quedamos con el que tenga Rigidbody, que es KartRoot
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject go in players)
        {
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb != null)
            {
                playerRigidbody = rb;
                playerTransform = go.transform;
                playerWheels = go.GetComponentsInChildren<WheelCollider>();
                Debug.Log($"LapManager: Rigidbody encontrado en {go.name}");
                break;
            }
        }

        if (playerRigidbody == null)
            Debug.LogError("LapManager: no encontré ningún Rigidbody en objetos con tag Player");

        RaceUI.Instance?.UpdateUI(0, totalLaps, 0f, 0f, 0f, false);
    }

    void Update()
    {
        if (!raceStarted || raceFinished) return;

        currentLapTime += Time.deltaTime;
        totalTime += Time.deltaTime;

        RaceUI.Instance?.UpdateUI(
            currentLap, totalLaps,
            currentLapTime,
            bestLapTime == float.MaxValue ? 0f : bestLapTime,
            totalTime,
            true
        );

        if (Input.GetKeyDown(KeyCode.R))
            Respawn();
    }

    public void OnFinishLineCrossed()
    {
        if (raceFinished) return;

        if (!raceStarted)
        {
            raceStarted = true;
            currentLap = 1;
            nextCheckpointExpected = 0;
            lastCheckpointReached = -1;
            currentLapTime = 0f;
            totalTime = 0f;
            Debug.Log("Carrera iniciada — Vuelta 1");
            return;
        }

        if (nextCheckpointExpected != 0)
        {
            Debug.Log($"Meta inválida: falta checkpoint {nextCheckpointExpected}");
            return;
        }

        float finishedLapTime = currentLapTime;
        lapTimes.Add(finishedLapTime);

        if (finishedLapTime < bestLapTime)
            bestLapTime = finishedLapTime;

        currentLap++;
        currentLapTime = 0f;
        lastCheckpointReached = -1;
        nextCheckpointExpected = 0;

        Debug.Log($"Vuelta completada en {FormatTime(finishedLapTime)}");

        if (currentLap > totalLaps)
        {
            raceFinished = true;
            RaceUI.Instance?.ShowFinishScreen(lapTimes, bestLapTime, totalTime);
        }
    }

    public void OnCheckpointCrossed(int index)
    {
        if (!raceStarted || raceFinished) return;
        if (index != nextCheckpointExpected)
        {
            Debug.Log($"Checkpoint {index} ignorado, esperaba {nextCheckpointExpected}");
            return;
        }

        lastCheckpointReached = index;
        nextCheckpointExpected = index + 1;

        if (nextCheckpointExpected >= totalCheckpoints)
            nextCheckpointExpected = 0;

        Debug.Log($"Checkpoint {index} ✓  — siguiente: {nextCheckpointExpected}");
    }

    void Respawn()
    {
        if (playerTransform == null || playerRigidbody == null)
        {
            Debug.LogError("Respawn: playerTransform o playerRigidbody es null");
            return;
        }

        Transform target = GetRespawnTarget();
        if (target == null)
        {
            Debug.LogWarning("Respawn: no hay Transform asignado en el Inspector");
            return;
        }

        // Desactiva WheelColliders antes de mover
        foreach (var w in playerWheels) w.enabled = false;

        // Para el Rigidbody completamente
        playerRigidbody.linearVelocity = Vector3.zero;
        playerRigidbody.angularVelocity = Vector3.zero;
        playerRigidbody.isKinematic = true;

        // Teleporta
        playerTransform.position = target.position;
        playerTransform.rotation = target.rotation;

        // Reactiva
        playerRigidbody.isKinematic = false;
        foreach (var w in playerWheels) w.enabled = true;

        Debug.Log($"Respawn en: {target.name}  pos: {target.position}");
    }

    Transform GetRespawnTarget()
    {
        if (lastCheckpointReached == -1)
            return finishRespawnPoint;

        if (checkpointRespawnPoints != null &&
            lastCheckpointReached < checkpointRespawnPoints.Length)
            return checkpointRespawnPoints[lastCheckpointReached];

        return finishRespawnPoint;
    }

    public static string FormatTime(float t)
    {
        int min = (int)(t / 60);
        int sec = (int)(t % 60);
        int ms = (int)((t * 1000) % 1000);
        return $"{min:00}:{sec:00}.{ms:000}";
    }
}