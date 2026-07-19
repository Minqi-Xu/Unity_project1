using UnityEngine;
using UnityEngine.UI;

public class CooldownIndicator : MonoBehaviour
{
    public Image cooldownCover; // Assign the cover image in the Inspector
    public float cooldownTime; // Duration of the cooldown
    private float cooldownStartTime;
    private bool isCooldownActive;
    private RectTransform cooldownCoverTransform;

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

    public bool IsCooldownComplete()
    {
        return Time.time >= cooldownStartTime + cooldownTime; // Check if cooldown is complete
    }

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
