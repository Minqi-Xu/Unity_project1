using System;
using System.Collections.Generic;
using UnityEngine;

// Generates level-up upgrade choices, applies selected upgrades, and tracks upgrade levels.
public class UpgradeManager : MonoBehaviour
{
    // Active upgrade manager instance for the current scene.
    public static UpgradeManager Instance { get; private set; }

    // Configured upgrade pool. If empty, runtime prototype upgrades are created.
    public UpgradeDefinition[] availableUpgrades;

    // Number of upgrade options offered per level-up.
    public int choicesPerLevel = 3;

    // Relative selection weights per variant; a higher value makes that rarity more frequent.
    [Min(0f)] public float commonWeight = 60f;
    [Min(0f)] public float rareWeight = 25f;
    [Min(0f)] public float epicWeight = 12f;
    [Min(0f)] public float legendaryWeight = 3f;

    // Automatically applies the first choice until a selection UI exists.
    public bool autoPickFirstChoiceForPrototype = false;

    // Pending level-ups are resolved one at a time using a fresh upgrade pool.
    private readonly Queue<KeyValuePair<PlayerController, int>> pendingPlayers = new Queue<KeyValuePair<PlayerController, int>>();
    private BuffSelectionUI selectionUI;
    private PlayerController selectingPlayer;
    private List<UpgradeDefinition> activeChoices;

    public bool IsSelecting => selectingPlayer != null;

    // Event raised when upgrade choices are generated for a player.
    public event Action<PlayerController, IReadOnlyList<UpgradeDefinition>> OnUpgradeChoicesOffered;

    // Applied level count for each upgrade key during this run.
    private readonly Dictionary<string, int> upgradeLevels = new Dictionary<string, int>();

    // Initializes the singleton and creates prototype upgrades if no pool is configured.
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        selectionUI = gameObject.AddComponent<BuffSelectionUI>();

        if (availableUpgrades == null || availableUpgrades.Length == 0)
        {
            availableUpgrades = CreatePrototypeUpgradePool();
        }
    }

    // Builds level-up choices for a player and optionally auto-applies one during prototype mode.
    public bool TryOfferLevelUpRewards(PlayerController player)
    {
        return player != null && OfferLevelUpRewards(player, player.level);
    }

    // Capture the earned level so queued rewards show the correct transition after multi-level gains.
    private bool OfferLevelUpRewards(PlayerController player, int earnedLevel)
    {
        if (player == null)
        {
            return false;
        }

        if (IsSelecting)
        {
            pendingPlayers.Enqueue(new KeyValuePair<PlayerController, int>(player, earnedLevel));
            return true;
        }

        List<UpgradeDefinition> choices = PickUpgradeChoices();
        if (choices.Count == 0)
        {
            return false;
        }

        Debug.Log($"[Upgrade] Level up choices: {FormatChoices(choices)}");
        if (autoPickFirstChoiceForPrototype)
        {
            return ApplyUpgrade(choices[0], player);
        }

        selectingPlayer = player;
        activeChoices = choices;
        selectionUI.Show(choices, earnedLevel, SelectUpgrade);
        OnUpgradeChoicesOffered?.Invoke(player, choices);
        return true;
    }

    // Only choices in the current offer can be selected; each offer is consumed once.
    private void SelectUpgrade(UpgradeDefinition upgrade)
    {
        if (!IsSelecting || activeChoices == null || !activeChoices.Contains(upgrade))
        {
            return;
        }

        if (!ApplyUpgrade(upgrade, selectingPlayer))
        {
            return;
        }

        selectingPlayer.DebugLogPlayerStatus("Buff Selected: " + upgrade.DisplayName, 0f);
        selectingPlayer = null;
        activeChoices = null;
        while (pendingPlayers.Count > 0)
        {
            KeyValuePair<PlayerController, int> nextReward = pendingPlayers.Dequeue();
            if (nextReward.Key != null && OfferLevelUpRewards(nextReward.Key, nextReward.Value))
            {
                return;
            }
        }

        selectionUI.Hide();
    }

    // Applies one upgrade to a player if the upgrade has not reached max level.
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

    // Picks weighted rarity variants without offering the same buff family twice.
    private List<UpgradeDefinition> PickUpgradeChoices()
    {
        List<UpgradeDefinition> candidates = GetAvailableUpgrades();
        List<UpgradeDefinition> choices = new List<UpgradeDefinition>();
        int choiceCount = Mathf.Clamp(choicesPerLevel, 1, Mathf.Max(1, candidates.Count));

        for (int i = 0; i < choiceCount && candidates.Count > 0; i++)
        {
            float totalWeight = 0f;
            foreach (UpgradeDefinition candidate in candidates)
            {
                totalWeight += GetRarityWeight(candidate.rarity);
            }

            int index = 0;
            if (totalWeight > 0f)
            {
                float roll = UnityEngine.Random.value * totalWeight;
                index = candidates.Count - 1;
                for (int candidateIndex = 0; candidateIndex < candidates.Count; candidateIndex++)
                {
                    roll -= GetRarityWeight(candidates[candidateIndex].rarity);
                    if (roll < 0f)
                    {
                        index = candidateIndex;
                        break;
                    }
                }
            }
            else
            {
                index = UnityEngine.Random.Range(0, candidates.Count);
            }

            UpgradeDefinition selected = candidates[index];
            choices.Add(selected);
            string selectedKey = GetUpgradeKey(selected);
            candidates.RemoveAll(candidate => GetUpgradeKey(candidate) == selectedKey);
        }

        return choices;
    }

    // Clamp inspector values as a safeguard against invalid serialized weights.
    private float GetRarityWeight(UpgradeRarity rarity)
    {
        switch (rarity)
        {
            case UpgradeRarity.Rare: return Mathf.Max(0f, rareWeight);
            case UpgradeRarity.Epic: return Mathf.Max(0f, epicWeight);
            case UpgradeRarity.Legendary: return Mathf.Max(0f, legendaryWeight);
            default: return Mathf.Max(0f, commonWeight);
        }
    }

    // Returns upgrades that still have remaining levels available.
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

    // Returns a stable tracking key for an upgrade definition.
    private string GetUpgradeKey(UpgradeDefinition upgrade)
    {
        if (!string.IsNullOrWhiteSpace(upgrade.upgradeId))
        {
            return upgrade.upgradeId;
        }

        return !string.IsNullOrWhiteSpace(upgrade.name) ? upgrade.name : upgrade.DisplayName;
    }

    // Formats upgrade choices for debug logging.
    private string FormatChoices(IReadOnlyList<UpgradeDefinition> choices)
    {
        List<string> names = new List<string>();
        foreach (UpgradeDefinition choice in choices)
        {
            names.Add(choice.DisplayName + " (" + choice.rarity + ")");
        }

        return string.Join(", ", names);
    }

    // Creates a small runtime upgrade pool so level-up rewards work before asset setup exists.
    private UpgradeDefinition[] CreatePrototypeUpgradePool()
    {
        UpgradeDefinition[] commonUpgrades = new UpgradeDefinition[]
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

        List<UpgradeDefinition> variants = new List<UpgradeDefinition>();
        foreach (UpgradeDefinition common in commonUpgrades)
        {
            variants.Add(common);
            variants.Add(CreateRarityVariant(common, UpgradeRarity.Rare, 1.5f));
            variants.Add(CreateRarityVariant(common, UpgradeRarity.Epic, 2f));
            variants.Add(CreateRarityVariant(common, UpgradeRarity.Legendary, 3f));
        }

        return variants.ToArray();
    }

    // Variants share upgradeId and maxLevel, so rarity does not bypass a buff family's level cap.
    private UpgradeDefinition CreateRarityVariant(UpgradeDefinition common, UpgradeRarity rarity, float strength)
    {
        UpgradeEffect[] effects = new UpgradeEffect[common.effects.Length];
        List<string> details = new List<string>();
        for (int i = 0; i < effects.Length; i++)
        {
            effects[i] = new UpgradeEffect(common.effects[i].effectType, common.effects[i].value * strength);
            details.Add(DescribeEffect(effects[i]));
        }

        UpgradeDefinition variant = CreateRuntimeUpgrade(common.upgradeId, common.DisplayName,
            string.Join("\n", details), rarity, common.maxLevel, effects);
        variant.name = common.upgradeId + "_" + rarity;
        return variant;
    }

    // Generate the description from the actual scaled effect so the UI displays the correct numbers.
    private string DescribeEffect(UpgradeEffect effect)
    {
        string percent = (effect.value * 100f).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "%";
        switch (effect.effectType)
        {
            case UpgradeEffectType.DamageMultiplier: return "Increase player damage by " + percent + ".";
            case UpgradeEffectType.FireCooldownReduction: return "Reduce bullet cooldown by " + percent + ".";
            case UpgradeEffectType.BombCooldownReduction: return "Reduce bomb cooldown by " + percent + ".";
            case UpgradeEffectType.DashCooldownReduction: return "Reduce dash cooldown by " + percent + ".";
            case UpgradeEffectType.MoveSpeedMultiplier: return "Increase movement speed by " + percent + ".";
            case UpgradeEffectType.DamageReduction: return "Reduce incoming damage by " + percent + ".";
            case UpgradeEffectType.PickupRadiusMultiplier: return "Increase pickup radius by " + percent + ".";
            case UpgradeEffectType.MaxHealth: return "Increase max health by " + effect.value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + ".";
            default: return effect.effectType + ": " + effect.value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    // Creates one runtime-only upgrade definition for the prototype pool.
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
