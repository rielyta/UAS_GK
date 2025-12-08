using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 360f; // derajat per detik
    public Vector3 rotationAxis = Vector3.forward; // Ubah ke forward agar muter di tempat

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

        // PENTING: Constraint rotasi agar tidak muter ke mana-mana
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;

        rb.angularVelocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        // Rotasi HANYA di axis Z (muter di tempat)
        // Menggunakan Rotate() lebih stable daripada manual euler
        transform.Rotate(rotationAxis, rotationSpeed * Time.fixedDeltaTime, Space.Self);

        // Check lifetime
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}