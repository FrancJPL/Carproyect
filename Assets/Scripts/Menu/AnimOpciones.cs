using UnityEngine;

public class AnimOpciones : MonoBehaviour
{
    public GameObject panelOpciones;
    public RectTransform ventana;
    public CanvasGroup canvasGroup;

    private bool abierto = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (abierto)
                CerrarOpciones();
            else
                AbrirOpciones();
        }
    }

    public void AbrirOpciones()
    {
    panelOpciones.SetActive(true);
    abierto = true;

    ventana.localScale = Vector3.one * 0.7f; // empieza más pequeño
    canvasGroup.alpha = 0;

    LeanTween.scale(ventana, Vector3.one, 0.55f).setEaseOutBack(); 
    LeanTween.alphaCanvas(canvasGroup, 1, 0.55f);
    }


    public void CerrarOpciones()
    {
    abierto = false;

    LeanTween.scale(ventana, Vector3.one * 0.7f, 0.35f).setEaseInBack();
    LeanTween.alphaCanvas(canvasGroup, 0, 0.35f)
        .setOnComplete(() => panelOpciones.SetActive(false));
    }

}
