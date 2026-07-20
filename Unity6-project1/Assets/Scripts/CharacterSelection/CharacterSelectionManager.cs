using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles character preview, selection, and confirmation before entering the game scene.
/// </summary>
public class CharacterSelectionManager : MonoBehaviour
{
    /// <summary>Available character prefabs shown in selection and spawned in game.</summary>
    public GameObject[] characters; // Assign character prefabs in the Inspector

    /// <summary>Preview anchor used to position the selected character preview.</summary>
    public Transform previewPosition;  // Assign the preview position

    // Runtime character selection state.
    private int selectedCharacterIndex = 0; // Index of the selected character
    private GameObject currentPreviewCharacter; // Stores the currently previewed character

    /// <summary>
    /// Displays the default selected character at scene start.
    /// </summary>
    void Start()
    {
        // Optionally, display the first character as selected by default
        DisplaySelectedCharacter();
    }

    /// <summary>
    /// Selects a character index and refreshes the preview.
    /// </summary>
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

    /// <summary>
    /// Saves selected character index and loads the game scene.
    /// </summary>
    public void ConfirmSelection()
    {
        // Store the selected character index using PlayerPrefs so it's available in the game scene
        PlayerPrefs.SetInt("SelectedCharacter", selectedCharacterIndex);

        // Load the GameScene
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// Rebuilds the preview object for the currently selected character.
    /// </summary>
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
