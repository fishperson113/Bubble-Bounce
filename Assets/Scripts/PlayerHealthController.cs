using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealthManager : MonoBehaviour
{
    public int maxHearts = 3;
    private int currentHearts;

    public Image[] heartImages;         
    public Sprite fullHeartSprite;      
    public Sprite emptyHeartSprite;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;  


    void Start()
    {
        currentHearts = maxHearts;
        UpdateHeartsUI();
    }

    public void LoseHeart()
    {
        if (currentHearts > 0)
        {
            currentHearts--;
            UpdateHeartsUI();

            if (currentHearts <= 0)
            {
                Debug.Log("Game Over");
                TriggerGameOver();
            }
        }
    }

    void UpdateHeartsUI()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < currentHearts)
                heartImages[i].sprite = fullHeartSprite;
            else
                heartImages[i].sprite = emptyHeartSprite;
        }
    }

    private void TriggerGameOver()
    {
        Debug.Log("Game Over");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);


        Time.timeScale = 0f;
    }



    public void ResetHearts()
    {
        currentHearts = maxHearts;
        UpdateHeartsUI();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
