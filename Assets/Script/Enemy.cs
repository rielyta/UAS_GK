using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float turnSpeed = 4f;

    [Header("Scaling")]
    public float growthAmount = 0.1f;
    public float maxSize = 5f;

    [Header("Explosion")]
    public float explosionForce = 20f;
    public float explosionRadius = 10f;
    public GameObject explosionParticle;

    private Rigidbody rb;
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

        // Cari player
        playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("Player not found! Make sure Player has tag 'Player'");
        }
    }

    void FixedUpdate()
    {
        if (playerObject == null) return;

        // Hitung direction ke player
        Vector3 direction = (playerObject.transform.position - transform.position).normalized;

        // Gerak menuju player
        rb.linearVelocity = direction * moveSpeed;

        // Rotasi enemy menghadap player
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            GrowSize();
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            Explode(collision.gameObject);
        }
    }

    void GrowSize()
    {
        transform.localScale += Vector3.one * growthAmount;
        if (transform.localScale.magnitude >= maxSize * Mathf.Sqrt(3))
        {
            Explode(null);
        }
    }

    void Explode(GameObject target)
    {
        if (explosionParticle != null)
            Instantiate(explosionParticle, transform.position, Quaternion.identity);

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in colliders)
        {
            Rigidbody r = col.GetComponent<Rigidbody>();
            if (r != null && r != rb)
                r.AddExplosionForce(explosionForce, transform.position, explosionRadius);
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
