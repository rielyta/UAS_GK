using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 0.5f; // Sesuai Inspector
    public float changeDirectionInterval = 3f; // Sesuai Inspector

    [Header("Scaling")]
    public float growthAmount = 0.1f; // Sesuai Inspector
    public float maxSize = 5f; // Sesuai Inspector

    [Header("Explosion")]
    public float explosionForce = 20f; // Sesuai Inspector
    public float explosionRadius = 10f; // Sesuai Inspector
    public GameObject explosionParticle;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private float nextDirectionChange;
    private Vector3 initialScale;
    private GameObject playerObject;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on Enemy!");
            Destroy(gameObject);
            return;
        }

        initialScale = transform.localScale;
        nextDirectionChange = Time.time + changeDirectionInterval;

        // Cari player di scene
        playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning("Player not found! Enemy will move randomly.");
        }

        // Set arah ke player
        ChangeDirection();
    }

    void FixedUpdate()
    {
        // Gerakkan enemy
        rb.linearVelocity = moveDirection * moveSpeed;

        // Update arah ke player secara periodik
        if (Time.time >= nextDirectionChange)
        {
            ChangeDirection();
            nextDirectionChange = Time.time + changeDirectionInterval;
        }

        // Rotate enemy menghadap arah gerak (burung menghadap target)
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 2f);
        }
    }

    void ChangeDirection()
    {
        // SELALU bergerak ke arah player (3D - termasuk Y axis)
        if (playerObject != null)
        {
            Vector3 directionToPlayer = (playerObject.transform.position - transform.position).normalized;
            // PERBAIKAN: Jangan lock Y axis, biar burung bisa terbang naik/turun
            moveDirection = directionToPlayer;

            Debug.Log("Enemy moving towards player in 3D");
        }
        else
        {
            // Fallback: random direction jika player tidak ada
            float randomX = Random.Range(-1f, 1f);
            float randomY = Random.Range(-0.3f, 0.3f); // Sedikit variasi Y
            float randomZ = Random.Range(-1f, 1f);
            moveDirection = new Vector3(randomX, randomY, randomZ).normalized;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Jika kena bullet
        if (collision.gameObject.CompareTag("Bullet"))
        {
            GrowSize();
            Destroy(collision.gameObject); // Hancurkan bullet
        }

        // Jika nabrak player
        if (collision.gameObject.CompareTag("Player"))
        {
            Explode(collision.gameObject);
        }
    }

    void GrowSize()
    {
        // Tambah ukuran enemy
        transform.localScale += Vector3.one * growthAmount;

        Debug.Log($"Enemy hit! New size: {transform.localScale.magnitude}");

        // Jika sudah maksimal, meledak
        if (transform.localScale.magnitude >= maxSize * Mathf.Sqrt(3))
        {
            Explode(null);
        }
    }

    void Explode(GameObject target)
    {
        Debug.Log("Enemy exploded!");

        // Spawn particle effect (opsional)
        if (explosionParticle != null)
        {
            Instantiate(explosionParticle, transform.position, Quaternion.identity);
        }

        // Apply force ke nearby objects
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in colliders)
        {
            Rigidbody nearbyRb = col.GetComponent<Rigidbody>();
            if (nearbyRb != null && nearbyRb != rb)
            {
                nearbyRb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

        // Jika nabrak player, beri efek ke player
        if (target != null && target.CompareTag("Player"))
        {
            Debug.Log("Player hit by enemy explosion!");
            // Bisa tambah logic damage player di sini
        }

        // Hancurkan enemy
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // Visualisasi explosion radius di editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}