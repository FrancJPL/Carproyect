using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System;

public class AuthManager : MonoBehaviour
{
    [Header("Configuración Server")]
    public string loginUrl = "https://roman-hanky-automaker.ngrok-free.dev/login";

    [Header("UI Login")]
    public TMP_InputField inputUsername;
    public TMP_InputField inputPassword;
    public TMP_Text textoEstado;
    public GameObject panelLogin;
    public GameObject botonLoginAbrir; 
    public GameObject botonCerrarSesion; // ✅ Nuevo botón para cerrar sesión
    public TMP_Text textoUsuarioLogueado; 

    void Start()
    {
        // Forzamos que el password sea oculto al inicio
        if (inputPassword != null)
            inputPassword.contentType = TMP_InputField.ContentType.Password;

        ActualizarEstadoUI();
        if (panelLogin != null) panelLogin.SetActive(false);
    }

    public void AbrirPanelLogin()
    {
        if (panelLogin != null) panelLogin.SetActive(true);
        if (textoEstado != null) textoEstado.text = "Introduce tus credenciales";
        
        // Limpiamos campos al abrir
        if (inputUsername != null) inputUsername.text = "";
        if (inputPassword != null) inputPassword.text = "";
    }

    public void CerrarPanelLogin()
    {
        if (panelLogin != null) panelLogin.SetActive(false);
    }

    public void IntentarLogin()
    {
        string user = inputUsername.text.Trim();
        string pass = inputPassword.text.Trim();

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            if (textoEstado != null) textoEstado.text = "❌ Rellena todos los campos";
            return;
        }

        StartCoroutine(EnviarLogin(user, pass));
    }

    IEnumerator EnviarLogin(string user, string pass)
    {
        if (textoEstado != null) textoEstado.text = "⏳ Conectando...";

        LoginData data = new LoginData { username = user, password = pass };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest(loginUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Login exitoso");
                PlayerSession.LogIn(user);
                ActualizarEstadoUI();
                if (textoEstado != null) textoEstado.text = "✅ ¡Bienvenido!";
                
                yield return new WaitForSeconds(1.5f);
                CerrarPanelLogin();
            }
            else
            {
                Debug.LogError("❌ Error login: " + request.error);
                if (textoEstado != null) textoEstado.text = "❌ Error: Usuario o password incorrectos";
            }
        }
    }

    // ✅ Toggle para ver/ocultar contraseña
    public void ToggleVerPassword()
    {
        if (inputPassword == null) return;

        if (inputPassword.contentType == TMP_InputField.ContentType.Password)
            inputPassword.contentType = TMP_InputField.ContentType.Standard;
        else
            inputPassword.contentType = TMP_InputField.ContentType.Password;

        inputPassword.ForceLabelUpdate();
    }

    // ✅ Función para Cerrar Sesión
    public void CerrarSesion()
    {
        PlayerSession.LogOut();
        ActualizarEstadoUI();
    }

    void ActualizarEstadoUI()
    {
        bool logueado = PlayerSession.IsLoggedIn;

        if (logueado)
        {
            if (textoUsuarioLogueado != null) textoUsuarioLogueado.text = "🏁 " + PlayerSession.Username;
        }
        else
        {
            if (textoUsuarioLogueado != null) textoUsuarioLogueado.text = "Sin sesión";
        }

        if (botonLoginAbrir != null) botonLoginAbrir.SetActive(!logueado);
        if (botonCerrarSesion != null) botonCerrarSesion.SetActive(logueado);
    }

    [Serializable]
    public class LoginData
    {
        public string username;
        public string password;
    }
}
