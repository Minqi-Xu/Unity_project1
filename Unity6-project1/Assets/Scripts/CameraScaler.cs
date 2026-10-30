using UnityEngine;

// Adjusts an orthographic camera size based on current screen width.
public class CameraScaler : MonoBehaviour
{
    // Camera component cached from this GameObject.
    private Camera cam;

    // Orthographic size used at the base screen width.
    public float baseOrthographicSize = 5f;  

    // Base screen width used to calculate camera scaling.
    public float baseWidth = 1920f; // Base resolution (e.g., 16:9 for 1920x1080)

    // Caches camera component and applies initial camera scaling.
    void Start()
    {
        cam = GetComponent<Camera>();
        AdjustCameraSize();
    }

    // Adjusts orthographic size according to current screen width.
    void AdjustCameraSize()
    {
        float currentWidth = Screen.width;

        // Adjust the orthographic size based on the aspect ratio
        cam.orthographicSize = baseOrthographicSize * (currentWidth / baseWidth);
    }

    // Recalculates camera size after runtime resolution changes.
    // Optionally call this if the resolution changes at runtime
    public void OnResolutionChanged()
    {
        AdjustCameraSize();
    }
}
