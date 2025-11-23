using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Pesawat : MonoBehaviour
{
    public float kecepatan = 10f;
    public float rollSpeed = 90f;
    public bool useGravity = false;
    
    Rigidbody rb;

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
        
        // Unlock cursor (tidak perlu lock lagi)
        Cursor.lockState = CursorLockMode.None;
    }

    void FixedUpdate()
    {
        // ===== INPUT KEYBOARD (WASD) =====
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
        
        // ===== INPUT ROLL (Q/E) - MOUSE DISABLED =====
        float rollInput = 0f;
        if (Keyboard.current.qKey.isPressed)
            rollInput = 1f;
        if (Keyboard.current.eKey.isPressed)
            rollInput = -1f;
        
        // Hitung rotasi - HANYA ROLL
        Quaternion deltaRot = Quaternion.Euler(
            0,  // pitch OFF
            0,  // yaw OFF
            rollInput * rollSpeed * Time.fixedDeltaTime
        );
        
        rb.MoveRotation(rb.rotation * deltaRot);
    }
}