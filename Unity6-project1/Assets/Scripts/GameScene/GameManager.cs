using UnityEngine;
using TMPro; // For TextMeshPro
using UnityEngine.SceneManagement;

/// <summary>
/// Controls game scene lifecycle, survival timer, player spawning, and end-game flow.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>Active GameManager instance for the current game scene.</summary>
    public static GameManager Instance { get { return instance; } } // Singleton instance

    /// <summary>UI text showing elapsed survival time.</summary>
    public TextMeshProUGUI survivalTimeText; // Assign in Inspector

    /// <summary>Panel shown when survival target time is reached.</summary>
    public GameObject gameOverPanel;    // Assign the survival gameOverPanel here

    /// <summary>Window shown when the player dies.</summary>
    public GameObject gameOverWindow;   // Assign the gameOverWindow which display when player die

    /// <summary>Seconds the player needs to survive to complete the run.</summary>
    public float targetGameTime = 1200f;  // Time that player need to survive

    /// <summary>Selectable character prefabs spawned at game start.</summary>
    public GameObject[] characterPrefabs;   // Assign the character prefabs

    // Runtime scene state.
    private float survivalTime = 0f;
    private bool gameEnded = false;
    private GameObject playerCharacter;
    private static GameManager instance;

    /// <summary>
    /// Initializes singleton instance and removes duplicate managers.
    /// </summary>
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

    /// <summary>
    /// Resets run state, ensures core systems exist, and spawns the selected player.
    /// </summary>
    void Start()
    {
        EnsureCoreSystems();
        Time.timeScale = 1f;
        survivalTime = 0f;
        gameEnded = false;
        
        // Retrieve the selected character index from PlayerPrefs
        int selectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);    // Default to first character

        if (characterPrefabs == null || characterPrefabs.Length == 0)
        {
            Debug.LogError("No character prefabs assigned to GameManager.");
            return;
        }

        selectedCharacterIndex = Mathf.Clamp(selectedCharacterIndex, 0, characterPrefabs.Length - 1);

        // Instantiate the selected character
        playerCharacter = Instantiate(characterPrefabs[selectedCharacterIndex], Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// Creates runtime core managers if the scene does not already contain them.
    /// </summary>
    private void EnsureCoreSystems()
    {
        if (UpgradeManager.Instance == null)
        {
            GameObject upgradeManagerObject = new GameObject("UpgradeManager");
            upgradeManagerObject.AddComponent<UpgradeManager>();
        }

        if (RewardManager.Instance == null)
        {
            GameObject rewardManagerObject = new GameObject("RewardManager");
            rewardManagerObject.AddComponent<RewardManager>();
        }
    }

    /// <summary>
    /// Advances survival timer, updates UI, and checks the survival end condition.
    /// </summary>
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
            if (survivalTimeText != null)
            {
                survivalTimeText.text = string.Format("Survived Time: {0:00}:{1:00}", minutes, seconds);
            }

            // Check if 20 minutes have passed
            if (survivalTime >= targetGameTime)
            {
                EndGame();
            }
        }
    }

    /// <summary>
    /// Ends the run, shows the survival end UI, and pauses gameplay.
    /// </summary>
    void EndGame()
    {
        gameEnded = true;

        // Show the Game Over UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Pause the game in background
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Restarts the current game scene.
    /// </summary>
    public void RestartGame()
    {
        // Unpause the game
        Time.timeScale = 1f;

        // Reload current game scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Returns to the start menu scene.
    /// </summary>
    public void ExitToStartMenu()
    {
        // Unpause the game
        Time.timeScale = 1f;

        // Load the start Menu scene
        SceneManager.LoadScene("StartMenu");
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit(); // Quit the application
    }
}
