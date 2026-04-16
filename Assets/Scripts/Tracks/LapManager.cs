﻿using UnityEngine;
using System.Collections.Generic;

public class LapManager : MonoBehaviour
{
    public static LapManager Instance { get; private set; }

    // Evento para notificar cuando la carrera termina
    public System.Action OnRaceFinished;

    [Header("Configuración")]
    public int totalLaps = 3;
    public int totalCheckpoints = 4;

    [Header("Respawn")]
    public Transform[] checkpointRespawnPoints;
    public Transform finishRespawnPoint;

    [Header("Guardado")]
    public TimeSaver timeSaver;

    [Header("Detección de dirección")]
    public float wrongWayCheckDistance = 10f;  // Distancia para detectar si vas en dirección contraria
    public LayerMask trackLayer;               // Layer de la pista para raycast

    private bool raceStarted = false;
    private bool raceFinished = false;
    private int currentLap = 0;
    private int nextCheckpointExpected = 0;
    private int lastCheckpointReached = -1;
    
    // NUEVO: Array para trackear qué checkpoints se han completado en la vuelta actual
    private bool[] checkpointsCompletedThisLap;
    
    // NUEVO: Para detección de dirección contraria
    private bool isGoingWrongWay = false;
    private float wrongWayTimer = 0f;

    private float totalTime = 0f;
    private float currentLapTime = 0f;
    private float bestLapTime = float.MaxValue;
    private List<float> lapTimes = new List<float>();

    private Transform playerTransform;
    private Rigidbody playerRigidbody;
    private WheelCollider[] playerWheels;
    
    private string currentCarName = "";
    private string currentMapName = "";

    void Awake()
    {
        Instance = this;
        
        // Inicializar el array de checkpoints
        checkpointsCompletedThisLap = new bool[totalCheckpoints];
    }

    void Start()
    {
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

        currentCarName = playerTransform != null ? playerTransform.name : "Coche Desconocido";
        currentMapName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        RaceUI.Instance?.UpdateUI(0, totalLaps, 0f, 0f, 0f, false);
        
        ResetCheckpointsProgress();
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
            
        // NUEVO: Detectar si va en dirección contraria
        CheckWrongWay();
    }
    
    // NUEVO: Método para detectar si el jugador va en dirección contraria
    void CheckWrongWay()
    {
        if (playerTransform == null || !raceStarted) return;
        
        // Hacer un raycast hacia adelante para ver la dirección de la pista
        RaycastHit hit;
        Vector3 forward = playerTransform.forward;
        
        if (Physics.Raycast(playerTransform.position + Vector3.up * 0.5f, forward, out hit, wrongWayCheckDistance, trackLayer))
        {
            // Obtener la dirección "correcta" del track (puedes usar el vector forward del checkpoint más cercano)
            // Por ahora, usamos una simplificación: si la velocidad es negativa respecto al forward del coche
            float speedDot = Vector3.Dot(playerRigidbody.linearVelocity.normalized, forward);
            
            bool newWrongWay = speedDot < -0.3f; // Si va hacia atrás (velocidad negativa respecto al forward)
            
            if (newWrongWay != isGoingWrongWay)
            {
                isGoingWrongWay = newWrongWay;
                if (isGoingWrongWay)
                {
                    wrongWayTimer = 0f;
                    Debug.Log("<color=red>⚠️ DIRECCIÓN CONTRARIA ⚠️</color>");
                    
                    // Opcional: mostrar mensaje en UI
                    RaceUI.Instance?.ShowWrongWayMessage(true);
                }
                else
                {
                    RaceUI.Instance?.ShowWrongWayMessage(false);
                }
            }
        }
        
        // Si va en dirección contraria por más de 2 segundos, resetear progreso de checkpoints
        if (isGoingWrongWay)
        {
            wrongWayTimer += Time.deltaTime;
            if (wrongWayTimer > 2f)
            {
                ResetLapProgress();
                wrongWayTimer = 0f;
                Debug.Log("<color=yellow>Progreso de vuelta reiniciado por ir en dirección contraria</color>");
            }
        }
    }
    
    // NUEVO: Resetear progreso de la vuelta actual (sin perder la vuelta)
    void ResetLapProgress()
    {
        nextCheckpointExpected = 0;
        lastCheckpointReached = -1;
        ResetCheckpointsProgress();
        RaceUI.Instance?.ShowMessage("¡Has perdido el progreso de la vuelta!", 2f);
    }
    
    void ResetCheckpointsProgress()
    {
        for (int i = 0; i < totalCheckpoints; i++)
        {
            checkpointsCompletedThisLap[i] = false;
        }
    }

    public void OnFinishLineCrossed()
    {
        if (raceFinished) return;

        if (!raceStarted)
        {
            StartRace();
            return;
        }

        // NUEVO: Validación más estricta para completar vuelta
        if (IsValidLapCompletion())
        {
            CompleteLap();
        }
        else
        {
            // Mostrar qué checkpoints faltan
            string missing = GetMissingCheckpointsString();
            Debug.Log($"<color=orange>❌ Vuelta inválida! Te faltan los checkpoints: {missing}</color>");
            RaceUI.Instance?.ShowMessage($"¡Completa todos los checkpoints primero! Faltan: {missing}", 2f);
        }
    }
    
    // NUEVO: Verificar si todos los checkpoints han sido completados en orden
    bool IsValidLapCompletion()
    {
        // Debe haber completado todos los checkpoints
        for (int i = 0; i < totalCheckpoints; i++)
        {
            if (!checkpointsCompletedThisLap[i])
                return false;
        }
        
        // Además, el siguiente esperado debe ser 0 (volver a la meta después del último checkpoint)
        return nextCheckpointExpected == 0;
    }
    
    // NUEVO: Obtener string de checkpoints faltantes (para debug)
    string GetMissingCheckpointsString()
    {
        List<int> missing = new List<int>();
        for (int i = 0; i < totalCheckpoints; i++)
        {
            if (!checkpointsCompletedThisLap[i])
                missing.Add(i);
        }
        return missing.Count > 0 ? string.Join(", ", missing) : "ninguno";
    }
    
    void StartRace()
    {
        raceStarted = true;
        currentLap = 1;
        nextCheckpointExpected = 0;
        lastCheckpointReached = -1;
        currentLapTime = 0f;
        totalTime = 0f;
        ResetCheckpointsProgress();
        Debug.Log("<color=green>🏁 Carrera iniciada — Vuelta 1 🏁</color>");
    }
    
    void CompleteLap()
    {
        float finishedLapTime = currentLapTime;
        lapTimes.Add(finishedLapTime);

        if (finishedLapTime < bestLapTime)
            bestLapTime = finishedLapTime;

        currentLap++;
        currentLapTime = 0f;
        
        // Resetear progreso para la nueva vuelta
        ResetCheckpointsProgress();
        lastCheckpointReached = -1;
        nextCheckpointExpected = 0;

        Debug.Log($"<color=green>✅ Vuelta {currentLap - 1} completada en {FormatTime(finishedLapTime)} ✅</color>");

        if (currentLap > totalLaps)
        {
            FinishRace();
        }
    }
    
    void FinishRace()
    {
        raceFinished = true;
        
        // Disparar el evento para deshabilitar controles del coche
        OnRaceFinished?.Invoke();
        
        RaceUI.Instance?.ShowFinishScreen(lapTimes, bestLapTime, totalTime, currentCarName, currentMapName);
        Debug.Log("<color=cyan>🏆 ¡CARRERA TERMINADA! 🏆</color>");
    }

    public void OnCheckpointCrossed(int index)
    {
        if (!raceStarted || raceFinished) return;
        
        // Validar que el checkpoint sea el esperado
        if (index != nextCheckpointExpected)
        {
            // Si no es el esperado, puede ser que vaya en dirección contraria
            if (isGoingWrongWay)
            {
                Debug.Log($"<color=red>Checkpoint {index} ignorado - Vas en dirección contraria</color>");
            }
            else
            {
                Debug.Log($"<color=orange>Checkpoint {index} fuera de orden. Esperaba {nextCheckpointExpected}</color>");
                RaceUI.Instance?.ShowMessage($"¡Sigue el orden! Checkpoint {nextCheckpointExpected} es el siguiente", 1.5f);
            }
            return;
        }

        // Marcar este checkpoint como completado
        checkpointsCompletedThisLap[index] = true;
        lastCheckpointReached = index;
        
        // Avanzar al siguiente checkpoint esperado
        nextCheckpointExpected = index + 1;
        
        // Si llegamos al final, el siguiente debe ser 0 (meta)
        if (nextCheckpointExpected >= totalCheckpoints)
        {
            nextCheckpointExpected = 0;
            Debug.Log($"<color=yellow>🏁 Checkpoint {index} ✓ ¡Listo para meta! 🏁</color>");
            RaceUI.Instance?.ShowMessage("¡Completaste todos los checkpoints! Ahora a la meta", 2f);
        }
        else
        {
            Debug.Log($"<color=green>✅ Checkpoint {index} ✓ — Siguiente: {nextCheckpointExpected}</color>");
        }
        
        // Mostrar progreso en UI (opcional)
        int completedCount = GetCompletedCheckpointsCount();
        RaceUI.Instance?.UpdateCheckpointProgress(completedCount, totalCheckpoints);
    }
    
    // NUEVO: Obtener cuántos checkpoints se han completado en la vuelta actual
    int GetCompletedCheckpointsCount()
    {
        int count = 0;
        for (int i = 0; i < totalCheckpoints; i++)
        {
            if (checkpointsCompletedThisLap[i])
                count++;
        }
        return count;
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

        foreach (var w in playerWheels) w.enabled = false;

        playerRigidbody.linearVelocity = Vector3.zero;
        playerRigidbody.angularVelocity = Vector3.zero;
        playerRigidbody.isKinematic = true;

        playerTransform.position = target.position;
        playerTransform.rotation = target.rotation;

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
    
    public bool IsRaceFinished() => raceFinished;
    public bool IsRaceStarted() => raceStarted;
    public int GetCurrentLap() => currentLap;
    public int GetNextCheckpoint() => nextCheckpointExpected;
}