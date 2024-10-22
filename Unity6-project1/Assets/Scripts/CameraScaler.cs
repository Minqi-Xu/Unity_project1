using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    private Camera cam;
    public float baseOrthographicSize = 5f;  
    public float baseWidth = 1920f; // Base resolution (e.g., 16:9 for 1920x1080)

    void Start()
    {
        cam = GetComponent<Camera>();
        AdjustCameraSize();
    }

    void AdjustCameraSize()
    {
        float currentWidth = Screen.width;

        // Adjust the orthographic size based on the aspect ratio
        cam.orthographicSize = baseOrthographicSize * (currentWidth / baseWidth);
    }

    // Optionally call this if the resolution changes at runtime
    public void OnResolutionChanged()
    {
        AdjustCameraSize();
    }
}
