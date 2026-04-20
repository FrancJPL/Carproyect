using UnityEngine;

public class NitroPickup : MonoBehaviour
{
    [Header("Configuracion")]
    public float boostAmount = 25f;
    public float respawnTime = 10f;
    
    [Header("Efectos")]
    public GameObject pickupEffect;
    public AudioClip pickupSound;
    
    private MeshRenderer meshRenderer;
    private Collider itemCollider;
    private bool isCollected = false;
    private float respawnTimer = 0f;
    
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        itemCollider = GetComponent<Collider>();
        
        if (itemCollider != null)
        {
            itemCollider.isTrigger = true;
        }
        
        Debug.Log($"Nitro inicializado en {transform.name}");
    }
    
    void Update()
    {
        if (isCollected)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f)
            {
                Respawn();
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;
        
        Debug.Log($"Nitro: Colision con {other.gameObject.name}, Tag: {other.tag}");
        
        if (other.CompareTag("Player"))
        {
            BoostSystem boostSystem = null;
            
            // Buscar en el objeto que colisionó
            boostSystem = other.GetComponent<BoostSystem>();
            
            // Si no está, buscar en el padre
            if (boostSystem == null && other.transform.parent != null)
            {
                boostSystem = other.transform.parent.GetComponent<BoostSystem>();
            }
            
            // Si no está, buscar en toda la jerarquía del coche
            if (boostSystem == null)
            {
                boostSystem = other.GetComponentInParent<BoostSystem>();
            }
            
            // Si no está, buscar en el GameObject con tag Player (por si acaso)
            if (boostSystem == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    boostSystem = player.GetComponent<BoostSystem>();
                }
            }
            
            if (boostSystem != null)
            {
                if (boostSystem.GetCurrentBoost() < boostSystem.GetMaxBoost())
                {
                    Collect(boostSystem);
                }
                else
                {
                    Debug.Log("Boost lleno, no puedes recoger mas");
                    RaceUI.Instance?.ShowMessage("BOOST LLENO", 1f);
                }
            }
            else
            {
                Debug.LogError($"Nitro: No se encontro BoostSystem en el Player. El objeto que colisionó es: {other.gameObject.name}, su tag: {other.tag}");
                
                // Listar todos los componentes del objeto para debug
                Component[] components = other.GetComponents<Component>();
                Debug.Log($"Componentes en {other.gameObject.name}:");
                foreach (Component c in components)
                {
                    Debug.Log($"  - {c.GetType().Name}");
                }
            }
        }
    }
    
    void Collect(BoostSystem boostSystem)
    {
        isCollected = true;
        
        boostSystem.AddBoost(boostAmount);
        
        // Efectos visuales y sonido desactivados temporalmente
        // if (pickupEffect != null)
        // {
        //     GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
        //     Destroy(effect, 2f);
        // }
        
        // if (pickupSound != null)
        // {
        //     AudioSource.PlayClipAtPoint(pickupSound, transform.position, 1f);
        // }
        
        if (meshRenderer != null) meshRenderer.enabled = false;
        if (itemCollider != null) itemCollider.enabled = false;
        
        respawnTimer = respawnTime;
        
        Debug.Log($"Nitro recogido! +{boostAmount} boost");
    }
    
    void Respawn()
    {
        isCollected = false;
        respawnTimer = 0f;
        
        if (meshRenderer != null) meshRenderer.enabled = true;
        if (itemCollider != null) itemCollider.enabled = true;
        
        Debug.Log($"Nitro reaparecido en {transform.name}");
    }
    
    public void ForceRespawn()
    {
        if (isCollected)
        {
            isCollected = false;
            respawnTimer = 0f;
            if (meshRenderer != null) meshRenderer.enabled = true;
            if (itemCollider != null) itemCollider.enabled = true;
        }
    }
}