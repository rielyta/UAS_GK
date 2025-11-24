using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject enemyPrefab;
    public int initialEnemyCount = 5;
    public float spawnInterval = 3f;
    public Vector3 spawnAreaMin = new Vector3(-50, 2, -50);
    public Vector3 spawnAreaMax = new Vector3(50, 2, 50);
    
    float spawnTimer = 0f;

    void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab not assigned to spawner!");
            return;
        }
        
        // Spawn initial enemies
        for (int i = 0; i < initialEnemyCount; i++)
        {
            SpawnEnemy();
        }
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;
        
        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    void SpawnEnemy()
    {
        Vector3 spawnPos = new Vector3(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            spawnAreaMin.y,
            Random.Range(spawnAreaMin.z, spawnAreaMax.z)
        );
        
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}