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
        // Hanya logika putaran peluru
        float rotationDelta = rotationSpeed * Time.fixedDeltaTime;
        Vector3 currentEuler = transform.eulerAngles;

        if (rotationAxis.x != 0) currentEuler.x += rotationDelta * rotationAxis.x;
        if (rotationAxis.y != 0) currentEuler.y += rotationDelta * rotationAxis.y;
        if (rotationAxis.z != 0) currentEuler.z += rotationDelta * rotationAxis.z;

        transform.eulerAngles = currentEuler;

        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}