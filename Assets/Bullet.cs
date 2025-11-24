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
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void FixedUpdate()
    {
        // Putar bullet
        transform.Rotate(rotationAxis * rotationSpeed * Time.fixedDeltaTime, Space.Self);
        
        // Destroy setelah lifetime habis
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        // Jika mengenai enemy, destroy bullet
        if (collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}