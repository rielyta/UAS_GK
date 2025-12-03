using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float zigzagSpeed = 3f;
    [SerializeField] private float zigzagAmount = 5f;
    [SerializeField] private float changeDirectionInterval = 2f;

    [Header("Growth Settings")]
    [SerializeField] private float growthAmount = 0.15f;
    [SerializeField] private float maxSize = 3f;
    [SerializeField] private int hitsToDestroy = 3; // Jumlah hit untuk hancur

    [Header("Explosion Settings")]
    [SerializeField] private float explosionForce = 25f;
    [SerializeField] private float explosionRadius = 12f;

    private Transform playerTransform;
    private Vector3 startPosition;
    private float zigzagDirection = 1f;
    private float timeSinceDirectionChange = 0f;
    private Rigidbody rb;
    private int currentHits = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Enemy needs Rigidbody!");
            return;
        }

        startPosition = transform.position;

        // Cari pesawat
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure Player has tag 'Player'");
        }

        // Random arah zigzag awal
        zigzagDirection = Random.Range(0, 2) == 0 ? -1f : 1f;

        Debug.Log($"Enemy spawned. Needs {hitsToDestroy} hits to destroy.");
    }

    void FixedUpdate()
    {
        if (playerTransform == null) return;

        // ===== TRANSFORMASI 1: TRANSLASI MAJU (menuju pesawat) =====
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0;

        Vector3 forwardMovement = directionToPlayer * moveSpeed * Time.fixedDeltaTime;

        // ===== TRANSFORMASI 2: TRANSLASI ZIGZAG (kiri-kanan) =====
        timeSinceDirectionChange += Time.fixedDeltaTime;
        if (timeSinceDirectionChange >= changeDirectionInterval)
        {
            zigzagDirection *= -1f;
            timeSinceDirectionChange = 0f;
        }

        Vector3 rightVector = Vector3.Cross(directionToPlayer, Vector3.up).normalized;
        Vector3 zigzagMovement = rightVector * zigzagDirection * zigzagSpeed * Time.fixedDeltaTime;

        float distanceFromCenter = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(startPosition.x, 0, startPosition.z)
        );

        if (distanceFromCenter > zigzagAmount)
        {
            zigzagDirection *= -1f;
        }

        // Terapkan movement
        rb.MovePosition(transform.position + forwardMovement + zigzagMovement);

        startPosition = new Vector3(
            playerTransform.position.x + directionToPlayer.x * Vector3.Distance(transform.position, playerTransform.position),
            startPosition.y,
            playerTransform.position.z + directionToPlayer.z * Vector3.Distance(transform.position, playerTransform.position)
        );

        // ===== TRANSFORMASI 3: ROTASI (menghadap pesawat) =====
        if (directionToPlayer != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(directionToPlayer.x, directionToPlayer.z) * Mathf.Rad2Deg;
            Vector3 currentEuler = transform.eulerAngles;
            float newAngle = Mathf.LerpAngle(currentEuler.y, targetAngle, Time.fixedDeltaTime * 3f);
            currentEuler.y = newAngle;
            transform.eulerAngles = currentEuler;
        }
    }

    // ===== FIX: Deteksi bullet dengan OnTriggerEnter =====
    private void OnTriggerEnter(Collider other)
    {
        // Debug untuk cek collision
        Debug.Log($"Enemy triggered by: {other.gameObject.name} with tag: {other.tag}");

        if (other.CompareTag("Bullet"))
        {
            Debug.Log("Bullet hit enemy!");

            // Hancurkan bullet
            Destroy(other.gameObject);

            // Tambah hit counter
            currentHits++;
            Debug.Log($"Enemy hit {currentHits}/{hitsToDestroy} times");

            // Membesar
            GrowEnemy();

            // Cek apakah sudah cukup hit untuk hancur
            if (currentHits >= hitsToDestroy)
            {
                Debug.Log("Enemy destroyed by bullets!");
                Explode();
            }
        }
    }

    // ===== FIX: Deteksi nabrak pesawat =====
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Enemy collided with: {collision.gameObject.name} with tag: {collision.gameObject.tag}");

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Enemy collided with Player!");
            Explode();
        }
    }

    // ===== TRANSFORMASI 4: SCALE (membesar saat terkena bullet) =====
    private void GrowEnemy()
    {
        Vector3 currentScale = transform.localScale;
        currentScale += Vector3.one * growthAmount;
        transform.localScale = currentScale;

        Debug.Log($"Enemy grew to size: {currentScale.x}");

        // Cek jika sudah mencapai ukuran maksimal
        if (currentScale.x >= maxSize)
        {
            Debug.Log("Enemy reached max size!");
            Explode();
        }
    }

    private void Explode()
    {
        Debug.Log("Enemy exploding!");

        // Tambahkan explosion force ke objek sekitar
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rbNearby = nearbyObject.GetComponent<Rigidbody>();
            if (rbNearby != null && rbNearby != rb)
            {
                rbNearby.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, zigzagAmount);
    }
}