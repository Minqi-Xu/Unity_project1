using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public TextMeshProUGUI countdownText;  // Reference to a countdown text UI
    public TextMeshProUGUI pauseText;   // Reference to PAUSED text UI
    private bool isPaused = false;  // Flag to track if game is paused
    private bool isCountDown = false;   // Flag to track if countdown is in progress

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

    public void Resume()
    {
        // Only resume if countdown is not active
        if(!isCountDown)
        {
            StartCoroutine(CountdownAndResume());   // start the countdown coroutine
        }  
    }

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

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        pauseText.gameObject.SetActive(true);   // Show PAUSED text
        countdownText.gameObject.SetActive(false);  // Hide the countdown
        Time.timeScale = 0f;    // Freeze time
        isPaused = true;
    }

    public void RestartGmae()
    {
        Time.timeScale = 1f;    // Resume to normal speed
        isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);     // Currently GameScene, but is to restart current scene
    }

    public void OpenSettings()
    {
        // Store the current scene before opening settings
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("SettingsMenu");
    }

    public void QuitToStartMenu()
    {
        Time.timeScale = 1f;    // Resume to normal speed
        isPaused = false;
        SceneManager.LoadScene("StartMenu");
    }
}