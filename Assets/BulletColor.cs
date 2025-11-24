using UnityEngine;

public class BulletColor : MonoBehaviour
{
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        
        // Ubah warna ke merah
        renderer.material.color = new Color(1, 0, 0, 1);
        
        // Ubah metallic
        renderer.material.SetFloat("_Metallic", 0.8f);
        
        // Ubah smoothness
        renderer.material.SetFloat("_Smoothness", 0.5f);
    }
}