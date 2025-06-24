using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Button startButton;

    [Header("Scene Settings")]
    public string sceneToLoad = "GameScene"; // Replace with your actual scene name

    private void Start()
    {
        // Ensure the button is assigned and listen for click
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            Debug.LogError("Start Button not assigned in inspector.");
        }
    }

    void OnStartButtonClicked()
    {
        // Optional: Add any pre-scene-loading effects like sound, animation, etc.
        SceneManager.LoadScene(sceneToLoad);
    }
}
