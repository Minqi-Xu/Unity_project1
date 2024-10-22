using UnityEngine;
public class ObjectScaler : MonoBehaviour
{
    private Vector3 baseScale;  // Store the initial scale of the object

    void Start()
    {
        // Store the original size of the object
        baseScale = transform.localScale;

        // Adjust the scale on start
        AdjustScale();
    }

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

    public void OnResolutionChanged()
    {
        // Adjust the scale whenever resolution is changed
        AdjustScale();
    }
}
