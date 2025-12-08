using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // WAJIB ADA: Untuk mengakses komponen Image

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;

    [Header("Heart System")]
    public Image[] heartIcons;   // Ubah dari GameObject[] ke Image[]
    public Sprite fullHeart;     // Tempat menaruh gambar Hati Penuh
    public Sprite brokenHeart;   // Tempat menaruh gambar Hati Rusak

    private int score = 0;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateScoreUI();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // === FUNGSI UPDATE NYAWA (VERSI TUKAR GAMBAR) ===
    public void UpdateLives(int currentHealth)
    {
        for (int i = 0; i < heartIcons.Length; i++)
        {
            // Jika index hati masih dalam batas nyawa, pakai Hati Penuh
            if (i < currentHealth)
            {
                heartIcons[i].sprite = fullHeart;
            }
            // Jika tidak, ganti jadi Hati Rusak
            else
            {
                heartIcons[i].sprite = brokenHeart;
            }
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
    }

    public void TriggerGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}