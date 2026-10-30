using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Handles pause UI, countdown resume, restart, settings navigation, and returning to the start menu.
public class PauseMenu : MonoBehaviour
{
    // Root pause menu UI object.
    public GameObject pauseMenuUI;

    // Countdown text shown before resuming gameplay.
    public TextMeshProUGUI countdownText;  // Reference to a countdown text UI

    // Paused label shown while the menu is open.
    public TextMeshProUGUI pauseText;   // Reference to PAUSED text UI

    // Runtime pause state.
    private bool isPaused = false;  // Flag to track if game is paused
    private bool isCountDown = false;   // Flag to track if countdown is in progress

    // Toggles pause when Escape is pressed.
    void Update()
    {
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.IsSelecting)
        {
            return;
        }

        // Check for ESC key press
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(isPaused)
            {
                Resume();   // Resume if game is paused
            }
            else
            {
                Pause();    // Pause if game if not paused
            }
        }
    }

    // Starts the countdown that resumes gameplay.
    public void Resume()
    {
        // Only resume if countdown is not active
        if(!isCountDown)
        {
            StartCoroutine(CountdownAndResume());   // start the countdown coroutine
        }  
    }

    // Counts down in unscaled time, hides pause UI, and restores normal timeScale.
    private IEnumerator CountdownAndResume()
    {
        pauseText.gameObject.SetActive(false);  // hide the PAUSED text
        countdownText.gameObject.SetActive(true);   // show countdown
        isCountDown = true;
        float countdownTime = 3f;   // Set countdown duration

        // display countdown
        while(countdownTime > 0)
        {
            countdownText.text = Mathf.Ceil(countdownTime).ToString();   // Update countdown
            countdownTime -= Time.unscaledDeltaTime;    // Decrease countdown time
            yield return null;  // wait for next frame
        }
        countdownText.text = "";    // clear countdown text
        countdownText.gameObject.SetActive(false);  // hide the countdown
        pauseMenuUI.SetActive(false);   // close pause menu
        Time.timeScale = 1f;    // Resume to normal speed
        isPaused = false;
        isCountDown = false;
    }

    // Shows pause UI and freezes gameplay.
    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        pauseText.gameObject.SetActive(true);   // Show PAUSED text
        countdownText.gameObject.SetActive(false);  // Hide the countdown
        Time.timeScale = 0f;    // Freeze time
        isPaused = true;
    }

    // Restarts the current scene from the pause menu.
    public void RestartGame()
    {
        Time.timeScale = 1f;    // Resume to normal speed
        isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);     // Currently GameScene, but is to restart current scene
    }

    // Backward-compatible wrapper for the existing scene button typo.
    public void RestartGmae()
    {
        RestartGame();
    }

    // Opens settings and records this scene as the return target.
    public void OpenSettings()
    {
        // Store the current scene before opening settings
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("SettingsMenu");
    }

    // Returns to the start menu and restores normal timeScale.
    public void QuitToStartMenu()
    {
        Time.timeScale = 1f;    // Resume to normal speed
        isPaused = false;
        SceneManager.LoadScene("StartMenu");
    }
}
