using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class TimeSaver : MonoBehaviour
{
    [Header("Configuración")]
    private string apiUrl = "https://roman-hanky-automaker.ngrok-free.dev/save_time";
    
    [Header("Referencias UI")]
    public GameObject panelFeedback; // El panel que muestra el "Guardando..."
    public TMP_Text textoEstado;         
    
    private string mejorVueltaActual;
    private string tiempoTotalActual;
    private string cocheActual;
    private string mapaActual;
    
    void Start()
    {
        if (panelFeedback != null)
            panelFeedback.SetActive(false);
    }
    
    public void AutoGuardarSiLogueado(string mejorVuelta, string tiempoTotal, string coche, string mapa)
    {
        if (PlayerSession.IsLoggedIn)
        {
            mejorVueltaActual = mejorVuelta;
            tiempoTotalActual = tiempoTotal;
            cocheActual = coche;
            mapaActual = mapa;
            
            Debug.Log("Iniciando guardado automático...");
            StartCoroutine(EnviarTiempoServidor(PlayerSession.Username));
        }
    }
    
    IEnumerator EnviarTiempoServidor(string nombre)
    {
        if (textoEstado != null)
        {
            panelFeedback?.SetActive(true); 
            textoEstado.text = "⏳ Guardando record automáticamente...";
        }
        
        DatosTiempo datos = new DatosTiempo();
        datos.name = nombre;
        datos.best_lap = mejorVueltaActual;
        datos.total_time = tiempoTotalActual;
        datos.car = cocheActual;
        datos.map = mapaActual;
        
        string jsonData = JsonUtility.ToJson(datos);
        
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                if (textoEstado != null)
                    textoEstado.text = "✅ ¡Record enviado al servidor!";
                
                yield return new WaitForSeconds(2f);
                panelFeedback?.SetActive(false);
            }
            else
            {
                if (textoEstado != null)
                    textoEstado.text = "❌ Error de conexión";
                
                yield return new WaitForSeconds(2f);
                panelFeedback?.SetActive(false);
            }
        }
    }

    [System.Serializable]
    public class DatosTiempo
    {
        public string name;
        public string best_lap;
        public string total_time;
        public string car;
        public string map;
    }
}