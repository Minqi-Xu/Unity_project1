using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        //Debug.Log($"START GAME BOTTON");
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        //Debug.Log($"QUIT GAME BOTTON");
        Application.Quit();
    }
}
