using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float changeDirectionInterval = 3f; // ganti arah setiap 3 detik
    
    [Header("Growth")]
    public float growthAmount = 0.1f; // berapa besar saat tertabrak
    public float maxSize = 5f; // ukuran maksimal
    
    [Header("Explosion")]
    public float explosionForce = 20f;
    public float explosionRadius = 10f;
    public ParticleSystem explosionParticle;
    
    Rigidbody rb;
    Vector3 moveDirection;
    float changeDirectionTimer = 0f;
    Vector3 initialScale;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on Enemy!");
            Destroy(gameObject);
            return;
        }
        
        rb.useGravity = true;
        rb.linearDamping = 0.5f;
        
        initialScale = transform.localScale;
        
        // Set arah gerak acak awal
        ChangeDirection();
    }

    void FixedUpdate()
    {
        // Gerak horizontal
        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
        
        // Timer ganti arah
        changeDirectionTimer += Time.fixedDeltaTime;
        if (changeDirectionTimer >= changeDirectionInterval)
        {
            ChangeDirection();
            changeDirectionTimer = 0f;
        }
    }

    void ChangeDirection()
    {
        // Arah acak horizontal
        float randomAngle = Random.Range(0f, 360f);
        moveDirection = new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), 0, Mathf.Sin(randomAngle * Mathf.Deg2Rad));
    }

    void OnCollisionEnter(Collision collision)
    {
        // Jika tertabrak bullet
        if (collision.gameObject.CompareTag("Bullet"))
        {
            GrowUp();
            Destroy(collision.gameObject); // destroy bullet
        }
        
        // Jika tertabrak pesawat, meledak
        if (collision.gameObject.CompareTag("Player"))
        {
            Explode();
        }
    }

    void GrowUp()
    {
        Vector3 newScale = transform.localScale + Vector3.one * growthAmount;
        
        // Batasi ukuran maksimal
        if (newScale.x < maxSize)
        {
            transform.localScale = newScale;
        }
        else
        {
            // Jika sudah max, langsung meledak
            Explode();
        }
    }

    void Explode()
    {
        Debug.Log("Enemy exploded!");
        
        // Spawn particle effect
        if (explosionParticle != null)
        {
            Instantiate(explosionParticle, transform.position, Quaternion.identity);
        }
        
        // Explosion force ke arah acak
        Vector3 explosionDirection = Random.onUnitSphere;
        
        // Aplly force ke nearby objects
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in nearbyColliders)
        {
            Rigidbody targetRb = col.GetComponent<Rigidbody>();
            if (targetRb != null && targetRb != rb)
            {
                targetRb.AddForce(explosionDirection * explosionForce, ForceMode.Impulse);
            }
        }
        
        // Destroy musuh
        Destroy(gameObject);
    }
}