using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central entry point for granting experience, upgrades, healing, and future rewards.
/// </summary>
public class RewardManager : MonoBehaviour
{
    /// <summary>Active reward manager instance for the current scene.</summary>
    public static RewardManager Instance { get; private set; }

    /// <summary>Fractional experience stored per player so small multipliers accumulate correctly.</summary>
    private readonly Dictionary<PlayerController, float> experienceRemainders = new Dictionary<PlayerController, float>();

    /// <summary>
    /// Initializes the singleton instance for reward delivery.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Grants experience to a player after applying experience multipliers.
    /// </summary>
    public void GrantExperience(PlayerController player, int amount)
    {
        if (player == null)
        {
            return;
        }

        PlayerStats stats = player.Stats;
        float scaledAmount = stats != null ? stats.GetExperienceAmount(amount) : Mathf.Max(0f, amount);
        experienceRemainders.TryGetValue(player, out float remainder);

        float totalExperience = scaledAmount + remainder;
        int wholeExperience = Mathf.FloorToInt(totalExperience);
        experienceRemainders[player] = totalExperience - wholeExperience;

        if (wholeExperience > 0)
        {
            player.CollectExperience(wholeExperience);
        }
    }

    /// <summary>
    /// Grants a specific upgrade to a player and logs the resulting player status.
    /// </summary>
    public void GrantUpgrade(PlayerController player, UpgradeDefinition upgrade)
    {
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.ApplyUpgrade(upgrade, player) && player != null)
        {
            player.DebugLogPlayerStatus("Upgrade Reward", 0f);
        }
    }

    /// <summary>
    /// Heals a player health component and logs status when a PlayerController is present.
    /// </summary>
    public void HealPlayer(PlayerHealth health, float amount)
    {
        if (health != null)
        {
            health.Heal(amount);
            PlayerController player = health.GetComponent<PlayerController>();
            if (player != null)
            {
                player.DebugLogPlayerStatus("Heal Reward", 0f);
            }
        }
    }
}
