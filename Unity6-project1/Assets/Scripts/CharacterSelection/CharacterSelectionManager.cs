using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectionManager : MonoBehaviour
{
    public GameObject[] characters; // Assign character prefabs in the Inspector
    private int selectedCharacterIndex = 0; // Index of the selected character

    void Start()
    {
        // Optionally, display the first character as selected by default
        DisplaySelectedCharacter();
    }

    public void SelectCharacter(int index)
    {
        // Store the selected character index
        selectedCharacterIndex = index;
        DisplaySelectedCharacter();
    }

    public void ConfirmSelection()
    {
        // Store the selected character index using PlayerPrefs so it's available in the game scene
        PlayerPrefs.SetInt("SelectedCharacter", selectedCharacterIndex);

        // Load the GameScene
        SceneManager.LoadScene("GameScene");
    }

    void DisplaySelectedCharacter()
    {
        // Display the selected character, e.g., highlight the button or preview the character in 3D or UI
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(i == selectedCharacterIndex); // Show only the selected character
        }
    }
}
