using UnityEngine;
using UnityEngine.UI;
using TMPro; // for TextMeshPro
using System.Collections.Generic;
using UnityEngine.SceneManagement; // Required for List<>

// Controls resolution and fullscreen settings UI.
public class SettingsMenu : MonoBehaviour
{
    // Dropdown listing supported resolution choices.
    public TMP_Dropdown resolutionDropdown;

    // Toggle used to apply fullscreen mode.
    public Toggle fullscreenToggle;

    // Supported resolutions and selected index.
    private Resolution[] availableResolutions;
    private int currentResolutionIndex = 0;

    // Validates UI references, populates resolution options, and reflects current settings.
    void Start()
    {
        if (resolutionDropdown == null || fullscreenToggle == null)
        {
            Debug.LogError("SettingsMenu is missing required UI references.");
            enabled = false;
            return;
        }

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

    // Reflect the current game settings in the dropdown and toggle.
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

    // Finds the dropdown index matching the current screen resolution.
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

    // Applies selected resolution and notifies camera/object scalers.
    public void SetResolution()
    {
        int resolutionIndex = Mathf.Clamp(resolutionDropdown.value, 0, availableResolutions.Length - 1);
        Resolution resolution = availableResolutions[resolutionIndex];
        bool isFullscreen = fullscreenToggle.isOn;
        Screen.SetResolution(resolution.width, resolution.height, isFullscreen);
        Debug.Log($"Setting resolution to: {resolution.width} x {resolution.height}, Fullscreen: {isFullscreen}");

        // Adjust the camera size after the resolution change
        Camera mainCamera = Camera.main;
        CameraScaler cameraScaler = mainCamera != null ? mainCamera.GetComponent<CameraScaler>() : null;
        if(cameraScaler != null)
        {
            cameraScaler.OnResolutionChanged();
        }
        BroadcastResolutionChange();
    }

    // Applies fullscreen toggle state.
    public void SetFullscreen()
    {
        if (fullscreenToggle == null)
        {
            return;
        }

        Screen.fullScreen = fullscreenToggle.isOn;
        Debug.Log($"Setting fullscreen: {fullscreenToggle.isOn}");
    }

    // Returns to the scene that opened the settings menu.
    public void BackToPreviousMenu()
    {
        string previousScene = PlayerPrefs.GetString("PreviousScene", "StartMenu"); // Default to start menu if not found
        Time.timeScale = 1f;    // Resume to normal speed
        SceneManager.LoadScene(previousScene);
    }

    // Notifies active ObjectScaler components after a resolution change.
    void BroadcastResolutionChange()
    {
        // Send a message to all objects that have an ObjectScaler to adjust their size
        ObjectScaler[] scalers = FindObjectsByType<ObjectScaler>(FindObjectsInactive.Exclude);
        foreach (var scaler in scalers)
        {
            scaler.OnResolutionChanged();
        }
    }
}
