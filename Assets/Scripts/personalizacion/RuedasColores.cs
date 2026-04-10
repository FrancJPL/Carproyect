using UnityEngine;

[ExecuteAlways]
public class RuedasColores : MonoBehaviour
{
    [Header("Ruedas (cada una con 1 slot de material)")]
    public Renderer[] renderersRuedas;

    [Header("Paleta (índice)")]
    public Material AFRC_Mat_Col1;            // Por defecto
    public Material AFRC_Env_Mat;             // España
    public Material AtomBallAtomSphere;       // Verde
    public Material SpatialMappingWireframe;  // Especial
    public Material TireClean;                // Negro

    public bool usarPlayerPrefs = true;

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            NormalizarRuedasShared();
            AplicarShared(AFRC_Mat_Col1);
            return;
        }

        if (usarPlayerPrefs)
        {
            int idx = PlayerPrefs.GetInt("materialRuedasElegido", 0);
            AplicarRuntime(ObtenerMaterialPorIndice(idx));
        }
    }

    // Botones
    public void AplicarPorDefecto() { Cambiar(0); }
    public void AplicarEspaña()     { Cambiar(1); }
    public void AplicarVerde()      { Cambiar(2); }
    public void AplicarEspecial()   { Cambiar(3); }
    public void AplicarNegro()      { Cambiar(4); }

    private void Cambiar(int indice)
    {
        var mat = ObtenerMaterialPorIndice(indice);
        AplicarRuntime(mat);
        if (Application.isPlaying && usarPlayerPrefs)
        {
            PlayerPrefs.SetInt("materialRuedasElegido", indice);
            PlayerPrefs.Save();
        }
    }

    private Material ObtenerMaterialPorIndice(int i)
    {
        switch (i)
        {
            case 0: return AFRC_Mat_Col1;
            case 1: return AFRC_Env_Mat;
            case 2: return AtomBallAtomSphere;
            case 3: return SpatialMappingWireframe;
            case 4: return TireClean;
            default: return AFRC_Mat_Col1;
        }
    }

    public Material GetMaterialByIndex(int i)
    {
        return ObtenerMaterialPorIndice(i);
    }

    private void NormalizarRuedasShared()
    {
        foreach (var r in renderersRuedas)
        {
            if (r == null) continue;
            var shared = r.sharedMaterials;
            if (shared == null || shared.Length != 1)
            {
                var baseMat = AFRC_Mat_Col1 != null ? AFRC_Mat_Col1 : (shared != null && shared.Length > 0 ? shared[0] : null);
                r.sharedMaterials = new Material[] { baseMat };
            }
        }
    }

    private void AplicarShared(Material mat)
    {
        if (mat == null) return;
        foreach (var r in renderersRuedas)
        {
            if (r == null) continue;
            r.sharedMaterials = new Material[] { mat };
        }
    }

    public void AplicarRuntime(Material mat)
    {
        if (mat == null) return;
        foreach (var r in renderersRuedas)
        {
            if (r == null) continue;
            var mats = r.materials;
            if (mats == null || mats.Length == 0) mats = new Material[] { mat };
            else mats[0] = mat;
            r.materials = mats;
        }
        Debug.Log($"[RuedasColores] Aplicado runtime: {mat.name}");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            NormalizarRuedasShared();
            if (AFRC_Mat_Col1 != null) AplicarShared(AFRC_Mat_Col1);
        }
    }
#endif
}
