using UnityEngine;
using TMPro; // For TextMeshPro
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get { return instance; } } // Singleton instance

    public TextMeshProUGUI survivalTimeText; // Assign in Inspector
    public GameObject gameOverPanel;    // Assign the survival gameOverPanel here
    public GameObject gameOverWindow;   // Assign the gameOverWindow which display when player die
    public float targetGameTime = 1200f;  // Time that player need to survive
    public GameObject[] characterPrefabs;   // Assign the character prefabs

    private float survivalTime = 0f;
    private bool gameEnded = false;
    private GameObject playerCharacter;
    private static GameManager instance;

    void Awake()
    {
        // Singleton pattern - ensure only one instance of GameManager exists
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);    // Destroy duplicate GameManager instances
        }
    }

    void Start()
    {
        survivalTime = 0f;
        gameEnded = false;
        
        // Retrieve the selected character index from PlayerPrefs
        int selectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);    // Default to first character

        // Instantiate the selected character
        playerCharacter = Instantiate(characterPrefabs[selectedCharacterIndex], Vector3.zero, Quaternion.identity);
    }

    void Update()
    {
        if (!gameEnded)
        {
            // Increment survival time
            survivalTime += Time.deltaTime;

            // Format time as minutes and seconds
            int minutes = Mathf.FloorToInt(survivalTime / 60);
            int seconds = Mathf.FloorToInt(survivalTime % 60);

            // Update the UI with the survival time
            survivalTimeText.text = string.Format("Survived Time: {0:00}:{1:00}", minutes, seconds);

            // Check if 20 minutes have passed
            if (survivalTime >= targetGameTime)
            {
                EndGame();
            }
        }
    }

    void EndGame()
    {
        gameEnded = true;

        // Show the Game Over UI
        gameOverPanel.SetActive(true);

        // Pause the game in background
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        // Unpause the game
        Time.timeScale = 1f;

        // Reload current game scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToStartMenu()
    {
        // Unpause the game
        Time.timeScale = 1f;

        // Load the start Menu scene
        SceneManager.LoadScene("StartMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit(); // Quit the application
    }
}
