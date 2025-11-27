using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 360f; // derajat per detik
    public Vector3 rotationAxis = Vector3.up;
    
    [Header("Lifetime")]
    public float lifetime = 10f; // bullet hilang setelah 10 detik
    
    Rigidbody rb;
    float spawnTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on Bullet!");
            Destroy(gameObject);
            return;
        }
        
        spawnTime = Time.time;
        
        // Bullet tidak terpengaruh gravity
        rb.useGravity = false;
        
        // ===== PENTING: Tidak gunakan rb.constraints =====
        // Gunakan manual rotation instead
        rb.angularVelocity = Vector3.zero;
    }

    void FixedUpdate()
    {

        // Hitung rotation delta per frame
        float rotationDelta = rotationSpeed * Time.fixedDeltaTime;
        
        // Ambil current rotation sebagai Euler angles
        Vector3 currentEuler = transform.eulerAngles;
        
        // Apply rotation pada axis yang ditentukan
        // rotationAxis biasanya (0, 1, 0) untuk putar ke atas
        if (rotationAxis.x != 0) currentEuler.x += rotationDelta * rotationAxis.x;
        if (rotationAxis.y != 0) currentEuler.y += rotationDelta * rotationAxis.y;
        if (rotationAxis.z != 0) currentEuler.z += rotationDelta * rotationAxis.z;
        
        // Terapkan rotation ke transform (MANUAL)
        transform.eulerAngles = currentEuler;
        
        // Destroy setelah lifetime habis
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

}