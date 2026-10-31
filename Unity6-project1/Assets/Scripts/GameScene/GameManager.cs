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

    // Victory panel shown when the final boss is killed.
    public GameObject gameOverPanel;    // Assign the survival gameOverPanel here

    // Window shown when the player dies.
    public GameObject gameOverWindow;   // Assign the gameOverWindow which display when player die

    // Selectable character configurations; defaults to the Resources catalog.
    public CharacterDefinition[] characterDefinitions;

    // Runtime scene state.
    private float survivalTime = 0f;
    private bool gameEnded = false;
    private GameObject playerCharacter;
    private static GameManager instance;
    private bool finalBossDefeated;
    public int RandomSeed { get; private set; }
    public bool FinalBossDefeated => finalBossDefeated;
    public void NotifyFinalBossDefeated()
    {
        if (gameEnded || finalBossDefeated) return;
        PlayerHealth health = playerCharacter != null ? playerCharacter.GetComponent<PlayerHealth>() : null;
        if (health != null && health.currentHealth <= 0f) return;
        finalBossDefeated = true;
        EndGame();
    }

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
        RandomSeed = RunRandomSeed.Initialize();
        EnsureCoreSystems();
        Time.timeScale = 1f;
        survivalTime = 0f;
        gameEnded = false;
        finalBossDefeated = false;
        
        // Retrieve the selected character index from PlayerPrefs
        int selectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);    // Default to first character

        if (characterDefinitions == null || characterDefinitions.Length == 0)
            characterDefinitions = CharacterCatalog.Load();
        CharacterDefinition selected = CharacterCatalog.Resolve(characterDefinitions,
            PlayerPrefs.GetString(CharacterCatalog.SelectionKey, ""), selectedCharacterIndex);
        if (selected != null && selected.playerPrefab != null)
        {
            if (!ValidatePlayerPrefab(selected.playerPrefab)) return;
            playerCharacter = Instantiate(selected.playerPrefab, Vector3.zero, Quaternion.identity);
            PlayerController controller = playerCharacter.GetComponent<PlayerController>();
            if (controller != null) selected.ApplyTo(controller);
            return;
        }

        Debug.LogError("No playable character configured. Add CharacterDefinition assets to Resources/Characters " +
            "or assign GameManager.characterDefinitions, with a valid playerPrefab.");
    }

    // Preview-only prefabs are unsuitable for gameplay; report the missing setup explicitly.
    private static bool ValidatePlayerPrefab(GameObject prefab)
    {
        if (prefab == null || prefab.GetComponent<PlayerController>() == null ||
            prefab.GetComponent<Rigidbody2D>() == null || prefab.GetComponent<PlayerHealth>() == null)
        {
            Debug.LogError($"Cannot spawn player '{(prefab != null ? prefab.name : "None")}'. " +
                "Assign a playable prefab with PlayerController, Rigidbody2D and PlayerHealth; " +
                "image-only preview prefabs cannot be controlled.");
            return false;
        }
        return true;
    }

    // Creates runtime core managers if the scene does not already contain them.
    private void EnsureCoreSystems()
    {
        if (FindAnyObjectByType<ZoneRewardDirector>() == null)
            new GameObject("ZoneRewardDirector").AddComponent<ZoneRewardDirector>();

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

    // Survival time is display-only; only the final boss kill triggers victory.
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

        }
    }

    // Ends the run after the final boss kill, shows victory UI, and pauses gameplay.
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
