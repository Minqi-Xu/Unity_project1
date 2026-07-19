using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    private readonly Dictionary<PlayerController, float> experienceRemainders = new Dictionary<PlayerController, float>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

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

    public void GrantUpgrade(PlayerController player, UpgradeDefinition upgrade)
    {
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.ApplyUpgrade(upgrade, player) && player != null)
        {
            player.DebugLogPlayerStatus("Upgrade Reward", 0f);
        }
    }

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
