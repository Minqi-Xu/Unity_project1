using UnityEngine;

/// <summary>
/// Scales an object relative to current screen resolution.
/// </summary>
public class ObjectScaler : MonoBehaviour
{
    /// <summary>Initial local scale captured before resolution scaling is applied.</summary>
    private Vector3 baseScale;  // Store the initial scale of the object

    /// <summary>
    /// Captures original scale and applies initial resolution scaling.
    /// </summary>
    void Start()
    {
        // Store the original size of the object
        baseScale = transform.localScale;

        // Adjust the scale on start
        AdjustScale();
    }

    /// <summary>
    /// Applies uniform scaling using the smaller screen-size ratio.
    /// </summary>
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

    /// <summary>
    /// Recalculates scale after runtime resolution changes.
    /// </summary>
    public void OnResolutionChanged()
    {
        // Adjust the scale whenever resolution is changed
        AdjustScale();
    }
}
