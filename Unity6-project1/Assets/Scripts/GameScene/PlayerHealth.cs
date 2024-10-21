using UnityEngine;
using UnityEngine.UI; // For UI elements
using TMPro;
using System;
using System.Collections; // For TextMeshPro

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f; // Set maximum health
    public float currentHealth; // Current health
    public Image healthBar; // Reference to the health bar UI
    public Image damageOverlay; // Reference to the damageOverlay UI
    public TextMeshProUGUI healthPercentText; // Reference to the health percent text UI
    public float damageReceiveIncreasingRate = 0.005f; // player damage receiving increasing rate over time
    public float damageMultiplier = 1f;
    public float maxReceivingDmg = 80f;
    public GameObject gameOverUI;  // Reference to the player's PriteRenderer component

    private SpriteRenderer playerSprite;    // Reference to the player's SpriteRenderer component

    void Start()
    {
        currentHealth = maxHealth; // Initialize health
        UpdateHealthUI();

        // Get the SpriteRenderer component to control player's visibility
        playerSprite = GetComponent<SpriteRenderer>();
        if (playerSprite == null)
        {
            Debug.LogError("Player not found!");
        }
    }

    public void TakeDamage(float damage, PlayerController player)
    {
        float final_damage = damage * (damageMultiplier + damageReceiveIncreasingRate * player.gameTime);
        final_damage = Mathf.Min(final_damage, maxReceivingDmg);
        // Debug.Log($"Player receives {final_damage} damage");
        currentHealth -= final_damage; // Reduce health by damage amount
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't go below 0
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Start flashing effect when player receives damage
            StartCoroutine(FlashOnDamage());
        }
    }

    private void UpdateHealthUI()
    {
        // Update health bar and percent text
        healthBar.fillAmount = currentHealth / maxHealth; // Update health bar
        healthPercentText.text = $"{(currentHealth / maxHealth) * 100:0}%"; // Update health percentage text

        // Update damage overlay
        damageOverlay.fillAmount = 1 - (currentHealth / maxHealth); // Red cover over the health bar
    }

    private IEnumerator FlashOnDamage()
    {
        float flashDuration = 1.5f; // Duration of the flashing effect
        float flashInterval = 0.1f; // Interval at which the player will appear/disappear
        float elapsedTime = 0f;

        while(elapsedTime < flashDuration)
        {
            // Toggle the visibility of the player
            playerSprite.enabled = !playerSprite.enabled;

            // Wait for the next flash interval
            yield return new WaitForSeconds(flashInterval);

            elapsedTime += flashInterval;
        }

        // Finally let player visible
        playerSprite.enabled = true;
    }

    private void Die()
    {
        // Handle player death (e.g., restart game, show game over screen, etc.)
        Debug.Log("Player is dead!");
        gameOverUI.SetActive(true);
        Time.timeScale = 0f;    // Pause the game
    }
}
