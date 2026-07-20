using UnityEngine;

/// <summary>
/// Adjusts an orthographic camera size based on current screen width.
/// </summary>
public class CameraScaler : MonoBehaviour
{
    // Camera component cached from this GameObject.
    private Camera cam;

    /// <summary>Orthographic size used at the base screen width.</summary>
    public float baseOrthographicSize = 5f;  

    /// <summary>Base screen width used to calculate camera scaling.</summary>
    public float baseWidth = 1920f; // Base resolution (e.g., 16:9 for 1920x1080)

    /// <summary>
    /// Caches camera component and applies initial camera scaling.
    /// </summary>
    void Start()
    {
        cam = GetComponent<Camera>();
        AdjustCameraSize();
    }

    /// <summary>
    /// Adjusts orthographic size according to current screen width.
    /// </summary>
    void AdjustCameraSize()
    {
        float currentWidth = Screen.width;

        // Adjust the orthographic size based on the aspect ratio
        cam.orthographicSize = baseOrthographicSize * (currentWidth / baseWidth);
    }

    /// <summary>
    /// Recalculates camera size after runtime resolution changes.
    /// </summary>
    // Optionally call this if the resolution changes at runtime
    public void OnResolutionChanged()
    {
        AdjustCameraSize();
    }
}
