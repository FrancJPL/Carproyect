using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoostSystem : MonoBehaviour
{
    [Header("Configuracion Boost")]
    public float maxBoost = 100f;
    public float currentBoost = 0f;
    public float boostConsumptionRate = 50f;
    public float boostForce = 2000f;
    public float minSpeedForBoost = 5f;
    
    [Header("UI")]
    public Image boostBarFill;
    public TextMeshProUGUI boostPercentageText;
    public GameObject boostParticles; // Efecto visual cuando boostea
    
    private CarController carController;
    private Rigidbody rb;
    private bool isBoosting = false;
    private ParticleSystem boostParticleSystem;
    
    void Start()
    {
        carController = GetComponent<CarController>();
        rb = GetComponent<Rigidbody>();
        currentBoost = 0f;
        
        // Buscar UI si no esta asignada
        if (boostBarFill == null || boostPercentageText == null)
        {
            FindBoostUI();
        }
        
        UpdateBoostUI();
        
        // Configurar particulas
        if (boostParticles != null)
        {
            boostParticleSystem = boostParticles.GetComponent<ParticleSystem>();
            boostParticles.SetActive(false);
        }
        
        Debug.Log($"BoostSystem inicializado. Boost actual: {currentBoost}/{maxBoost}");
    }
    
    void FindBoostUI()
    {
        RaceUI raceUI = FindObjectOfType<RaceUI>();
        if (raceUI != null)
        {
            if (boostBarFill == null)
                boostBarFill = raceUI.boostBarFill;
            if (boostPercentageText == null)
                boostPercentageText = raceUI.boostPercentageText;
            Debug.Log("Boost UI encontrada en RaceUI");
        }
    }
    
    void Update()
    {
        // Detectar Shift izquierdo
        bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        
        if (shiftPressed && currentBoost > 0 && !isBoosting)
        {
            StartBoost();
        }
        
        if (isBoosting)
        {
            currentBoost -= boostConsumptionRate * Time.deltaTime;
            UpdateBoostUI();
            
            if (currentBoost <= 0)
            {
                StopBoost();
            }
        }
        
        // Si suelta Shift, dejar de boostear
        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
        {
            StopBoost();
        }
        
        // Debug para probar (opcional: presionar B para añadir boost)
        if (Input.GetKeyDown(KeyCode.B))
        {
            AddBoost(50f);
            Debug.Log("Boost añadido manualmente con tecla B");
        }
    }
    
    void FixedUpdate()
    {
        if (isBoosting && rb != null && rb.linearVelocity.magnitude >= minSpeedForBoost)
        {
            Vector3 boostDir = transform.forward;
            rb.AddForce(boostDir * boostForce, ForceMode.Acceleration);
        }
    }
    
    void StartBoost()
    {
        isBoosting = true;
        
        // Activar efecto de particulas
        if (boostParticles != null)
        {
            boostParticles.SetActive(true);
            if (boostParticleSystem != null)
                boostParticleSystem.Play();
        }
        
        Debug.Log($"Boost activado! Boost restante: {currentBoost}/{maxBoost}");
    }
    
    void StopBoost()
    {
        isBoosting = false;
        
        // Desactivar efecto de particulas
        if (boostParticles != null)
        {
            if (boostParticleSystem != null)
                boostParticleSystem.Stop();
            boostParticles.SetActive(false);
        }
        
        Debug.Log("Boost desactivado");
    }
    
    public void AddBoost(float amount)
    {
        float previousBoost = currentBoost;
        currentBoost = Mathf.Min(currentBoost + amount, maxBoost);
        UpdateBoostUI();
        
        float actualAdded = currentBoost - previousBoost;
        Debug.Log($"Boost ganado: +{actualAdded:F1}, total: {currentBoost:F1}/{maxBoost}");
        
        // Mostrar mensaje en UI
        RaceUI.Instance?.ShowMessage($"+{actualAdded:F0} BOOST", 1f);
    }
    
    void UpdateBoostUI()
    {
        float fillAmount = currentBoost / maxBoost;
        
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
    
    public bool IsBoosting() => isBoosting;
    public float GetBoostPercentage() => currentBoost / maxBoost;
    public float GetCurrentBoost() => currentBoost;
    public float GetMaxBoost() => maxBoost;
}