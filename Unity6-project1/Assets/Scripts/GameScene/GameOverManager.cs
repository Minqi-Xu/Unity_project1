using UnityEngine;
using UnityEngine.SceneManagement; // For scene management

public class GameOverManager : MonoBehaviour
{
    public void RestartGame()
    {
        Time.timeScale = 1f; // Resume time before restarting
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene
    }

    public void ExitToStartMenu()
    {
        Time.timeScale = 1f; // Resume time before going to the start menu
        SceneManager.LoadScene("StartMenu"); // Load the start menu scene
    }

    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit(); // Quit the application
    }
}
