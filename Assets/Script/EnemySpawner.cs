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

        // Arah depan player (forward)
        Vector3 playerForward = player.transform.forward;

        // Posisi dasar: di depan player
        Vector3 spawnPos = player.transform.position + playerForward * spawnDistanceAhead;

        // Tambah random offset kiri/kanan dan tinggi
        spawnPos += player.transform.right * Random.Range(-spawnDistanceRange, spawnDistanceRange);
        // SESUDAH:
        spawnPos.y = player.transform.position.y + Random.Range(-0.5f, 0.5f);

        // Instantiate enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        Debug.Log($"Enemy spawned at {spawnPos} (ahead of player)");
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