using UnityEngine;

public class BulletBoundary : MonoBehaviour
{
    [Header("Boundary Settings")]
    public float maxDistanceFromSpawn = 100f; // Jarak maksimal dari spawn point

    private Vector3 spawnPosition;

    void Start()
    {
        // Simpan posisi spawn
        spawnPosition = transform.position;
    }

    void Update()
    {
        // Hitung jarak dari spawn point
        float distance = Vector3.Distance(transform.position, spawnPosition);

        // Jika melebihi batas, hancurkan bullet
        if (distance > maxDistanceFromSpawn)
        {
            Debug.Log($"Bullet destroyed - exceeded boundary ({distance:F1}m from spawn)");
            Destroy(gameObject);
        }
    }
}