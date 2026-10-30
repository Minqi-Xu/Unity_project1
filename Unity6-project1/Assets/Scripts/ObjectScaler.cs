using UnityEngine;

// Scales an object relative to current screen resolution.
public class ObjectScaler : MonoBehaviour
{
    // Initial local scale captured before resolution scaling is applied.
    private Vector3 baseScale;  // Store the initial scale of the object

    // Captures original scale and applies initial resolution scaling.
    void Start()
    {
        // Store the original size of the object
        baseScale = transform.localScale;

        // Adjust the scale on start
        AdjustScale();
    }

    // Applies uniform scaling using the smaller screen-size ratio.
    void AdjustScale()
    {
        // Define your base resolution (for example 1920x1080)
        float baseWidth = 1920f;
        float baseHeight = 1080f;

        // Calculate the current screen size ratio
        float widthRatio = Screen.width / baseWidth;
        float heightRatio = Screen.height / baseHeight;

        // Calculate the new scale
        float scaleMultiplier = Mathf.Min(widthRatio, heightRatio);  // Use the smaller ratio for consistent scaling
        transform.localScale = baseScale * scaleMultiplier;
    }

    // Recalculates scale after runtime resolution changes.
    public void OnResolutionChanged()
    {
        // Adjust the scale whenever resolution is changed
        AdjustScale();
    }
}
