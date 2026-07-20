using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Handles pause UI, countdown resume, restart, settings navigation, and returning to the start menu.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    /// <summary>Root pause menu UI object.</summary>
    public GameObject pauseMenuUI;

    /// <summary>Countdown text shown before resuming gameplay.</summary>
    public TextMeshProUGUI countdownText;  // Reference to a countdown text UI

    /// <summary>Paused label shown while the menu is open.</summary>
    public TextMeshProUGUI pauseText;   // Reference to PAUSED text UI

    // Runtime pause state.
    private bool isPaused = false;  // Flag to track if game is paused
    private bool isCountDown = false;   // Flag to track if countdown is in progress

    /// <summary>
    /// Toggles pause when Escape is pressed.
    /// </summary>
    void Update()
    {
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

    /// <summary>
    /// Starts the countdown that resumes gameplay.
    /// </summary>
    public void Resume()
    {
        // Only resume if countdown is not active
        if(!isCountDown)
        {
            StartCoroutine(CountdownAndResume());   // start the countdown coroutine
        }  
    }

    /// <summary>
    /// Counts down in unscaled time, hides pause UI, and restores normal timeScale.
    /// </summary>
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

    /// <summary>
    /// Shows pause UI and freezes gameplay.
    /// </summary>
    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        pauseText.gameObject.SetActive(true);   // Show PAUSED text
        countdownText.gameObject.SetActive(false);  // Hide the countdown
        Time.timeScale = 0f;    // Freeze time
        isPaused = true;
    }

    /// <summary>
    /// Restarts the current scene from the pause menu.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;    // Resume to normal speed
        isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);     // Currently GameScene, but is to restart current scene
    }

    /// <summary>
    /// Backward-compatible wrapper for the existing scene button typo.
    /// </summary>
    public void RestartGmae()
    {
        RestartGame();
    }

    /// <summary>
    /// Opens settings and records this scene as the return target.
    /// </summary>
    public void OpenSettings()
    {
        // Store the current scene before opening settings
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("SettingsMenu");
    }

    /// <summary>
    /// Returns to the start menu and restores normal timeScale.
    /// </summary>
    public void QuitToStartMenu()
    {
        Time.timeScale = 1f;    // Resume to normal speed
        isPaused = false;
        SceneManager.LoadScene("StartMenu");
    }
}
