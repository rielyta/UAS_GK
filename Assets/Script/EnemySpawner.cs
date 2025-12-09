using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 3f;

    [Header("Spawn Area")]
    public float spawnDistance = 60f; // Jarak spawn di depan pemain
    public float spawnRadius = 30f;   // Radius sebaran musuh
    public float minHeightFromGround = 10f; // Tinggi minimum dari tanah

    private Transform player;
    private float timer;

    void Start()
    {
        // Cari objek pemain otomatis
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        // Logika timer
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0;
        }
    }

    void SpawnEnemy()
    {
        // 1. Tentukan titik pusat di depan pemain
        Vector3 spawnCenter = player.position + (player.forward * spawnDistance);

        // 2. Acak koordinat dalam lingkaran
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

        // 3. Konversi ke posisi dunia mengikuti orientasi pesawat
        Vector3 spawnPos = spawnCenter + (player.right * randomCircle.x) + (player.up * randomCircle.y);

        // 4. Koreksi ketinggian agar tidak tembus tanah
        float terrainHeight = Terrain.activeTerrain.SampleHeight(spawnPos);
        if (spawnPos.y < terrainHeight + minHeightFromGround)
        {
            spawnPos.y = terrainHeight + minHeightFromGround;
        }

        // 5. Spawn musuh dengan rotasi menghadap pemain
        Instantiate(enemyPrefab, spawnPos, Quaternion.LookRotation(player.position - spawnPos));
    }

    // Visualisasi area spawn di Editor (Bola Merah)
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