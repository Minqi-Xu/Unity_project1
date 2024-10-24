using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectionManager : MonoBehaviour
{
    public GameObject[] characters; // Assign character prefabs in the Inspector
    public Transform previewPosition;  // Assign the preview position

    private int selectedCharacterIndex = 0; // Index of the selected character
    private GameObject currentPreviewCharacter; // Stores the currently previewed character

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
        // Destroy the previous preview character, if any
        if(currentPreviewCharacter != null)
        {
            Destroy(currentPreviewCharacter);
        }

        // Instantiate the selected character at the preview position
        currentPreviewCharacter = Instantiate(characters[selectedCharacterIndex], previewPosition.position, Quaternion.identity);
        
        // Adjust the scale and position for the preview to fit the scene
        currentPreviewCharacter.transform.SetParent(previewPosition);   // Set parent to keep organized

    }
}
