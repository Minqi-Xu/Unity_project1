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
    private bool isPaused = false;
    private bool isCountDown = false;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
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
        float countdownTime = 3f;

        // display countdown
        while(countdownTime > 0)
        {
            countdownText.text = Mathf.Ceil(countdownTime).ToString();   // Update countdown
            countdownTime -= Time.unscaledDeltaTime;
            yield return null;  // wait for next frame
        }
        countdownText.text = "";
        countdownText.gameObject.SetActive(false);  // hide the countdown
        pauseMenuUI.SetActive(false);
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

    public void QuitToStartMenu()
    {
        Time.timeScale = 1f;    // Resume to normal speed
        isPaused = false;
        SceneManager.LoadScene("StartMenu");
    }
}