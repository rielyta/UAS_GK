using UnityEngine;
using TMPro; // Wajib ada untuk mengakses TextMeshPro

public class UIManager : MonoBehaviour
{
    public static UIManager instance; 

    [Header("UI Settings")]
    public TextMeshProUGUI scoreText; 

    private int score = 0; // Skor awal

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateScoreUI(); // skor 0 saat mulai
    }

    // fungsi yang akan dipanggil saat musuh mati
    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        // Mengubah tulisan di layar
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }
}