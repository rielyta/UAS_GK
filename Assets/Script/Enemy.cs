using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement Stats")]
    public float moveSpeed = 15f;
    public float turnSpeed = 2f;
    public float hoverFrequency = 2f;
    public float hoverAmplitude = 0.5f;

    [Header("Gameplay")]
    public int maxHealth = 3;
    public int scoreValue = 100;
    public GameObject explosionPrefab;

    [Header("Collision Settings")]
    public float damageToPlayer = 1;
    public bool destroyOnPlayerHit = true;

    private Transform playerTarget;
    private Rigidbody rb;
    private int currentHealth;
    private float randomHoverOffset;
    private bool hasHitPlayer = false; // Prevent multiple hits

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // IMPORTANT: Setup proper collision
        rb.useGravity = false;
        rb.isKinematic = false; // CHANGED: Must be false for collision detection
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        currentHealth = maxHealth;
        randomHoverOffset = Random.Range(0f, 10f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTarget = player.transform;

        // Tambahkan komponen visual builder jika belum ada
        if (GetComponent<EnemyVisualBuilder>() == null)
            gameObject.AddComponent<EnemyVisualBuilder>();

        // CRITICAL: Ensure enemy has proper collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // Add sphere collider if missing
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.radius = 1f;
            sphere.isTrigger = false; // MUST be false for collision
            Debug.Log("Added SphereCollider to Enemy");
        }
        else
        {
            col.isTrigger = false; // Make sure it's NOT a trigger
        }

        // Make sure enemy has the correct tag
        if (!gameObject.CompareTag("Enemy"))
        {
            gameObject.tag = "Enemy";
            Debug.Log("Set Enemy tag");
        }
    }

    void FixedUpdate()
    {
        if (playerTarget == null) return;

        // ROTASI: Menghadap ke Pemain
        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }

        // POSISI: Maju + Hover effect
        Vector3 forwardMovement = transform.forward * moveSpeed * Time.fixedDeltaTime;
        float hoverY = Mathf.Sin((Time.time + randomHoverOffset) * hoverFrequency) * hoverAmplitude * Time.fixedDeltaTime;
        Vector3 hoverMovement = Vector3.up * hoverY;

        rb.MovePosition(transform.position + forwardMovement + hoverMovement);
    }

    // === BULLET COLLISION (Trigger) ===
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("🎯 Enemy hit by bullet!");
            Destroy(other.gameObject);
            TakeDamage(1);
        }
    }

    // === PLAYER COLLISION (Non-Trigger) ===
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !hasHitPlayer)
        {
            hasHitPlayer = true;
            Debug.Log("💥 Enemy collided with Player!");

            // Damage player
            Pesawat playerScript = collision.gameObject.GetComponent<Pesawat>();
            if (playerScript != null)
            {
                playerScript.TakeDamage((int)damageToPlayer, transform.position);
            }

            // Destroy enemy if set
            if (destroyOnPlayerHit)
            {
                Explode();
            }
        }
    }

    void TakeDamage(int damage)
    {
        currentHealth -= damage;
        StartCoroutine(HitFlash());

        Debug.Log($"Enemy health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Explode();
        }
    }

    System.Collections.IEnumerator HitFlash()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.2f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
    }

    void Explode()
    {
        Debug.Log($"💥 Enemy exploded! Score: +{scoreValue}");

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