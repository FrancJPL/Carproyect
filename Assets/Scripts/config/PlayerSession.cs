using UnityEngine;

public static class PlayerSession
{
    public static string Username = "";
    public static bool IsLoggedIn = false;

    public static void LogIn(string username)
    {
        Username = username;
        IsLoggedIn = true;
        Debug.Log($"Sesión iniciada para: {Username}");
    }

    public static void LogOut()
    {
        Username = "";
        IsLoggedIn = false;
        Debug.Log("Sesión cerrada");
    }
}
