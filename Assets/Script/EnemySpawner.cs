using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefab")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int initialEnemyCount = 3;
    [SerializeField] private float spawnInterval = 5f;

    [Header("Spawn Area (Di depan pesawat)")]
    [SerializeField] private float spawnDistanceFromPlayer = 50f; // Jarak spawn dari pesawat
    [SerializeField] private float spawnWidth = 20f;              // Lebar area spawn (kiri-kanan)
    [SerializeField] private float spawnHeight = 5f;              // Variasi tinggi spawn

    private Transform playerTransform;
    private float timeSinceLastSpawn = 0f;

    void Start()
    {
        // Cari pesawat
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure Player has tag 'Player'");
            return;
        }

        // Spawn musuh awal
        for (int i = 0; i < initialEnemyCount; i++)
        {
            SpawnEnemy();
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Timer untuk spawn musuh baru
        timeSinceLastSpawn += Time.deltaTime;

        if (timeSinceLastSpawn >= spawnInterval)
        {
            SpawnEnemy();
            timeSinceLastSpawn = 0f;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab not assigned!");
            return;
        }

        if (playerTransform == null) return;

        // Spawn DI DEPAN pesawat (arah forward pesawat)
        Vector3 forwardDirection = playerTransform.forward;
        Vector3 spawnCenter = playerTransform.position + forwardDirection * spawnDistanceFromPlayer;

        // Random offset kiri-kanan
        Vector3 rightOffset = playerTransform.right * Random.Range(-spawnWidth / 2, spawnWidth / 2);

        // Random offset tinggi
        Vector3 upOffset = Vector3.up * Random.Range(0, spawnHeight);

        // Posisi spawn final
        Vector3 spawnPosition = spawnCenter + rightOffset + upOffset;

        // Spawn enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        Debug.Log($"Enemy spawned at {spawnPosition}");
    }

    // Debug visualization di Scene view
    private void OnDrawGizmos()
    {
        if (playerTransform == null) return;

        // Draw spawn area
        Gizmos.color = Color.red;
        Vector3 spawnCenter = playerTransform.position + playerTransform.forward * spawnDistanceFromPlayer;

        // Draw spawn box
        Gizmos.DrawWireCube(spawnCenter + Vector3.up * spawnHeight / 2,
                           new Vector3(spawnWidth, spawnHeight, 5));
    }
}