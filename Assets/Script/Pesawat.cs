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
    
    private Rigidbody rb;
    private float lastShootTime = 0f;
    private PesawatShaderAnimation shaderAnimation;

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
        rb.constraints = RigidbodyConstraints.FreezePositionY;
        
        Cursor.lockState = CursorLockMode.None;
        
        if (bulletSpawnPoint == null)
            bulletSpawnPoint = transform;
        
        // Get shader animation component
        shaderAnimation = GetComponent<PesawatShaderAnimation>();
        if (shaderAnimation == null)
        {
            Debug.LogWarning("PesawatShaderAnimation component not found! Shader effects will be disabled.");
        }
    }

    void FixedUpdate()
    {
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
        
        // Update shader animation untuk roll effect
        if (shaderAnimation != null)
        {
            shaderAnimation.UpdateRollEffect(rollInput);
        }
        
        // Hitung rotasi sebagai Euler angles
        float rollDelta = rollInput * rollSpeed * Time.fixedDeltaTime;
        Vector3 currentEuler = transform.eulerAngles;
        
        // Apply roll (Z-axis rotation)
        currentEuler.z += rollDelta;
        
        // Apply back to transform
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

        // Trigger shader glow effect
        if (shaderAnimation != null)
        {
            shaderAnimation.TriggerShootGlow();
        }

        // Instantiate bullet di spawn point
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        
        if (bulletRb != null)
        {
            // Berikan velocity ke arah depan pesawat
            bulletRb.linearVelocity = transform.forward * bulletSpeed;
        }
        
        Debug.Log("Bullet fired!");
    }
}