using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement Stats")]
    public float moveSpeed = 15f;
    public float turnSpeed = 2f;      // Kecepatan belok
    public float hoverFrequency = 2f; // Kecepatan naik turun
    public float hoverAmplitude = 0.5f; // Jarak naik turun

    [Header("Gameplay")]
    public int maxHealth = 3;
    public int scoreValue = 100;
    public GameObject explosionPrefab;

    private Transform playerTarget;
    private Rigidbody rb;
    private int currentHealth;

    // Variabel untuk efek visual
    private float randomHoverOffset;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true; // Kita gerakkan manual via MovePosition

        currentHealth = maxHealth;

        // Agar gerakan naik turun tiap musuh tidak serentak
        randomHoverOffset = Random.Range(0f, 10f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTarget = player.transform;

        // Tambahkan komponen visual builder jika belum ada
        if (GetComponent<EnemyVisualBuilder>() == null)
            gameObject.AddComponent<EnemyVisualBuilder>();
    }

    void FixedUpdate()
    {
        if (playerTarget == null) return;

        // 1. ROTASI: Menghadap ke Pemain secara halus
        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            // Slerp membuat rotasi halus, tidak scapping
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }

        // 2. POSISI: Maju ke depan (berdasarkan arah hadap sekarang) + Efek Hover
        Vector3 forwardMovement = transform.forward * moveSpeed * Time.fixedDeltaTime;

        // Hitung efek naik turun (Sinusoidal wave)
        float hoverY = Mathf.Sin((Time.time + randomHoverOffset) * hoverFrequency) * hoverAmplitude * Time.fixedDeltaTime;
        Vector3 hoverMovement = Vector3.up * hoverY;

        // Terapkan gerakan
        rb.MovePosition(transform.position + forwardMovement + hoverMovement);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Destroy(other.gameObject); // Hancurkan peluru
            TakeDamage(1);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Explode(); // Tabrakan dengan pemain langsung meledak
        }
    }

    void TakeDamage(int damage)
    {
        currentHealth -= damage;
        // Efek visual kena hit (sedikit membesar sebentar lalu kembali)
        StartCoroutine(HitFlash());

        if (currentHealth <= 0)
        {
            Explode();
        }
    }

    System.Collections.IEnumerator HitFlash()
    {
        // Simple scale effect
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.2f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
    }

    void Explode()
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.AddScore(scoreValue);
        }

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}