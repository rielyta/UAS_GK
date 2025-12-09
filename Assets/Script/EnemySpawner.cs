using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 3f;

    [Header("Spawn Area")]
    public float spawnDistance = 60f; // Jarak spawn di depan pemain
    public float spawnRadius = 30f;   // Sebaran musuh (seberapa lebar areanya)
    public float minHeightFromGround = 10f; // Agar tidak spawn di bawah tanah

    private Transform player;
    private float timer;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0;
        }
    }

    void SpawnEnemy()
    {
        // 1. Tentukan titik pusat spawn di depan pemain
        Vector3 spawnCenter = player.position + (player.forward * spawnDistance);

        // 2. Acak posisi di dalam lingkaran (Random Point in Circle)
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

        // 3. Aplikasikan ke posisi dunia (X dan Y diacak, Z tetap di depan)
        // Kita gunakan transform.right dan transform.up agar spawn mengikuti orientasi pesawat pemain
        Vector3 spawnPos = spawnCenter + (player.right * randomCircle.x) + (player.up * randomCircle.y);

        // 4. Cek agar tidak spawn terlalu rendah (nabrak pohon/tanah)
        float terrainHeight = Terrain.activeTerrain.SampleHeight(spawnPos);
        if (spawnPos.y < terrainHeight + minHeightFromGround)
        {
            spawnPos.y = terrainHeight + minHeightFromGround;
        }

        // 5. Spawn
        Instantiate(enemyPrefab, spawnPos, Quaternion.LookRotation(player.position - spawnPos));
    }

    // Untuk melihat area spawn di Scene View
    private void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color.red;
            Vector3 center = player.position + (player.forward * spawnDistance);
            Gizmos.DrawWireSphere(center, spawnRadius);
        }
    }
}