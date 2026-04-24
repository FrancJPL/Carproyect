using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SelectorCoches : MonoBehaviour
{
    [Header("Botones UI")]
    public Button botonIzquierda;
    public Button botonDerecha;
    
    [Header("Tus 4 coches")]
    public GameObject coche1;
    public GameObject coche2;
    public GameObject coche3;
    public GameObject coche4;
    
    private int cocheActual = 1;
    
    void Start()
    {
        MostrarCoche(1);
        
        if (botonIzquierda != null)
            botonIzquierda.onClick.AddListener(CocheAnterior);
        if (botonDerecha != null)
            botonDerecha.onClick.AddListener(CocheSiguiente);
    }
    
    void MostrarCoche(int cual)
    {
        if (coche1 != null) coche1.SetActive(false);
        if (coche2 != null) coche2.SetActive(false);
        if (coche3 != null) coche3.SetActive(false);
        if (coche4 != null) coche4.SetActive(false);
        
        switch (cual)
        {
            case 1: if (coche1 != null) coche1.SetActive(true); break;
            case 2: if (coche2 != null) coche2.SetActive(true); break;
            case 3: if (coche3 != null) coche3.SetActive(true); break;
            case 4: if (coche4 != null) coche4.SetActive(true); break;
        }
        
        Debug.Log("Coche actual: " + cual);
    }
    
    public void CocheSiguiente()
    {
        cocheActual++;
        if (cocheActual > 4) cocheActual = 1;
        MostrarCoche(cocheActual);
    }
    
    public void CocheAnterior()
    {
        cocheActual--;
        if (cocheActual < 1) cocheActual = 4;
        MostrarCoche(cocheActual);
    }
}