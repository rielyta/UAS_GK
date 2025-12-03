using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public int initialEnemyCount = 5;
    public float spawnInterval = 3f;

    [Header("Spawn Area - Relative to Player")]
    public float spawnDistanceAhead = 50f; // Jarak di depan player
    public float spawnDistanceRange = 20f; // Random offset kiri/kanan
    public float spawnHeight = 2f;

    private float nextSpawnTime;
    private GameObject player;

    void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab not assigned to EnemySpawner!");
            return;
        }

        // Cari player
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure Player has 'Player' tag.");
            return;
        }

        // Spawn musuh awal
        for (int i = 0; i < initialEnemyCount; i++)
        {
            SpawnEnemy();
        }

        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        // Spawn musuh baru secara periodik
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

void SpawnEnemy()
{
    if (player == null) return;

    // Referensi kamera utama
    Camera cam = Camera.main;
    if (cam == null)
    {
        Debug.LogError("Main Camera not found!");
        return;
    }

    // Arah depan kamera
    Vector3 forward = cam.transform.forward;

    // Posisi dasar tepat di depan kamera
    Vector3 spawnPos = cam.transform.position + forward * spawnDistanceAhead;

    // Random kiri/kanan berdasarkan kamera orientation
    spawnPos += cam.transform.right * Random.Range(-spawnDistanceRange, spawnDistanceRange);

    // Tinggi
    spawnPos.y += Random.Range(-1f, 1f);

    // Spawn enemy
    Instantiate(enemyPrefab, spawnPos, Quaternion.LookRotation(forward));
    Debug.Log($"Enemy spawned in front of camera at {spawnPos}");
}


    void OnDrawGizmos()
    {
        // Visualisasi spawn area di depan player
        if (player != null)
        {
            Vector3 center = player.transform.position + player.transform.forward * spawnDistanceAhead;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, spawnDistanceRange);
        }
    }
}