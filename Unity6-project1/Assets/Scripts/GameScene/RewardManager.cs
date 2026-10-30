using UnityEngine;

// Central entry point for granting experience, upgrades, healing, and future rewards.
public class RewardManager : MonoBehaviour
{
    public const int EliteChestBuffCount = 2;
    public const int BossChestBuffCount = 3;
    public static int GetEnemyChestBuffCount(EnemyRank rank) =>
        rank == EnemyRank.Elite ? EliteChestBuffCount :
        rank == EnemyRank.MiniBoss || rank == EnemyRank.FinalBoss ? BossChestBuffCount : 0;
    // Active reward manager instance for the current scene.
    public static RewardManager Instance { get; private set; }

    // Initializes the singleton instance for reward delivery.
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void DropEnemyChest(Vector2 position, SpriteRenderer source, EnemyRank rank)
    {
        if (GetEnemyChestBuffCount(rank) == 0) return;
        GameObject chest = new GameObject("Elite Treasure Chest");
        chest.transform.position = position;
        SpriteRenderer visual = chest.AddComponent<SpriteRenderer>();
        if (source != null)
        {
            visual.sprite = source.sprite;
            visual.sharedMaterial = source.sharedMaterial;
            chest.transform.localScale = Vector3.one * .55f;
        }
        visual.color = new Color(1f, .8f, .15f);
        visual.sortingOrder = 10;
        chest.AddComponent<RewardChest>().enemyRank = rank;
    }

    public bool GrantEnemyChest(PlayerController player, EnemyRank rank)
    {
        if (player == null || UpgradeManager.Instance == null) return false;
        int count = GetEnemyChestBuffCount(rank);
        if (count == 0) return false;
        for (int i = 1; i <= count; i++)
            UpgradeManager.Instance.TryOfferRareRewards(player, i, count);
        return true;
    }

    public void GrantExhaustedChestReward(PlayerController player)
    {
        // Each exhausted roll has a fallback, including rolls already in the selection queue.
        PlayerHealth health = player != null ? player.GetComponent<PlayerHealth>() : null;
        if (health != null) HealPlayer(health, health.maxHealth * .5f);
        Debug.Log("[Reward] Rare upgrade pool exhausted; chest roll restores 50% max health.");
    }

    // Grants experience to a player after applying experience multipliers.
    public void GrantExperience(PlayerController player, float amount)
    {
        if (player == null || !ExperienceMath.IsPositiveFinite(amount))
        {
            return;
        }

        PlayerStats stats = player.Stats;
        float scaledAmount = stats != null ? stats.GetExperienceAmount(amount) : Mathf.Max(0f, amount);
        player.CollectExperience(scaledAmount);
    }

    // Grants a specific upgrade to a player and logs the resulting player status.
    public void GrantUpgrade(PlayerController player, UpgradeDefinition upgrade)
    {
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.ApplyUpgrade(upgrade, player) && player != null)
        {
            player.DebugLogPlayerStatus("Upgrade Reward", 0f);
        }
    }

    // Heals a player health component and logs status when a PlayerController is present.
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
