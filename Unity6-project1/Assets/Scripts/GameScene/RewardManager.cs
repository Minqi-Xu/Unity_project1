using UnityEngine;

// Central entry point for granting experience, upgrades, healing, and future rewards.
public class RewardManager : MonoBehaviour
{
    public const int EliteChestBuffCount = 2;
    public const int BossChestBuffCount = 3;
    [Range(0f, 1f), Tooltip("Extra chance for the first roll of other enemy chests to offer only primary-weapon specializations.")]
    public float primaryWeaponChestChance = .25f;
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

    public void DropEnemyChest(Vector2 position, SpriteRenderer source, EnemyRank rank, bool guaranteePrimaryWeapon = false)
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
        RewardChest reward = chest.AddComponent<RewardChest>();
        reward.enemyRank = rank;
        reward.guaranteePrimaryWeaponBuff = guaranteePrimaryWeapon;
    }

    public bool GrantEnemyChest(PlayerController player, EnemyRank rank, bool guaranteePrimaryWeapon = false)
    {
        if (player == null || UpgradeManager.Instance == null) return false;
        int count = GetEnemyChestBuffCount(rank);
        if (count == 0) return false;
        bool primaryFirst = guaranteePrimaryWeapon || Random.value < primaryWeaponChestChance;
        for (int i = 1; i <= count; i++)
            if (i == 1 && primaryFirst)
                UpgradeManager.Instance.TryOfferPrimaryWeaponRewards(player, $"Treasure Chest {i}/{count} | Primary Weapon Specialization");
            else UpgradeManager.Instance.TryOfferRareRewards(player, i, count);
        return true;
    }

    public void GrantExhaustedChestReward(PlayerController player)
    {
        // Each exhausted roll has a fallback, including rolls already in the selection queue.
        PlayerHealth health = player != null ? player.GetComponent<PlayerHealth>() : null;
        if (health != null) HealPlayer(health, health.maxHealth * .5f);
        Debug.Log("[Reward] Rare upgrade pool exhausted; chest roll restores 50% max health.");
    }

    public bool GrantZoneReward(PlayerController player, ZoneRewardType type)
    {
        if (player == null) return false;
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health == null || health.currentHealth <= 0f) return false;
        switch (type)
        {
            case ZoneRewardType.Altar:
                if (UpgradeManager.Instance == null) return false;
                UpgradeManager.Instance.TryOfferZoneRewards(player, false);
                return true;
            case ZoneRewardType.DangerousChest:
                if (UpgradeManager.Instance == null) return false;
                UpgradeManager.Instance.TryOfferPrimaryWeaponRewards(player, "Dangerous Chest | Guaranteed Primary Weapon Specialization");
                return true;
            case ZoneRewardType.HealingSpring:
                HealPlayer(health, health.maxHealth * .5f);
                return true;
            case ZoneRewardType.WeaponWreckage:
                PlayerWeaponSystem weapon = player.GetComponent<PlayerWeaponSystem>();
                if (weapon == null || weapon.Definition == null) return false;
                weapon.UpgradeLevel();
                Debug.Log($"[Reward] {weapon.Definition.displayName} level {weapon.WeaponLevel}, damage +15% per level.");
                return true;
            case ZoneRewardType.ExperienceMagnet:
                // Collect all existing map drops, including those outside the camera view.
                // Snapshot before collecting: level-up pauses do not cancel the rest of the reward.
                foreach (ExperienceDrop drop in FindObjectsByType<ExperienceDrop>())
                    drop.Collect(player);
                return true;
            default: return false;
        }
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
