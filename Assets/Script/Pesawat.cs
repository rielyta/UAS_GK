using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Pesawat : MonoBehaviour
{
    [Header("Movement")]
    public float kecepatan = 10f;
    public float rollSpeed = 90f;
    public float pitchSpeed = 90f;
    public float yawSpeed = 60f;
    public bool useGravity = false;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed = 20f;
    public float shootCooldown = 0.2f;

    [Header("Collision Response")]
    public float collisionKnockbackForce = 5f;

    Rigidbody rb;
    float lastShootTime = 0f;
    private bool isBeingKnockedBack = false;
    private float knockbackEndTime = 0f;

    // Manual rotation tracking
    private float currentPitch = 0f;
    private float currentRoll = 0f;
    private float currentYaw = 0f;

    // Manual position tracking
    private Vector3 currentPosition;
    private Vector3 currentVelocity;

    // Manual speed control
    private float currentSpeed = 0f;
    public float maxSpeed = 20f;
    public float acceleration = 5f;
    public float deceleration = 8f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on " + gameObject.name);
            return;
        }

        // Setup Rigidbody
        rb.useGravity = useGravity;
        rb.isKinematic = true; // Manual control sepenuhnya
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Initialize manual tracking
        currentPosition = transform.position;
        currentVelocity = Vector3.zero;
        currentPitch = 0f;
        currentRoll = 0f;
        currentYaw = 0f;

        Cursor.lockState = CursorLockMode.Locked; // Lock cursor ke tengah screen

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

        // Update rotasi
        HandlePitchInput();
        HandleRollInput();
        HandleYawInput();
        ApplyRotation();

        // Update posisi
        HandleMovement();
        ApplyPosition();
    }

    void Update()
    {
        HandleShooting();
        HandleCursorToggle();
    }

    void HandleCursorToggle()
    {
        // Tekan ESC untuk toggle cursor lock
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }
    }



    void HandlePitchInput()
    {
        // Pakai mouse Y-axis untuk pitch
        float mouseY = Mouse.current.delta.y.ReadValue();
        float pitchInput = -mouseY / 50f; // Negative biar natural (up = pitch up)

        // Hitung perubahan pitch
        float pitchDelta = pitchInput * pitchSpeed * Time.fixedDeltaTime;
        currentPitch += pitchDelta;

        // Clamp pitch agar tidak over-rotate
        currentPitch = ClampAngle(currentPitch, -89f, 89f);
    }

    void HandleRollInput()
    {
        float rollInput = 0f;

        // Q/E untuk roll
        if (Keyboard.current.qKey.isPressed)
            rollInput = 1f;
        if (Keyboard.current.eKey.isPressed)
            rollInput = -1f;

        // Hitung perubahan roll
        float rollDelta = rollInput * rollSpeed * Time.fixedDeltaTime;
        currentRoll += rollDelta;

        // Normalize roll angle
        currentRoll = NormalizeAngle(currentRoll);
    }

    void HandleYawInput()
    {
        // Pakai mouse X-axis untuk yaw
        float mouseX = Mouse.current.delta.x.ReadValue();
        float yawInput = mouseX / 50f;

        // Hitung perubahan yaw
        float yawDelta = yawInput * yawSpeed * Time.fixedDeltaTime;
        currentYaw += yawDelta;

        // Normalize yaw angle
        currentYaw = NormalizeAngle(currentYaw);
    }

    void ApplyRotation()
    {
        // Terapkan rotasi secara manual menggunakan Euler angles
        // Format: X = Pitch, Y = Yaw, Z = Roll
        Vector3 eulerAngles = new Vector3(currentPitch, currentYaw, currentRoll);
        transform.eulerAngles = eulerAngles;
    }

  
    void HandleMovement()
    {
        // Hitung input maju/mundur
        float thrustInput = 0f;
        
        if (Keyboard.current.wKey.isPressed)
            thrustInput = 1f;
        if (Keyboard.current.sKey.isPressed)
            thrustInput = -1f;

        // Hitung acceleration/deceleration
        if (Mathf.Abs(thrustInput) > 0.01f)
        {
            // Ada input, accelerate
            currentSpeed = Mathf.Lerp(currentSpeed, thrustInput * maxSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Tidak ada input, decelerate
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
        }

        // Hitung forward direction berdasarkan rotation
        Vector3 forwardDirection = CalculateForwardDirection();

        // Hitung velocity dengan speed yang sudah di-calculate
        currentVelocity = forwardDirection * currentSpeed;
    }

    Vector3 CalculateForwardDirection()
    {
        // Konversi Euler angles ke quaternion untuk mendapat forward direction
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, currentRoll);
        Vector3 forward = rotation * Vector3.forward;

        return forward.normalized;
    }

    void ApplyPosition()
    {
        // Update posisi secara manual
        currentPosition += currentVelocity * Time.fixedDeltaTime;

        // Terapkan posisi ke transform
        transform.position = currentPosition;

        // Update Rigidbody position (karena kinematic)
        rb.MovePosition(currentPosition);
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

        // Hitung posisi spawn bullet
        Vector3 spawnPosition = bulletSpawnPoint.position;

        // Hitung rotasi bullet (sama dengan pesawat)
        Quaternion spawnRotation = Quaternion.Euler(currentPitch, currentYaw, currentRoll);

        // Instantiate bullet
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, spawnRotation);
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

        if (bulletRb != null)
        {
            // Hitung forward direction dan berikan velocity
            Vector3 bulletForward = CalculateForwardDirection();
            bulletRb.linearVelocity = bulletForward * bulletSpeed;
        }

        Debug.Log("Bullet fired!");
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Hitung arah knockback (menjauhi enemy)
            Vector3 knockbackDirection = CalculateKnockbackDirection(collision.transform.position);

            // Terapkan knockback
            ApplyKnockback(knockbackDirection);

            // Set knockback state (0.2 detik)
            isBeingKnockedBack = true;
            knockbackEndTime = Time.time + 0.2f;

            Debug.Log("Pesawat hit enemy!");
        }
    }

    Vector3 CalculateKnockbackDirection(Vector3 enemyPosition)
    {
        // Hitung vector dari enemy ke pesawat
        Vector3 directionFromEnemy = currentPosition - enemyPosition;

        // Normalisasi dan jaga Y agar tidak terbang
        directionFromEnemy.y = 0;
        directionFromEnemy = directionFromEnemy.normalized;

        return directionFromEnemy;
    }

    void ApplyKnockback(Vector3 direction)
    {
        // Tambahkan force ke velocity saat ini
        Vector3 knockbackVelocity = direction * collisionKnockbackForce;
        currentVelocity += knockbackVelocity;
    }

    // ===== UTILITY FUNCTIONS (MANUAL) =====

    float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180f)
            angle -= 360f;

        if (angle < -180f)
            angle += 360f;

        // Clamp antara min dan max
        if (angle > max)
            angle = max;
        if (angle < min)
            angle = min;

        return angle;
    }

    float NormalizeAngle(float angle)
    {
        // Normalize angle ke range -180 sampai 180
        while (angle > 180f)
            angle -= 360f;

        while (angle < -180f)
            angle += 360f;

        return angle;
    }

    // Debug function untuk display info
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 240));
        GUILayout.Label("Pesawat Controller (Mouse Control)");
        GUILayout.Label($"Position: {currentPosition}");
        GUILayout.Label($"Speed: {currentSpeed:F2} / {maxSpeed}");
        GUILayout.Label($"Velocity Magnitude: {currentVelocity.magnitude:F2}");
        GUILayout.Label($"Pitch: {currentPitch:F2}°");
        GUILayout.Label($"Roll: {currentRoll:F2}°");
        GUILayout.Label($"Yaw: {currentYaw:F2}°");
        GUILayout.Label("");
        GUILayout.Label("Controls:");
        GUILayout.Label("W/S - Maju/Mundur");
        GUILayout.Label("Mouse Move - Pitch/Yaw");
        GUILayout.Label("Q/E - Roll");
        GUILayout.Label("Mouse Left Click - Shoot");
        GUILayout.Label("ESC - Unlock Cursor");
        GUILayout.EndArea();
    }
}