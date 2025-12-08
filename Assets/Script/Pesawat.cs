using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Pesawat : MonoBehaviour
{
    [Header("Status")]
    public int maxHealth = 3;
    private int currentHealth;

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
    public AudioClip shootSound;
    public int bulletsPerShot = 1;
    public float bulletSpread = 10f;

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

    void Start()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.useGravity = useGravity;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        currentPosition = transform.position;
        currentVelocity = Vector3.zero;
        currentPitch = 0f;
        currentRoll = 0f;
        currentYaw = 0f;

        Cursor.lockState = CursorLockMode.Locked;

        if (bulletSpawnPoint == null) bulletSpawnPoint = transform;
    }

    void FixedUpdate()
    {
        if (isBeingKnockedBack && Time.time < knockbackEndTime) return;

        isBeingKnockedBack = false;

        HandlePitchInput();
        HandleRollInput();
        HandleYawInput();
        ApplyRotation();

        HandleMovement();
        ApplyPosition();
    }

    void Update()
    {
        if (Time.timeScale > 0)
        {
            HandleShooting();
            HandleCursorToggle();
        }
    }

    void HandleCursorToggle()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    void HandlePitchInput()
    {
        float mouseY = Mouse.current.delta.y.ReadValue();
        float pitchInput = -mouseY / 50f;
        float pitchDelta = pitchInput * pitchSpeed * Time.fixedDeltaTime;
        currentPitch += pitchDelta;
        currentPitch = ClampAngle(currentPitch, -89f, 89f);
    }

    void HandleRollInput()
    {
        float rollInput = 0f;
        if (Keyboard.current.qKey.isPressed) rollInput = 1f;
        if (Keyboard.current.eKey.isPressed) rollInput = -1f;

        float rollDelta = rollInput * rollSpeed * Time.fixedDeltaTime;
        currentRoll += rollDelta;
        currentRoll = NormalizeAngle(currentRoll);
    }

    void HandleYawInput()
    {
        float mouseX = Mouse.current.delta.x.ReadValue();
        float yawInput = mouseX / 50f;
        float yawDelta = yawInput * yawSpeed * Time.fixedDeltaTime;
        currentYaw += yawDelta;
        currentYaw = NormalizeAngle(currentYaw);
    }

    void ApplyRotation()
    {
        Vector3 eulerAngles = new Vector3(currentPitch, currentYaw, currentRoll);
        transform.eulerAngles = eulerAngles;
    }

    void HandleMovement()
    {
        float thrustInput = 0f;
        if (Keyboard.current.wKey.isPressed) thrustInput = 1f;
        if (Keyboard.current.sKey.isPressed) thrustInput = -1f;

        if (Mathf.Abs(thrustInput) > 0.01f)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, thrustInput * maxSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
        }

        Vector3 forwardDirection = CalculateForwardDirection();
        currentVelocity = forwardDirection * currentSpeed;
    }

    Vector3 CalculateForwardDirection()
    {
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, currentRoll);
        return (rotation * Vector3.forward).normalized;
    }

    void ApplyPosition()
    {
        currentPosition += currentVelocity * Time.fixedDeltaTime;
        transform.position = currentPosition;
        rb.MovePosition(currentPosition);
    }

    void HandleShooting()
    {
        if (Mouse.current.leftButton.isPressed && Time.time >= lastShootTime + shootCooldown)
        {
            ShootBullet();
            lastShootTime = Time.time;
        }
    }

    void ShootBullet()
    {
        if (bulletPrefab == null || bulletSpawnPoint == null) return;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float angle = 0f;
            if (bulletsPerShot > 1)
            {
                angle = bulletSpread * (i - (bulletsPerShot - 1) / 2f);
            }

            Quaternion spreadRotation = Quaternion.Euler(0, angle, 0);
            Quaternion finalRotation = bulletSpawnPoint.rotation * spreadRotation;

            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, finalRotation);
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

            if (bulletRb != null)
            {
                Vector3 bulletForward = finalRotation * Vector3.forward;
                bulletRb.linearVelocity = bulletForward * bulletSpeed;
            }
        }

        // Efek Visual/Audio saat menembak
        PesawatShaderAnimation shaderAnim = GetComponent<PesawatShaderAnimation>();
        if (shaderAnim != null) shaderAnim.TriggerShootGlow();

        StartCoroutine(CameraShake(0.1f, 0.15f));

        if (bulletsPerShot > 1) Debug.Log($"Fired {bulletsPerShot} bullets!");
        else Debug.Log("Bullet fired!");
    }

    // --- FUNGSI INI SEHARUSNYA DI LUAR ShootBullet() ---
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            currentHealth--;
            Debug.Log($"Duar! Pesawat tertabrak! Sisa nyawa: {currentHealth}");

            if (currentHealth <= 0)
            {
                if (UIManager.instance != null)
                {
                    UIManager.instance.TriggerGameOver();
                    this.enabled = false; // Matikan kontrol pesawat
                }
            }
            else
            {
                Vector3 knockbackDirection = CalculateKnockbackDirection(collision.transform.position);
                ApplyKnockback(knockbackDirection);
                isBeingKnockedBack = true;
                knockbackEndTime = Time.time + 0.2f;
            }
        }
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        Vector3 originalPos = cam.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            cam.transform.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cam.transform.localPosition = originalPos;
    }

    Vector3 CalculateKnockbackDirection(Vector3 enemyPosition)
    {
        Vector3 directionFromEnemy = currentPosition - enemyPosition;
        directionFromEnemy.y = 0;
        return directionFromEnemy.normalized;
    }

    void ApplyKnockback(Vector3 direction)
    {
        Vector3 knockbackVelocity = direction * collisionKnockbackForce;
        currentVelocity += knockbackVelocity;
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        if (angle > max) angle = max;
        if (angle < min) angle = min;
        return angle;
    }

    float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 240));
        GUILayout.Label($"Health: {currentHealth}/{maxHealth}");
        GUILayout.Label($"Speed: {currentSpeed:F2}");
        GUILayout.EndArea();
    }
}