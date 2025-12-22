using UnityEngine;

public class EnemyVisualBuilder : MonoBehaviour
{
    [Header("Visual Settings")]
    public Color bodyColor = Color.grey;
    public Color accentColor = Color.red;

    void Start()
    {
        // Cek apakah ada mesh bawaan (misal Cube default) dan hapus
        // supaya tidak tumpang tindih dengan model baru yang mau kita rakit
        MeshFilter oldMesh = GetComponent<MeshFilter>();
        if (oldMesh != null) Destroy(oldMesh);

        MeshRenderer oldRen = GetComponent<MeshRenderer>();
        if (oldRen != null) Destroy(oldRen);

        // Mulai merakit bentuk drone
        BuildDroneShape();
    }

    void BuildDroneShape()
    {
        // --- BODY UTAMA ---
        // Bikin kotak, jadikan anak dari objek ini, lalu ubah ukurannya
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(1f, 0.5f, 2f); 
        ApplyMaterial(body, bodyColor);
        
        // Hapus collider di anak objek supaya fisikanya lebih ringan
        // Kita cukup pakai satu collider besar di script induk (Enemy.cs)
        Destroy(body.GetComponent<BoxCollider>());

        // --- SAYAP ---
        // Bikin kotak lagi, tapi ditarik lebar ke samping
        GameObject wings = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wings.transform.SetParent(transform);
        wings.transform.localPosition = Vector3.zero;
        wings.transform.localScale = new Vector3(3f, 0.1f, 0.8f); 
        ApplyMaterial(wings, bodyColor);
        Destroy(wings.GetComponent<BoxCollider>());

        // --- MESIN TURBIN ---
        // Panggil fungsi helper biar gak nulis kode berulang untuk kiri & kanan
        CreateEngine(new Vector3(-1.5f, 0, 0));
        CreateEngine(new Vector3(1.5f, 0, 0));

        // --- KOKPIT ---
        // Bikin bola kecil di atas depan, kasih efek menyala
        GameObject cockpit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cockpit.transform.SetParent(transform);
        cockpit.transform.localPosition = new Vector3(0, 0.3f, 0.5f);
        cockpit.transform.localScale = Vector3.one * 0.5f;
        ApplyMaterial(cockpit, accentColor, true); 
        Destroy(cockpit.GetComponent<SphereCollider>());
    }

    // Fungsi helper untuk bikin mesin (Silinder yang ditidurkan 90 derajat)
    void CreateEngine(Vector3 pos)
    {
        GameObject engine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        engine.transform.SetParent(transform);
        engine.transform.localPosition = pos;
        engine.transform.localRotation = Quaternion.Euler(90, 0, 0); 
        engine.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
        ApplyMaterial(engine, Color.darkGray);
        Destroy(engine.GetComponent<CapsuleCollider>());
    }

    // Fungsi untuk mewarnai objek dan atur shader
    void ApplyMaterial(GameObject obj, Color col, bool isEmissive = false)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        
        // Cari shader URP. Kalau warnanya jadi pink/magenta, ganti string ini jadi "Standard"
        rend.material = new Material(Shader.Find("Universal Render Pipeline/Lit")); 
        rend.material.color = col;
        
        // Kalau emissive true, bikin warnanya bercahaya (glowing)
        if (isEmissive)
        {
            rend.material.EnableKeyword("_EMISSION");
            rend.material.SetColor("_EmissionColor", col * 2f);
        }
    }
}