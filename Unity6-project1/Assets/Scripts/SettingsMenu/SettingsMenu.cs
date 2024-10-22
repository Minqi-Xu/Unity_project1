using UnityEngine;
using UnityEngine.UI;
using TMPro; // for TextMeshPro
using System.Collections.Generic;
using UnityEngine.SceneManagement; // Required for List<>

public class SettingsMenu : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    private Resolution[] availableResolutions;
    private int currentResolutionIndex = 0;

    void Start()
    {
        // Fetch available resolutions
        availableResolutions = new Resolution[]
        {
            new Resolution { width = 1366, height = 768 },
            new Resolution { width = 1920, height = 1080 },
            new Resolution { width = 2560, height = 1440 },
            new Resolution { width = 3840, height = 2160 }
        };

        // Populate the dropdown with available resolutions
        resolutionDropdown.ClearOptions();
        List<string> resolutionOptions = new List<string>();

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            string resolutionOption = availableResolutions[i].width + " x " + availableResolutions[i].height;
            resolutionOptions.Add(resolutionOption);
        }

        resolutionDropdown.AddOptions(resolutionOptions);

        // Reflect current settings when the settings menu is opened
        LoadCurrentSettings();
    }

    // Reflect the current game settings in the dropdown and toggle
    void LoadCurrentSettings()
    {
        // Get the current resolution
        currentResolutionIndex = GetCurrentResolutionIndex();
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Set fullscreen toggle to match the current fullscreen state
        fullscreenToggle.isOn = Screen.fullScreen;
    }

    // Method to find the index of the current screen resolution
    int GetCurrentResolutionIndex()
    {
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            if (Screen.width == availableResolutions[i].width && Screen.height == availableResolutions[i].height)
            {
                return i;
            }
        }
        return 0; // Default to the first resolution if no match found
    }

    public void SetResolution()
    {
        int resolutionIndex = resolutionDropdown.value;
        Resolution resolution = availableResolutions[resolutionIndex];
        bool isFullscreen = fullscreenToggle.isOn;
        Screen.SetResolution(resolution.width, resolution.height, isFullscreen);
        Debug.Log($"Setting resolution to: {resolution.width} x {resolution.height}, Fullscreen: {isFullscreen}");
    }

    public void SetFullscreen()
    {
        Screen.fullScreen = fullscreenToggle.isOn;
        Debug.Log($"Setting fullscreen: {fullscreenToggle.isOn}");
    }

    public void BackToPreviousMenu()
    {
        string previousScene = PlayerPrefs.GetString("PreviousScene", "StartMenu"); // Default to start menu if not found
        Time.timeScale = 1f;    // Resume to normal speed
        SceneManager.LoadScene(previousScene);
    }
}
