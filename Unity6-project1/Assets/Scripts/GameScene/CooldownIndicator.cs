using UnityEngine;
using UnityEngine.UI;

// Displays cooldown progress by shrinking a cover image over an icon.
public class CooldownIndicator : MonoBehaviour
{
    // Cover image scaled vertically while cooldown is active.
    public Image cooldownCover; // Assign the cover image in the Inspector

    // Duration of the currently displayed cooldown.
    public float cooldownTime; // Duration of the cooldown

    // Runtime cooldown display state.
    private float cooldownStartTime;
    private bool isCooldownActive;
    private RectTransform cooldownCoverTransform;

    // Validates the cover reference and starts hidden.
    private void Awake()
    {
        if (cooldownCover == null)
        {
            Debug.LogError($"{name} has no cooldown cover assigned.");
            enabled = false;
            return;
        }

        cooldownCoverTransform = cooldownCover.GetComponent<RectTransform>();
        cooldownCover.gameObject.SetActive(false); // Start hidden
    }

    // Starts displaying a cooldown for the provided duration.
    public void StartCooldown(float duration)
    {
        if (cooldownCover == null || cooldownCoverTransform == null || duration <= 0f)
        {
            return;
        }

        cooldownTime = duration;
        cooldownStartTime = Time.time;
        isCooldownActive = true;
        cooldownCover.gameObject.SetActive(true); // Show the cover
        UpdateCooldownCover(); // Initial update for display
        //Debug.Log($"Cooldown started for {duration} seconds.");
    }

    // Returns true when the current cooldown has reached its end time.
    public bool IsCooldownComplete()
    {
        return Time.time >= cooldownStartTime + cooldownTime; // Check if cooldown is complete
    }

    // Updates or hides the cooldown cover each frame.
    private void Update()
    {
        // Update the cooldown cover to shrink
        if (isCooldownActive)
        {
            UpdateCooldownCover(); // Call the method to update the cover
            if (IsCooldownComplete())
            {
                isCooldownActive = false; // Mark cooldown as inactive
            }
        }
        else
        {
            if (cooldownCover != null)
            {
                cooldownCover.gameObject.SetActive(false);
            }
        }
    }

    // Scales the cover image to match the remaining cooldown fraction.
    private void UpdateCooldownCover()
    {
        if (cooldownCoverTransform == null)
        {
            return;
        }

        // Adjust the cover's height based on remaining cooldown
        float remainingTime = (cooldownStartTime + cooldownTime) - Time.time; // Calculate remaining time
        float fillAmount = Mathf.Clamp01(remainingTime / cooldownTime); // Calculate remaining cooldown fraction

        cooldownCoverTransform.localScale = new Vector3(1, fillAmount, 1); // Scale height from bottom to top
        //Debug.Log($"Remaining cooldown: {remainingTime} seconds, Fill amount: {fillAmount}");
    }
}
