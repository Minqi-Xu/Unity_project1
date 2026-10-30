using UnityEngine;
using TMPro; // For TextMeshPro
using UnityEngine.SceneManagement;

// Controls game scene lifecycle, survival timer, player spawning, and end-game flow.
public class GameManager : MonoBehaviour
{
    // Active GameManager instance for the current game scene.
    public static GameManager Instance { get { return instance; } } // Singleton instance

    // UI text showing elapsed survival time.
    public TextMeshProUGUI survivalTimeText; // Assign in Inspector

    // Panel shown when survival target time is reached.
    public GameObject gameOverPanel;    // Assign the survival gameOverPanel here

    // Window shown when the player dies.
    public GameObject gameOverWindow;   // Assign the gameOverWindow which display when player die

    // Seconds the player needs to survive to complete the run.
    public float targetGameTime = 1200f;  // Time that player need to survive

    // Selectable character prefabs spawned at game start.
    public GameObject[] characterPrefabs;   // Assign the character prefabs

    // Runtime scene state.
    private float survivalTime = 0f;
    private bool gameEnded = false;
    private GameObject playerCharacter;
    private static GameManager instance;

    // Initializes singleton instance and removes duplicate managers.
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

    // Resets run state, ensures core systems exist, and spawns the selected player.
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

    // Creates runtime core managers if the scene does not already contain them.
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

    // Advances survival timer, updates UI, and checks the survival end condition.
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

    // Ends the run, shows the survival end UI, and pauses gameplay.
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

    // Restarts the current game scene.
    public void RestartGame()
    {
        // Unpause the game
        Time.timeScale = 1f;

        // Reload current game scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Returns to the start menu scene.
    public void ExitToStartMenu()
    {
        // Unpause the game
        Time.timeScale = 1f;

        // Load the start Menu scene
        SceneManager.LoadScene("StartMenu");
    }

    // Quits the application.
    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit(); // Quit the application
    }
}
