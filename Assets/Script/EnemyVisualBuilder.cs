using UnityEngine;

public class EnemyVisualBuilder : MonoBehaviour
{
    [Header("Visual Settings")]
    public Color bodyColor = Color.grey;
    public Color accentColor = Color.red;

    void Start()
    {
        MeshFilter oldMesh = GetComponent<MeshFilter>();
        if (oldMesh != null) Destroy(oldMesh);

        MeshRenderer oldRen = GetComponent<MeshRenderer>();
        if (oldRen != null) Destroy(oldRen);

        BuildDroneShape();
    }

    void BuildDroneShape()
    {
        // --- MEMBUAT BODY UTAMA (Kotak memanjang) ---
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(1f, 0.5f, 2f); // Pipih memanjang
        ApplyMaterial(body, bodyColor);
        Destroy(body.GetComponent<BoxCollider>()); // Hapus collider anak

        // --- MEMBUAT SAYAP (Kiri & Kanan) ---
        GameObject wings = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wings.transform.SetParent(transform);
        wings.transform.localPosition = Vector3.zero;
        wings.transform.localScale = new Vector3(3f, 0.1f, 0.8f); // Lebar
        ApplyMaterial(wings, bodyColor);
        Destroy(wings.GetComponent<BoxCollider>());

        // --- MEMBUAT MESIN/TURBIN (Kiri & Kanan) ---
        CreateEngine(new Vector3(-1.5f, 0, 0));
        CreateEngine(new Vector3(1.5f, 0, 0));

        // --- MEMBUAT KOKPIT (Merah menyala) ---
        GameObject cockpit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cockpit.transform.SetParent(transform);
        cockpit.transform.localPosition = new Vector3(0, 0.3f, 0.5f);
        cockpit.transform.localScale = Vector3.one * 0.5f;
        ApplyMaterial(cockpit, accentColor, true);
        Destroy(cockpit.GetComponent<SphereCollider>());
    }

    void CreateEngine(Vector3 pos)
    {
        GameObject engine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        engine.transform.SetParent(transform);
        engine.transform.localPosition = pos;
        engine.transform.localRotation = Quaternion.Euler(90, 0, 0); // Tidurkan cylinder
        engine.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
        ApplyMaterial(engine, Color.darkGray);
        Destroy(engine.GetComponent<CapsuleCollider>());
    }

    void ApplyMaterial(GameObject obj, Color col, bool isEmissive = false)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        rend.material = new Material(Shader.Find("Universal Render Pipeline/Lit")); // Atau Standard
        rend.material.color = col;
        if (isEmissive)
        {
            rend.material.EnableKeyword("_EMISSION");
            rend.material.SetColor("_EmissionColor", col * 2f);
        }
    }
}