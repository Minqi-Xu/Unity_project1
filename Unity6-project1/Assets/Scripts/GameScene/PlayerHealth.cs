using UnityEngine;
using UnityEngine.UI; // For UI elements
using TMPro;
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
    private Coroutine flashCoroutine;

    void Start()
    {
        healthBar = FindComponentByName<Image>("HealthBar");
        damageOverlay = FindComponentByName<Image>("DamageOverlay");
        healthPercentText = FindComponentByName<TextMeshProUGUI>("HealthPercent");
        gameOverUI = GameManager.Instance != null ? GameManager.Instance.gameOverWindow : gameOverUI;

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
        PlayerController playerController = player != null ? player : GetComponent<PlayerController>();
        float playerGameTime = playerController != null ? playerController.gameTime : 0f;
        float final_damage = damage * (damageMultiplier + damageReceiveIncreasingRate * playerGameTime);
        PlayerStats stats = playerController != null ? playerController.Stats : GetComponent<PlayerStats>();
        if (stats != null)
        {
            final_damage *= stats.GetIncomingDamageMultiplier();
        }

        final_damage = Mathf.Min(final_damage, maxReceivingDmg);
        // Debug.Log($"Player receives {final_damage} damage");
        currentHealth -= final_damage; // Reduce health by damage amount
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't go below 0
        UpdateHealthUI();
        LogPlayerStatusChanged(playerController, final_damage);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Start flashing effect when player receives damage
            if (playerSprite != null)
            {
                if (flashCoroutine != null)
                {
                    StopCoroutine(flashCoroutine);
                }

                flashCoroutine = StartCoroutine(FlashOnDamage());
            }
        }
    }

    public void IncreaseMaxHealth(float amount, bool healBySameAmount)
    {
        if (amount <= 0f)
        {
            return;
        }

        maxHealth += amount;
        if (healBySameAmount)
        {
            currentHealth += amount;
        }

        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthUI();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        UpdateHealthUI();
    }

    private void LogPlayerStatusChanged(PlayerController playerController, float enemyDamage)
    {
        if (playerController != null)
        {
            playerController.DebugLogPlayerStatus("Take Damage", enemyDamage);
            return;
        }

        Debug.Log($"[PlayerStatus] Take Damage | PlayerDMG: N/A | PlayerHP: {currentHealth:0.##} | EnemyDMG: {enemyDamage:0.##}");
    }

    private void UpdateHealthUI()
    {
        float healthFraction = maxHealth > 0f ? currentHealth / maxHealth : 0f;

        // Update health bar and percent text
        if (healthBar != null)
        {
            healthBar.fillAmount = healthFraction; // Update health bar
        }

        if (healthPercentText != null)
        {
            healthPercentText.text = $"{healthFraction * 100:0}%"; // Update health percentage text
        }

        // Update damage overlay
        if (damageOverlay != null)
        {
            damageOverlay.fillAmount = 1 - healthFraction; // Red cover over the health bar
        }
    }

    private IEnumerator FlashOnDamage()
    {
        if (playerSprite == null)
        {
            yield break;
        }

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
        flashCoroutine = null;
    }

    private void Die()
    {
        // Handle player death (e.g., restart game, show game over screen, etc.)
        Debug.Log("Player is dead!");
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        Time.timeScale = 0f;    // Pause the game
    }

    private static T FindComponentByName<T>(string objectName) where T : Component
    {
        GameObject target = GameObject.Find(objectName);
        return target != null ? target.GetComponent<T>() : null;
    }
}
