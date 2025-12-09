using UnityEngine;

[Header("Movement Stats")]
    public float moveSpeed = 15f;      // Kecepatan terbang
    public float turnSpeed = 2f;       // Kecepatan berputar mengejar pemain
    public float hoverFrequency = 2f;  // Seberapa cepat naik-turun (ombak)
    public float hoverAmplitude = 0.5f;// Seberapa tinggi naik-turunnya

    [Header("Gameplay")]
    public int maxHealth = 3;          // Nyawa musuh (butuh 3 peluru)
    public int scoreValue = 100;       // Poin jika mati ditembak
    public GameObject explosionPrefab; // Efek ledakan (VFX)

    [Header("Collision Settings")]
    public float damageToPlayer = 1;      // Damage jika menabrak badan pemain
    public bool destroyOnPlayerHit = true;// Apakah musuh hancur jika nabrak pemain?

    private Transform playerTarget;   // Menyimpan posisi pemain untuk dikejar
    private Rigidbody rb;             // Komponen fisik Unity
    private int currentHealth;        // Nyawa saat ini
    private float randomHoverOffset;  // Angka acak agar gerakan naik-turun tiap musuh tidak seragam
    private bool hasHitPlayer = false;// Supaya tidak menabrak berkali-kali dalam 1 frame

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Setup proper collision
        rb.useGravity = false; //matikan gravitasi biar ga jatuh
        rb.isKinematic = false; //supaya ga ditembus benda lain
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; //mencegah musuh menembus tembok
        rb.constraints = RigidbodyConstraints.FreezeRotation; //supaya tetap stabil saat ditabrak

        currentHealth = maxHealth;
        randomHoverOffset = Random.Range(0f, 10f);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTarget = player.transform;


        // Tambahkan komponen visual builder jika belum ada
        if (GetComponent<EnemyVisualBuilder>() == null)
            gameObject.AddComponent<EnemyVisualBuilder>();

        //mengecek apakah sudah ada collider, kalau tidak maka dibuatkan sphere collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.radius = 1f;
            sphere.isTrigger = false; 
            Debug.Log("Added SphereCollider to Enemy");
        }
        else
        {
            col.isTrigger = false; 

        }

        //memastikan tag objek adalah "Enemy" agar peluru pemain bisa mendeteksinya
        if (!gameObject.CompareTag("Enemy"))
        {
            gameObject.tag = "Enemy";

            Debug.Log("Set Enemy tag");

        }
    }

    void FixedUpdate()
    {
        if (playerTarget == null) return;

        // ROTASI: Menghadap ke Pemain
        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }

        // POSISI: Maju + Hover effect
        Vector3 forwardMovement = transform.forward * moveSpeed * Time.fixedDeltaTime;
        float hoverY = Mathf.Sin((Time.time + randomHoverOffset) * hoverFrequency) * hoverAmplitude * Time.fixedDeltaTime;
        Vector3 hoverMovement = Vector3.up * hoverY;
        rb.MovePosition(transform.position + forwardMovement + hoverMovement);
    }

    // === BULLET COLLISION (Trigger) ===
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("🎯 Enemy hit by bullet!");
            Destroy(other.gameObject);

            TakeDamage(1);
        }
    }

    // Cek tag "Player" DAN pastikan belum menabrak (cegah double-hit)
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !hasHitPlayer)
        {
            hasHitPlayer = true;
            Debug.Log("💥 Enemy collided with Player!");

            // Mengakses script "Pesawat" di objek pemain untuk mengurangi nyawanya
            Pesawat playerScript = collision.gameObject.GetComponent<Pesawat>();

            if (playerScript != null)
            {
                playerScript.TakeDamage((int)damageToPlayer, transform.position);
            }
            // Destroy enemy if set
            if (destroyOnPlayerHit)
            {
                Explode(false);
            }

        }
    }

    void TakeDamage(int damage)
    {
        currentHealth -= damage;
        StartCoroutine(HitFlash()); // Efek visual berkedip/membesar saat kena hit
        Debug.Log($"Enemy health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Explode(true);
        }
    }

    //U efek visual (Membesar sebentar lalu kembali normal)
    System.Collections.IEnumerator HitFlash()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.2f; // Perbesar 20%
        yield return new WaitForSeconds(0.1f);       // Tunggu 0.1 detik
        transform.localScale = originalScale;        // Kembalikan ukuran
    }

    // Fungsi kematian musuh
    void Explode(bool hitungScore)
    {
        Debug.Log($"💥 Enemy exploded! Score check: {hitungScore}");

        // Tambah skor ke UI Manager HANYA jika mati karena ditembak (hitungScore = true)
        if (UIManager.instance != null && hitungScore == true)
        {
            UIManager.instance.AddScore(scoreValue);
        }
        
        // Munculkan efek partikel ledakan di posisi musuh
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        
        // Hapus objek musuh dari game world
        Destroy(gameObject);
    }
}