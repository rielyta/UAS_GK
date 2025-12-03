using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Pesawat : MonoBehaviour
{
    [Header("Movement")]
    public float kecepatan = 10f;
    public float rollSpeed = 90f;
    public bool useGravity = false;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed = 20f;
    public float shootCooldown = 0.2f;

    [Header("Collision Response")]
    public float collisionKnockbackForce = 5f; // Force saat nabrak

    Rigidbody rb;
    float lastShootTime = 0f;
    private bool isBeingKnockedBack = false;
    private float knockbackEndTime = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on " + gameObject.name);
            return;
        }

        rb.useGravity = useGravity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // PENTING: Lock rotation untuk mencegah rotasi saat nabrak
        rb.constraints = RigidbodyConstraints.FreezePositionY |
                        RigidbodyConstraints.FreezeRotationX |
                        RigidbodyConstraints.FreezeRotationZ;

        // Tambah mass supaya tidak mudah terlempar
        rb.mass = 5f;

        // Tambah drag supaya lebih stabil
        rb.linearDamping = 1f;
        rb.angularDamping = 5f;

        Cursor.lockState = CursorLockMode.None;

        if (bulletSpawnPoint == null)
            bulletSpawnPoint = transform;
    }

    void FixedUpdate()
    {
        // Jika sedang knockback, skip movement control
        if (isBeingKnockedBack && Time.time < knockbackEndTime)
        {
            return;
        }

        isBeingKnockedBack = false;
        HandleMovement();
        HandleRoll();
    }

    void Update()
    {
        HandleShooting();
    }

    void HandleMovement()
    {
        Vector3 dir = Vector3.zero;

        if (Keyboard.current.wKey.isPressed)
            dir += transform.forward;
        if (Keyboard.current.sKey.isPressed)
            dir -= transform.forward;
        if (Keyboard.current.aKey.isPressed)
            dir -= transform.right;
        if (Keyboard.current.dKey.isPressed)
            dir += transform.right;

        if (dir.magnitude > 0)
        {
            dir = dir.normalized * kecepatan;
        }

        rb.linearVelocity = new Vector3(dir.x, rb.linearVelocity.y, dir.z);
    }

    void HandleRoll()
    {
        float rollInput = 0f;
        if (Keyboard.current.qKey.isPressed)
            rollInput = 1f;
        if (Keyboard.current.eKey.isPressed)
            rollInput = -1f;

        // Hitung rotasi sebagai Euler angles
        float rollDelta = rollInput * rollSpeed * Time.fixedDeltaTime;

        // Ambil current rotation sebagai Euler angles
        Vector3 currentEuler = transform.eulerAngles;

        // Apply roll (Y-axis rotation untuk top-down view)
        currentEuler.y += rollDelta;

        // Terapkan kembali ke transform (MANUAL)
        transform.eulerAngles = currentEuler;
    }

    void HandleShooting()
    {
        // Cek jika mouse button kiri ditekan dan cooldown habis
        if (Mouse.current.leftButton.isPressed && Time.time >= lastShootTime + shootCooldown)
        {
            ShootBullet();
            lastShootTime = Time.time;
        }
    }

    void ShootBullet()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet Prefab not assigned!");
            return;
        }

        // Instantiate bullet di spawn point
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

        if (bulletRb != null)
        {
            // Berikan velocity ke arah depan pesawat
            bulletRb.linearVelocity = transform.forward * bulletSpeed;
        }

        Debug.Log("Bullet fired!");
    }

    // Handle collision dengan enemy
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Hitung arah knockback (menjauhi enemy)
            Vector3 knockbackDirection = (transform.position - collision.transform.position).normalized;
            knockbackDirection.y = 0; // Jaga agar tidak terbang

            // Terapkan knockback force (hanya bergetar sedikit)
            rb.AddForce(knockbackDirection * collisionKnockbackForce, ForceMode.Impulse);

            // Set knockback state (0.2 detik)
            isBeingKnockedBack = true;
            knockbackEndTime = Time.time + 0.2f;

            Debug.Log("Pesawat hit enemy!");
        }
    }
}