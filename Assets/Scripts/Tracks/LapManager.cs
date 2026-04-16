using UnityEngine;
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
    public float wrongWayCheckDistance = 10f;
    public LayerMask trackLayer;

    private bool raceStarted = false;
    private bool raceFinished = false;
    private int currentLap = 0;
    private int nextCheckpointExpected = 0;
    private int lastCheckpointReached = -1;

    private bool[] checkpointsCompletedThisLap;

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

        CheckWrongWay();
    }

    void ResetCheckpointsProgress()
    {
        for (int i = 0; i < totalCheckpoints; i++)
        {
            checkpointsCompletedThisLap[i] = false;
        }

        RaceUI.Instance?.UpdateCheckpointProgress(0, totalCheckpoints);
    }

    void CheckWrongWay()
    {
        if (playerTransform == null || playerRigidbody == null || !raceStarted || raceFinished) return;

        float speed = playerRigidbody.linearVelocity.magnitude;
        if (speed < 2f)
        {
            if (isGoingWrongWay)
            {
                isGoingWrongWay = false;
                wrongWayTimer = 0f;
                RaceUI.Instance?.ShowWrongWayMessage(false);
            }
            return;
        }

        if (trackLayer.value != 0)
        {
            Physics.Raycast(
                playerTransform.position + Vector3.up * 0.5f,
                playerTransform.forward,
                out _,
                wrongWayCheckDistance,
                trackLayer
            );
        }

        float forwardDot = Vector3.Dot(playerRigidbody.linearVelocity.normalized, playerTransform.forward);
        bool newWrongWay = forwardDot < -0.3f;

        if (newWrongWay != isGoingWrongWay)
        {
            isGoingWrongWay = newWrongWay;

            if (isGoingWrongWay)
            {
                wrongWayTimer = 0f;
                Debug.Log("<color=red>⚠️ DIRECCIÓN CONTRARIA ⚠️</color>");
                RaceUI.Instance?.ShowWrongWayMessage(true);
            }
            else
            {
                RaceUI.Instance?.ShowWrongWayMessage(false);
            }
        }

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

    void ResetLapProgress()
    {
        nextCheckpointExpected = 0;
        lastCheckpointReached = -1;
        ResetCheckpointsProgress();
        RaceUI.Instance?.ShowMessage("¡Has perdido el progreso de la vuelta!", 2f);
    }

    public void OnFinishLineCrossed()
    {
        if (raceFinished) return;

        if (!raceStarted)
        {
            StartRace();
            return;
        }

        if (IsValidLapCompletion())
        {
            CompleteLap();
        }
        else
        {
            string missing = GetMissingCheckpointsString();
            Debug.Log($"<color=orange>❌ Vuelta inválida! Te faltan los checkpoints: {missing}</color>");
            RaceUI.Instance?.ShowMessage($"¡Completa todos los checkpoints primero! Faltan: {missing}", 2f);
        }
    }

    bool IsValidLapCompletion()
    {
        for (int i = 0; i < totalCheckpoints; i++)
        {
            if (!checkpointsCompletedThisLap[i])
                return false;
        }

        return nextCheckpointExpected == 0;
    }

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
        isGoingWrongWay = false;
        wrongWayTimer = 0f;
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

        lastCheckpointReached = -1;
        nextCheckpointExpected = 0;
        ResetCheckpointsProgress();

        Debug.Log($"<color=green>✅ Vuelta {currentLap - 1} completada en {FormatTime(finishedLapTime)} ✅</color>");

        if (currentLap > totalLaps)
        {
            FinishRace();
        }
    }

    public void OnCheckpointCrossed(int index)
    {
        if (!raceStarted || raceFinished) return;

        if (index != nextCheckpointExpected)
        {
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

        checkpointsCompletedThisLap[index] = true;
        lastCheckpointReached = index;

        nextCheckpointExpected = index + 1;
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

        int completedCount = GetCompletedCheckpointsCount();
        RaceUI.Instance?.UpdateCheckpointProgress(completedCount, totalCheckpoints);
    }

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

    void FinishRace()
    {
        raceFinished = true;
        isGoingWrongWay = false;
        RaceUI.Instance?.ShowWrongWayMessage(false);

        OnRaceFinished?.Invoke();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1f;

        RaceUI.Instance?.ShowFinishScreen(lapTimes, bestLapTime, totalTime, currentCarName, currentMapName);
        Debug.Log("<color=cyan>🏆 ¡CARRERA TERMINADA! 🏆</color>");
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
