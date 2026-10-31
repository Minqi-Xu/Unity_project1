using UnityEngine;
using UnityEngine.UI; // For UI elements
using TMPro;
using System.Collections; // For TextMeshPro

// Tracks player health, applies incoming damage scaling, updates health UI, and handles death.
public class PlayerHealth : MonoBehaviour
{
    // Maximum player health before max-health upgrades.
    public float maxHealth = 100f; // Set maximum health

    // Current player health.
    public float currentHealth; // Current health

    // UI fill image that displays remaining health.
    public Image healthBar; // Reference to the health bar UI

    // UI fill image that displays missing health as damage overlay.
    public Image damageOverlay; // Reference to the damageOverlay UI

    // UI text that displays health as a percentage.
    public TextMeshProUGUI healthPercentText; // Reference to the health percent text UI

    // Rate at which incoming damage increases with player survival time.
    public float damageReceiveIncreasingRate = 0.005f; // player damage receiving increasing rate over time

    // Base incoming damage multiplier before time scaling and stat reduction.
    public float damageMultiplier = 1f;

    // Maximum damage the player can receive from one hit.
    public float maxReceivingDmg = 80f;

    // Game over UI shown when the player dies.
    public GameObject gameOverUI;  // Reference to the game-over UI shown when the player dies

    // Cached damage-feedback state.
    private SpriteRenderer playerSprite;    // Reference to the player's SpriteRenderer component
    private Coroutine flashCoroutine;
    // Scaled-time expiry of immunity granted by the last successful hit.
    private float invulnerableUntil;

    // Initializes health, UI references, and the sprite used for damage flashing.
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

    // Applies enemy damage after time scaling, player stat reduction, and max-hit clamping.
    public void TakeDamage(float damage, PlayerController player)
    {
        if (currentHealth <= 0f || damage <= 0f || Time.timeScale <= 0f || Time.time < invulnerableUntil) return;
        PlayerController playerController = player != null ? player : GetComponent<PlayerController>();
        if (playerController != null && playerController.IsDashInvulnerable) return;
        float playerGameTime = playerController != null ? playerController.gameTime : 0f;
        float final_damage = damage * (damageMultiplier + damageReceiveIncreasingRate * playerGameTime);
        PlayerStats stats = playerController != null ? playerController.Stats : GetComponent<PlayerStats>();
        if (stats != null)
        {
            final_damage *= stats.GetIncomingDamageMultiplier();
        }

        final_damage = Mathf.Min(final_damage, maxReceivingDmg);
        final_damage = CombatTuning.ApplyIncomingDamage(final_damage);
        if (final_damage <= 0f) return;
        invulnerableUntil = Time.time + (stats != null ? Mathf.Max(0f, stats.hitInvulnerability) : 0f);
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

    // Increases maximum health and optionally heals by the same amount.
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

    // Restores health without exceeding max health.
    public void Heal(float amount)
    {
        if (amount <= 0f || currentHealth <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        UpdateHealthUI();
    }

    // Logs player status after health changes.
    private void LogPlayerStatusChanged(PlayerController playerController, float enemyDamage)
    {
        if (playerController != null)
        {
            playerController.DebugLogPlayerStatus("Take Damage", enemyDamage);
            return;
        }

        Debug.Log($"[PlayerStatus] Take Damage | PlayerDMG: N/A | PlayerHP: {currentHealth:0.##} | EnemyDMG: {enemyDamage:0.##}");
    }

    // Updates health bar, percent text, and damage overlay UI.
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

    // Flashes the player sprite briefly after taking damage.
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

    // Shows game-over UI and pauses the game.
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

    // Finds a component on a scene object by object name.
    private static T FindComponentByName<T>(string objectName) where T : Component
    {
        GameObject target = GameObject.Find(objectName);
        return target != null ? target.GetComponent<T>() : null;
    }
}
