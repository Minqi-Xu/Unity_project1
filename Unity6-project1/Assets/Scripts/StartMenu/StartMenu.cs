using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Button callbacks for the start menu scene.
/// </summary>
public class MainMenu : MonoBehaviour
{
    /// <summary>
    /// Loads the game scene.
    /// </summary>
    public void StartGame()
    {
        // Debug log for start button can be re-enabled when needed.
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// Opens the character selection scene.
    /// </summary>
    public void GoToCharacterSelection()
    {
        SceneManager.LoadScene("CharacterSelection");
    }

    /// <summary>
    /// Opens settings and records the current scene as the return target.
    /// </summary>
    public void OpenSettings()
    {
        // Store the current scene before opening settings.
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("SettingsMenu");
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame()
    {
        // Debug log for quit button can be re-enabled when needed.
        Application.Quit();
    }
}
