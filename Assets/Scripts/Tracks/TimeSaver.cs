using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro; // ✅ Importante para TextMeshPro

public class TimeSaver : MonoBehaviour
{
    [Header("Configuración")]
    private string apiUrl = "https://roman-hanky-automaker.ngrok-free.dev/save_time";
    
    [Header("Referencias UI")]
    public GameObject panelNombre;
    public TMP_InputField inputNombre;   // ✅ Antes InputField
    public TMP_Text textoEstado;         // ✅ Antes Text
    
    // Variables para almacenar los tiempos actuales
    private string mejorVueltaActual;
    private string tiempoTotalActual;
    private string cocheActual;
    private string mapaActual;
    
    void Start()
    {
        if (panelNombre != null)
            panelNombre.SetActive(false);
    }
    
    public void AbrirPanelGuardar(string mejorVuelta, string tiempoTotal, string coche, string mapa)
    {
        mejorVueltaActual = mejorVuelta;
        tiempoTotalActual = tiempoTotal;
        cocheActual = coche;
        mapaActual = mapa;
        
        if (inputNombre != null)
            inputNombre.text = "";
        
        if (panelNombre != null)
            panelNombre.SetActive(true);
            
        if (textoEstado != null)
            textoEstado.text = "Escribe tu nombre y presiona Guardar";
    }
    
    public void GuardarTiempo()
    {
        string nombre = inputNombre.text.Trim();
        
        if (string.IsNullOrEmpty(nombre))
        {
            if (textoEstado != null)
                textoEstado.text = "❌ Por favor escribe un nombre";
            return;
        }
        
        StartCoroutine(EnviarTiempoServidor(nombre));
    }
    
    public void CancelarGuardado()
    {
        panelNombre.SetActive(false);
    }
    
    IEnumerator EnviarTiempoServidor(string nombre)
    {
        if (textoEstado != null)
            textoEstado.text = "⏳ Guardando tiempos...";
        
        DatosTiempo datos = new DatosTiempo();
        datos.name = nombre;
        datos.best_lap = mejorVueltaActual;
        datos.total_time = tiempoTotalActual;
        datos.car = cocheActual;
        datos.map = mapaActual;
        
        string jsonData = JsonUtility.ToJson(datos);
        Debug.Log("Enviando: " + jsonData);
        
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Tiempo guardado: " + request.downloadHandler.text);
                if (textoEstado != null)
                    textoEstado.text = "✅ ¡Tiempos guardados correctamente!";
                
                yield return new WaitForSeconds(1f);
                panelNombre.SetActive(false);
            }
            else
            {
                Debug.LogError("❌ Error: " + request.error);
                if (textoEstado != null)
                    textoEstado.text = "❌ Error al guardar. ¿El servidor está activo?";
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

    public void GuardarTiempoDirecto(string nombre, string mejorVuelta, string tiempoTotal, string coche, string mapa)
    {
        StartCoroutine(EnviarTiempoDirecto(nombre, mejorVuelta, tiempoTotal, coche, mapa));
    }

    IEnumerator EnviarTiempoDirecto(string nombre, string mejorVuelta, string tiempoTotal, string coche, string mapa)
    {
        DatosTiempo datos = new DatosTiempo();
        datos.name = nombre;
        datos.best_lap = mejorVuelta;
        datos.total_time = tiempoTotal;
        datos.car = coche;
        datos.map = mapa;
        
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
                Debug.Log($"✅ Tiempo guardado para {nombre}");
                if (textoEstado != null)
                    textoEstado.text = "✅ ¡Guardado!";
            }
            else
            {
                Debug.LogError($"❌ Error: {request.error}");
                if (textoEstado != null)
                    textoEstado.text = "❌ Error al guardar";
            }
        }
    }

}