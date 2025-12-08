using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private bool canMove = false; // SET INI FALSE UNTUK DIEM!
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float zigzagSpeed = 3f;
    [SerializeField] private float zigzagAmount = 5f;
    [SerializeField] private float changeDirectionInterval = 2f;

    [Header("Growth Settings")]
    [SerializeField] private float growthAmount = 0.15f;
    [SerializeField] private float maxSize = 3f;
    [SerializeField] private int hitsToDestroy = 3;

    [Header("Score Settings")]
    [SerializeField] private int scoreValue = 10;

    [Header("Explosion Settings")]
    [SerializeField] private GameObject explosionEffect;
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

        // PENTING: Setup rigidbody agar BENAR-BENAR diem
        rb.isKinematic = true; // Kinematic = tidak terpengaruh physics
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll; // Freeze semua movement dan rotation

        startPosition = transform.position;

        // Cari pesawat (untuk rotasi menghadap player)
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

        Debug.Log($"Enemy spawned (Stationary Mode). Needs {hitsToDestroy} hits to destroy.");
    }

    void FixedUpdate()
    {
        // SKIP SEMUA MOVEMENT JIKA canMove = false
        if (!canMove || playerTransform == null) return;

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

    private void OnTriggerEnter(Collider other)
    {
        // Deteksi Peluru
        if (other.CompareTag("Bullet"))
        {
            Debug.Log($"🎯 BULLET HIT ENEMY!");

            Destroy(other.gameObject);
            currentHits++;
            GrowEnemy();

            Debug.Log($"Enemy hit! {currentHits}/{hitsToDestroy}");

            // Jika sudah 3x kena, baru meledak dan tambah skor
            if (currentHits >= hitsToDestroy)
            {
                Explode();
            }
        }
    }

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
        Debug.Log($"💥 Enemy exploding! Adding {scoreValue} score.");

        // ===== TAMBAH SKOR (PAKAI VARIABLE) =====
        if (UIManager.instance != null)
        {
            Debug.Log($"✅ UIManager found! Adding score: {scoreValue}");
            UIManager.instance.AddScore(scoreValue);
        }
        else
        {
            Debug.LogError("❌ UIManager.instance is NULL! Score not added.");
        }

        // ===== SPAWN EXPLOSION EFFECT =====
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // ===== EXPLOSION FORCE KE OBJEK SEKITAR =====
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rbNearby = nearbyObject.GetComponent<Rigidbody>();
            if (rbNearby != null && rbNearby != rb)
            {
                rbNearby.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

        // ===== HANCURKAN ENEMY =====
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