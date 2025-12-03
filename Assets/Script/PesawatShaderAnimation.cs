using UnityEngine;
using System.Collections;

public class PesawatShaderAnimation : MonoBehaviour
{
    private Material pesawatMaterial;
    
    [Header("Roll Effect")]
    public float maxMetallicFromRoll = 0.5f;
    
    [Header("Shoot Glow")]
    public float maxGlowIntensity = 10f;  // Increased dari 3 ke 10 untuk flash lebih terang
    public float glowFadeDuration = 0.3f;
    
    private Coroutine glowCoroutine;
    void Start()
{
    // Try get Renderer in same object
    Renderer renderer = GetComponent<Renderer>();

    // If not found, search children
    if (renderer == null)
        renderer = GetComponentInChildren<Renderer>();

    if (renderer != null)
    {
        pesawatMaterial = renderer.material;
        
        if (pesawatMaterial == null)
        {
            Debug.LogError("Material not found on renderer!");
            return;
        }

        if (!pesawatMaterial.HasProperty("_RollIntensity"))
            Debug.LogWarning("Shader doesn't have _RollIntensity property!");

        if (!pesawatMaterial.HasProperty("_ShootGlowIntensity"))
            Debug.LogWarning("Shader doesn't have _ShootGlowIntensity property!");
    }
    else
    {
        Debug.LogError("Renderer component not found in object or children!");
    }
}

    
    /// <summary>
    /// Update roll effect - mengubah metallic berdasarkan input roll
    /// </summary>
    /// <param name="rollInput">Nilai roll dari -1 (right) hingga 1 (left)</param>
    public void UpdateRollEffect(float rollInput)
    {
        if (pesawatMaterial == null) return;
        
        // Semakin besar absolute roll, semakin metallic
        float rollIntensity = Mathf.Abs(rollInput);
        pesawatMaterial.SetFloat("_RollIntensity", rollIntensity);
    }
    
    /// <summary>
    /// Trigger glow effect saat menembak
    /// </summary>
    public void TriggerShootGlow()
    {
        if (pesawatMaterial == null)
        {
            Debug.LogError("❌ Material NULL - Glow tidak bisa trigger!");
            return;
        }
        
        Debug.Log("💥 SHOOT GLOW TRIGGERED!");
        
        // Stop coroutine sebelumnya jika ada
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
        }
        
        // FORCE bright emission color (jangan hitam!)
        pesawatMaterial.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.6f)); // Bright orange-ish
        Debug.Log("🔆 Emission Color set to: Bright Orange");
        
        // Set glow intensity ke maksimum
        pesawatMaterial.SetFloat("_ShootGlowIntensity", maxGlowIntensity);
        Debug.Log($"✨ Glow Intensity set to: {maxGlowIntensity}");
        
        // Fade out glow
        glowCoroutine = StartCoroutine(FadeOutGlow());
    }
    
    private IEnumerator FadeOutGlow()
    {
        float elapsed = 0f;
        
        while (elapsed < glowFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / glowFadeDuration;
            float intensity = Mathf.Lerp(maxGlowIntensity, 0f, t);
            
            pesawatMaterial.SetFloat("_ShootGlowIntensity", intensity);
            yield return null;
        }
        
        // Ensure final value
        pesawatMaterial.SetFloat("_ShootGlowIntensity", 0f);
    }
    
    /// <summary>
    /// Set custom emission intensity (untuk efek lain)
    /// </summary>
    public void SetEmissiveIntensity(float intensity)
    {
        if (pesawatMaterial == null) return;
        
        pesawatMaterial.SetFloat("_EmissiveIntensity", Mathf.Clamp01(intensity));
    }
    
    /// <summary>
    /// Set base color (untuk perubahan warna dinamis)
    /// </summary>
    public void SetBaseColor(Color color)
    {
        if (pesawatMaterial == null) return;
        
        pesawatMaterial.SetColor("_BaseColor", color);
    }
}