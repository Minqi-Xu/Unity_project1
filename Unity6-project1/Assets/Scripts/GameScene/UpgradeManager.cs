using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    public UpgradeDefinition[] availableUpgrades;
    public int choicesPerLevel = 3;
    public bool autoPickFirstChoiceForPrototype = true;

    public event Action<PlayerController, IReadOnlyList<UpgradeDefinition>> OnUpgradeChoicesOffered;

    private readonly Dictionary<string, int> upgradeLevels = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (availableUpgrades == null || availableUpgrades.Length == 0)
        {
            availableUpgrades = CreatePrototypeUpgradePool();
        }
    }

    public bool TryOfferLevelUpRewards(PlayerController player)
    {
        if (player == null)
        {
            return false;
        }

        List<UpgradeDefinition> choices = PickUpgradeChoices();
        if (choices.Count == 0)
        {
            return false;
        }

        Debug.Log($"[Upgrade] Level up choices: {FormatChoices(choices)}");
        OnUpgradeChoicesOffered?.Invoke(player, choices);

        if (autoPickFirstChoiceForPrototype)
        {
            return ApplyUpgrade(choices[0], player);
        }

        return true;
    }

    public bool ApplyUpgrade(UpgradeDefinition upgrade, PlayerController player)
    {
        if (upgrade == null || player == null)
        {
            return false;
        }

        PlayerStats stats = player.Stats;
        if (stats == null)
        {
            return false;
        }

        string upgradeKey = GetUpgradeKey(upgrade);
        upgradeLevels.TryGetValue(upgradeKey, out int currentLevel);
        if (upgrade.maxLevel > 0 && currentLevel >= upgrade.maxLevel)
        {
            return false;
        }

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        stats.ApplyUpgrade(upgrade, health);
        upgradeLevels[upgradeKey] = currentLevel + 1;

        Debug.Log($"[Upgrade] Applied {upgrade.DisplayName} ({upgradeLevels[upgradeKey]}/{upgrade.maxLevel})");
        return true;
    }

    private List<UpgradeDefinition> PickUpgradeChoices()
    {
        List<UpgradeDefinition> candidates = GetAvailableUpgrades();
        List<UpgradeDefinition> choices = new List<UpgradeDefinition>();
        int choiceCount = Mathf.Clamp(choicesPerLevel, 1, Mathf.Max(1, candidates.Count));

        for (int i = 0; i < choiceCount && candidates.Count > 0; i++)
        {
            int index = UnityEngine.Random.Range(0, candidates.Count);
            choices.Add(candidates[index]);
            candidates.RemoveAt(index);
        }

        return choices;
    }

    private List<UpgradeDefinition> GetAvailableUpgrades()
    {
        List<UpgradeDefinition> candidates = new List<UpgradeDefinition>();
        if (availableUpgrades == null)
        {
            return candidates;
        }

        foreach (UpgradeDefinition upgrade in availableUpgrades)
        {
            if (upgrade == null)
            {
                continue;
            }

            string upgradeKey = GetUpgradeKey(upgrade);
            upgradeLevels.TryGetValue(upgradeKey, out int currentLevel);
            if (upgrade.maxLevel <= 0 || currentLevel < upgrade.maxLevel)
            {
                candidates.Add(upgrade);
            }
        }

        return candidates;
    }

    private string GetUpgradeKey(UpgradeDefinition upgrade)
    {
        if (!string.IsNullOrWhiteSpace(upgrade.upgradeId))
        {
            return upgrade.upgradeId;
        }

        return !string.IsNullOrWhiteSpace(upgrade.name) ? upgrade.name : upgrade.DisplayName;
    }

    private string FormatChoices(IReadOnlyList<UpgradeDefinition> choices)
    {
        List<string> names = new List<string>();
        foreach (UpgradeDefinition choice in choices)
        {
            names.Add(choice.DisplayName);
        }

        return string.Join(", ", names);
    }

    private UpgradeDefinition[] CreatePrototypeUpgradePool()
    {
        return new UpgradeDefinition[]
        {
            CreateRuntimeUpgrade(
                "damage_training",
                "Damage Training",
                "Increase player damage by 10%.",
                UpgradeRarity.Common,
                10,
                new UpgradeEffect(UpgradeEffectType.DamageMultiplier, 0.1f)),
            CreateRuntimeUpgrade(
                "rapid_fire",
                "Rapid Fire",
                "Reduce bullet cooldown by 8%.",
                UpgradeRarity.Common,
                5,
                new UpgradeEffect(UpgradeEffectType.FireCooldownReduction, 0.08f)),
            CreateRuntimeUpgrade(
                "battle_boots",
                "Battle Boots",
                "Increase movement speed by 8%.",
                UpgradeRarity.Common,
                5,
                new UpgradeEffect(UpgradeEffectType.MoveSpeedMultiplier, 0.08f)),
            CreateRuntimeUpgrade(
                "dash_practice",
                "Dash Practice",
                "Reduce dash cooldown by 10%.",
                UpgradeRarity.Common,
                5,
                new UpgradeEffect(UpgradeEffectType.DashCooldownReduction, 0.1f)),
            CreateRuntimeUpgrade(
                "vitality",
                "Vitality",
                "Increase max health by 20.",
                UpgradeRarity.Common,
                5,
                new UpgradeEffect(UpgradeEffectType.MaxHealth, 20f)),
            CreateRuntimeUpgrade(
                "armor_training",
                "Armor Training",
                "Reduce incoming damage by 5%.",
                UpgradeRarity.Common,
                5,
                new UpgradeEffect(UpgradeEffectType.DamageReduction, 0.05f)),
            CreateRuntimeUpgrade(
                "magnet",
                "Magnet",
                "Increase pickup radius by 20%.",
                UpgradeRarity.Common,
                5,
                new UpgradeEffect(UpgradeEffectType.PickupRadiusMultiplier, 0.2f)),
            CreateRuntimeUpgrade(
                "bomb_tech",
                "Bomb Tech",
                "Reduce bomb cooldown by 10%.",
                UpgradeRarity.Common,
                5,
                new UpgradeEffect(UpgradeEffectType.BombCooldownReduction, 0.1f))
        };
    }

    private UpgradeDefinition CreateRuntimeUpgrade(
        string upgradeId,
        string displayName,
        string description,
        UpgradeRarity rarity,
        int maxLevel,
        params UpgradeEffect[] effects)
    {
        UpgradeDefinition upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
        upgrade.name = upgradeId;
        upgrade.upgradeId = upgradeId;
        upgrade.displayName = displayName;
        upgrade.description = description;
        upgrade.rarity = rarity;
        upgrade.maxLevel = maxLevel;
        upgrade.effects = effects;
        return upgrade;
    }
}
