using UnityEngine;
using UnityEngine.SceneManagement;

// Builds the data-driven roster and saves the selected character before gameplay.
public class CharacterSelectionManager : MonoBehaviour
{
    public CharacterDefinition[] characterDefinitions;
    private CharacterSelectionUI selectionUI;
    private int selectedCharacterIndex;

    private void Start()
    {
        if (characterDefinitions == null || characterDefinitions.Length == 0)
            characterDefinitions = CharacterCatalog.Load();
        if (characterDefinitions.Length == 0)
        {
            Debug.LogError("No character definitions found. Add assets to Resources/Characters.");
            return;
        }

        CharacterDefinition selected = CharacterCatalog.Resolve(characterDefinitions,
            PlayerPrefs.GetString(CharacterCatalog.SelectionKey, ""), PlayerPrefs.GetInt("SelectedCharacter", 0));
        selectedCharacterIndex = System.Array.IndexOf(characterDefinitions, selected);
        selectionUI = gameObject.AddComponent<CharacterSelectionUI>();
        selectionUI.Build(characterDefinitions, SelectCharacter, ConfirmSelection);
        selectionUI.Show(selected, selectedCharacterIndex);
    }

    public void SelectCharacter(int index)
    {
        if (characterDefinitions == null || characterDefinitions.Length == 0 || selectionUI == null) return;
        selectedCharacterIndex = Mathf.Clamp(index, 0, characterDefinitions.Length - 1);
        selectionUI.Show(characterDefinitions[selectedCharacterIndex], selectedCharacterIndex);
    }

    public void ConfirmSelection()
    {
        if (characterDefinitions == null || characterDefinitions.Length == 0) return;
        PlayerPrefs.SetString(CharacterCatalog.SelectionKey, characterDefinitions[selectedCharacterIndex].characterId);
        SceneManager.LoadScene("GameScene");
    }
}
