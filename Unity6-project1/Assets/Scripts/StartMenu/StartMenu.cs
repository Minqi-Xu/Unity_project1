using UnityEngine;
using UnityEngine.SceneManagement;

// Button callbacks for the start menu scene.
public class MainMenu : MonoBehaviour
{
    // Loads the game scene.
    public void StartGame()
    {
        // Debug log for start button can be re-enabled when needed.
        SceneManager.LoadScene("GameScene");
    }

    // Opens the character selection scene.
    public void GoToCharacterSelection()
    {
        SceneManager.LoadScene("CharacterSelection");
    }

    // Opens settings and records the current scene as the return target.
    public void OpenSettings()
    {
        // Store the current scene before opening settings.
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("SettingsMenu");
    }

    // Quits the application.
    public void QuitGame()
    {
        // Debug log for quit button can be re-enabled when needed.
        Application.Quit();
    }
}
