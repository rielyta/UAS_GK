using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private bool canMove = false;
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float zigzagSpeed = 3f;
    [SerializeField] private float zigzagAmount = 5f;
    [SerializeField] private float changeDirectionInterval = 2f;

    [Header("Growth Settings")]
    [SerializeField] private float growthAmount = 0.15f;
    [SerializeField] private float maxSize = 3f;
    [SerializeField] private int hitsToDestroy = 3;

    [Header("Score Settings")]
    [SerializeField] private int scoreValue = 5; 

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
        if (rb == null) return;

        rb.isKinematic = true;
        rb.useGravity = false;

        startPosition = transform.position;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        zigzagDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
    }

    void FixedUpdate()
    {
        if (!canMove || playerTransform == null) return;

        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0;
        Vector3 forwardMovement = directionToPlayer * moveSpeed * Time.fixedDeltaTime;

        timeSinceDirectionChange += Time.fixedDeltaTime;
        if (timeSinceDirectionChange >= changeDirectionInterval)
        {
            zigzagDirection *= -1f;
            timeSinceDirectionChange = 0f;
        }

        Vector3 rightVector = Vector3.Cross(directionToPlayer, Vector3.up).normalized;
        Vector3 zigzagMovement = rightVector * zigzagDirection * zigzagSpeed * Time.fixedDeltaTime;

        rb.MovePosition(transform.position + forwardMovement + zigzagMovement);

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 3f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);
            currentHits++;
            GrowEnemy();

            if (currentHits >= hitsToDestroy)
            {
                Explode();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) Explode();
    }

    private void GrowEnemy()
    {
        transform.localScale += Vector3.one * growthAmount;
        if (transform.localScale.x >= maxSize) Explode();
    }

    private void Explode()
    {
        if (UIManager.instance != null)
        {
            // Tambahkan skor sebesar scoreValue (misal 10)
            UIManager.instance.AddScore(scoreValue);
            Debug.Log("Score Added!");
        }
        else
        {
            Debug.LogError("UIManager.instance is NULL! Pastikan ada GameManager di Scene.");
        }

        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}