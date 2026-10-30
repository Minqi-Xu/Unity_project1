using UnityEngine;
using UnityEngine.SceneManagement;

// Handles character preview, selection, and confirmation before entering the game scene.
public class CharacterSelectionManager : MonoBehaviour
{
    // Available character prefabs shown in selection and spawned in game.
    public GameObject[] characters; // Assign character prefabs in the Inspector

    // Preview anchor used to position the selected character preview.
    public Transform previewPosition;  // Assign the preview position

    // Runtime character selection state.
    private int selectedCharacterIndex = 0; // Index of the selected character
    private GameObject currentPreviewCharacter; // Stores the currently previewed character

    // Displays the default selected character at scene start.
    void Start()
    {
        // Optionally, display the first character as selected by default
        DisplaySelectedCharacter();
    }

    // Selects a character index and refreshes the preview.
    public void SelectCharacter(int index)
    {
        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("No character prefabs assigned to CharacterSelectionManager.");
            return;
        }

        // Store the selected character index
        selectedCharacterIndex = Mathf.Clamp(index, 0, characters.Length - 1);
        DisplaySelectedCharacter();
    }

    // Saves selected character index and loads the game scene.
    public void ConfirmSelection()
    {
        // Store the selected character index using PlayerPrefs so it's available in the game scene
        PlayerPrefs.SetInt("SelectedCharacter", selectedCharacterIndex);

        // Load the GameScene
        SceneManager.LoadScene("GameScene");
    }

    // Rebuilds the preview object for the currently selected character.
    void DisplaySelectedCharacter()
    {
        if (characters == null || characters.Length == 0 || previewPosition == null)
        {
            return;
        }

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
