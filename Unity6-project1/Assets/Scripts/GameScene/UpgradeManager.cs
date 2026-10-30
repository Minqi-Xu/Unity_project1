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
    public AutonomousOfferPolicy autonomousWeapons = new AutonomousOfferPolicy();
    private readonly Dictionary<PlayerController, int> autonomousMisses = new Dictionary<PlayerController, int>();
    public int GetAutonomousMisses(PlayerController player) => autonomousMisses.TryGetValue(player, out int value) ? value : 0;

    // Pending level-ups are resolved one at a time using a fresh upgrade pool.
    private struct PendingReward
    {
        public PlayerController player;
        public int level;
        public bool rare;
        public string title;
    }
    private readonly Queue<PendingReward> pendingPlayers = new Queue<PendingReward>();
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
        List<UpgradeDefinition> pool = new List<UpgradeDefinition>(availableUpgrades);
        foreach (UpgradeDefinition upgrade in WeaponUpgradeCatalog.Create())
            if (!pool.Exists(item => item != null && item.upgradeId == upgrade.upgradeId)) pool.Add(upgrade);
            else Destroy(upgrade);
        availableUpgrades = pool.ToArray();
    }

    // Builds level-up choices for a player and optionally auto-applies one during prototype mode.
    public bool TryOfferLevelUpRewards(PlayerController player)
    {
        return player != null && OfferLevelUpRewards(player, player.level);
    }

    // Capture the earned level so queued rewards show the correct transition after multi-level gains.
    private bool OfferLevelUpRewards(PlayerController player, int earnedLevel) => OfferRewards(player, earnedLevel, false);

    public bool TryOfferRareRewards(PlayerController player, int rewardIndex = 1, int rewardCount = 1) =>
        player != null && OfferRewards(player, player.level, true,
            $"Treasure Chest {rewardIndex}/{rewardCount} | Choose a Rare Upgrade");

    private bool OfferRewards(PlayerController player, int earnedLevel, bool rare, string rewardTitle = null)
    {
        if (player == null)
        {
            return false;
        }

        if (IsSelecting)
        {
            pendingPlayers.Enqueue(new PendingReward { player = player, level = earnedLevel, rare = rare, title = rewardTitle });
            return true;
        }

        List<UpgradeDefinition> choices = rare ? PickRareChoices(player) : PickLevelUpChoices(player);
        if (choices.Count == 0)
        {
            if (rare && RewardManager.Instance != null) RewardManager.Instance.GrantExhaustedChestReward(player);
            return false;
        }

        Debug.Log($"[Upgrade] Level up choices: {FormatChoices(choices)}");
        if (autoPickFirstChoiceForPrototype)
        {
            return ApplyUpgrade(choices[0], player);
        }

        selectingPlayer = player;
        activeChoices = choices;
        selectionUI.Show(choices, earnedLevel, SelectUpgrade, rewardTitle);
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
            PendingReward nextReward = pendingPlayers.Dequeue();
            if (nextReward.player != null && OfferRewards(nextReward.player, nextReward.level, nextReward.rare, nextReward.title) && IsSelecting)
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
        if (upgrade.grantedWeapon != null)
        {
            if (!WeaponInventory.Ensure(player).Acquire(upgrade.grantedWeapon)) return false;
        }
        if (upgrade.weaponSpecific)
        {
            PlayerWeaponSystem weapon = WeaponInventory.Ensure(player).Find(upgrade.targetWeapon);
            if (weapon == null) return false;
            weapon.ApplyBuff(upgrade.weaponEffect, upgrade.weaponValue);
        }

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        int bundleCount = GetBundleCount(upgrade);
        if (bundleCount > 0)
        {
            List<UpgradeDefinition> rewards = GetBundleCandidates(upgrade);
            if (rewards.Count < bundleCount) return false;
            for (int i = 0; i < bundleCount; i++)
            {
                int index = UnityEngine.Random.Range(0, rewards.Count);
                UpgradeDefinition reward = rewards[index];
                ApplyUpgrade(reward, player);
                rewards.RemoveAt(index);
                Debug.Log("[Fortune Bundle] Granted " + reward.DisplayName + " (" + reward.rarity + ")");
            }
        }
        stats.ApplyUpgrade(upgrade, health);
        upgradeLevels[upgradeKey] = currentLevel + 1;
        PlayerBuffCollection.Ensure(player).Record(upgrade, upgradeKey);

        Debug.Log($"[Upgrade] Applied {upgrade.DisplayName} ({upgradeLevels[upgradeKey]}/{upgrade.maxLevel})");
        return true;
    }

    // Picks weighted rarity variants without offering the same buff family twice.
    private List<UpgradeDefinition> PickUpgradeChoices()
    {
        return PickWeightedChoices(GetAvailableUpgrades().FindAll(item => !item.weaponSpecific && item.grantedWeapon == null), choicesPerLevel);
    }

    private List<UpgradeDefinition> PickLevelUpChoices(PlayerController player)
    {
        WeaponInventory inventory = WeaponInventory.Ensure(player);
        List<UpgradeDefinition> eligible = GetAvailableUpgrades();
        List<UpgradeDefinition> weapons = eligible.FindAll(item => item.grantedWeapon != null && inventory.CanAcquire(item.grantedWeapon));
        List<UpgradeDefinition> normal = eligible.FindAll(item => item.grantedWeapon == null &&
            (!item.weaponSpecific || inventory.Find(item.targetWeapon) != null) && IsUsefulForLoadout(item, inventory));
        int misses = GetAutonomousMisses(player);
        bool weaponOffered = weapons.Count > 0 && autonomousWeapons.ShouldOffer(misses, UnityEngine.Random.value);
        int count = Mathf.Max(1, choicesPerLevel);
        List<UpgradeDefinition> choices = PickWeightedChoices(normal, count - (weaponOffered ? 1 : 0));
        if (weaponOffered) choices.Insert(UnityEngine.Random.Range(0, choices.Count + 1), weapons[UnityEngine.Random.Range(0, weapons.Count)]);
        if (weapons.Count > 0 && choices.Count > 0) autonomousMisses[player] = weaponOffered ? 0 : misses + 1;
        return choices;
    }

    private List<UpgradeDefinition> PickRareChoices(PlayerController player)
    {
        WeaponInventory inventory = WeaponInventory.Ensure(player);
        return PickWeightedChoices(GetAvailableUpgrades().FindAll(item => item.rarity >= UpgradeRarity.Rare &&
            (item.grantedWeapon == null || inventory.CanAcquire(item.grantedWeapon)) &&
            (!item.weaponSpecific || inventory.Find(item.targetWeapon) != null) && IsUsefulForLoadout(item, inventory)), 2);
    }

    private static bool IsUsefulForLoadout(UpgradeDefinition upgrade, WeaponInventory inventory)
    {
        if (upgrade.effects == null) return true;
        foreach (UpgradeEffect effect in upgrade.effects)
        {
            if ((effect.effectType == UpgradeEffectType.PierceCount || effect.effectType == UpgradeEffectType.FinalPierceExplosion) &&
                inventory.Find(WeaponKind.Pistol) == null && inventory.Find(WeaponKind.Shotgun) == null &&
                inventory.Find(WeaponKind.Bow) == null && inventory.Find(WeaponKind.Turret) == null && inventory.Find(WeaponKind.Drone) == null)
                return false;
            if (effect.effectType == UpgradeEffectType.ProjectileCount)
            {
                PlayerWeaponSystem primary = inventory.GetComponent<PlayerWeaponSystem>();
                if (primary == null || primary.Definition == null || primary.Definition.kind == WeaponKind.Bomb || primary.Definition.kind == WeaponKind.Axe)
                    return false;
            }
        }
        return true;
    }

    private List<UpgradeDefinition> PickWeightedChoices(List<UpgradeDefinition> candidates, int count)
    {
        List<UpgradeDefinition> choices = new List<UpgradeDefinition>();
        int choiceCount = Mathf.Clamp(count, 0, candidates.Count);

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
            if ((upgrade.maxLevel <= 0 || currentLevel < upgrade.maxLevel)
                && (GetBundleCount(upgrade) == 0 || GetBundleCandidates(upgrade).Count >= GetBundleCount(upgrade)))
            {
                candidates.Add(upgrade);
            }
        }

        return candidates;
    }

    // Bundles are manager-owned effects, never recursive stat effects.
    private int GetBundleCount(UpgradeDefinition upgrade)
    {
        int count = 0;
        if (upgrade.effects != null)
            foreach (UpgradeEffect effect in upgrade.effects)
                if (effect.effectType == UpgradeEffectType.RandomHigherRarityBuffs)
                    count += Mathf.Max(0, Mathf.RoundToInt(effect.value));
        return count;
    }

    // Returns one eligible basic buff per family at exactly the next rarity, excluding bundles.
    private List<UpgradeDefinition> GetBundleCandidates(UpgradeDefinition bundle)
    {
        List<UpgradeDefinition> result = new List<UpgradeDefinition>();
        HashSet<string> keys = new HashSet<string>();
        if (availableUpgrades == null || bundle.rarity == UpgradeRarity.Legendary) return result;
        foreach (UpgradeDefinition candidate in availableUpgrades)
        {
            if (candidate == null || candidate.weaponSpecific || candidate.grantedWeapon != null || candidate.category == UpgradeCategory.Universal || GetBundleCount(candidate) > 0
                || (int)candidate.rarity != (int)bundle.rarity + 1) continue;
            string key = GetUpgradeKey(candidate);
            upgradeLevels.TryGetValue(key, out int level);
            if ((candidate.maxLevel <= 0 || level < candidate.maxLevel) && keys.Add(key)) result.Add(candidate);
        }
        return result;
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

    // Builds twenty basic families and rare-only universal or synergy upgrades.
    private UpgradeDefinition[] CreatePrototypeUpgradePool()
    {
        UpgradeDefinition[] basics =
        {
            Basic("damage_training", "Primary Weapon Damage", UpgradeCategory.Attack, UpgradeEffectType.PrimaryWeaponDamageMultiplier, 0.1f, 10),
            Basic("rapid_fire", "Primary Weapon Attack Speed", UpgradeCategory.Attack, UpgradeEffectType.PrimaryWeaponAttackSpeed, 0.08f),
            Basic("multishot", "Multishot", UpgradeCategory.Attack, UpgradeEffectType.ProjectileCount, 1f, 3),
            Basic("piercing", "Piercing Rounds", UpgradeCategory.Attack, UpgradeEffectType.PierceCount, 1f, 3),
            Basic("blast_radius", "Blast Radius", UpgradeCategory.Attack, UpgradeEffectType.ExplosionRadiusMultiplier, 0.15f),
            Basic("bomb_tech", "Bomb Tech", UpgradeCategory.Attack, UpgradeEffectType.BombCooldownReduction, 0.1f),
            Basic("critical_chance", "Precision", UpgradeCategory.Attack, UpgradeEffectType.CriticalChance, 0.05f),
            Basic("critical_damage", "Deadly Aim", UpgradeCategory.Attack, UpgradeEffectType.CriticalDamageMultiplier, 0.25f),
            Basic("vitality", "Vitality", UpgradeCategory.Survival, UpgradeEffectType.MaxHealth, 20f),
            Basic("armor_training", "Armor Training", UpgradeCategory.Survival, UpgradeEffectType.DamageReduction, 0.08f),
            Basic("regeneration", "Regeneration", UpgradeCategory.Survival, UpgradeEffectType.Regeneration, 1f),
            Basic("second_wind", "Second Wind", UpgradeCategory.Survival, UpgradeEffectType.HitInvulnerability, 0.2f),
            Basic("battle_boots", "Battle Boots", UpgradeCategory.Mobility, UpgradeEffectType.MoveSpeedMultiplier, 0.08f),
            Basic("dash_practice", "Dash Practice", UpgradeCategory.Mobility, UpgradeEffectType.DashCooldownReduction, 0.1f),
            Basic("long_dash", "Long Dash", UpgradeCategory.Mobility, UpgradeEffectType.DashSpeedMultiplier, 0.15f),
            Basic("magnet", "Magnet", UpgradeCategory.Mobility, UpgradeEffectType.PickupRadiusMultiplier, 0.2f),
            Basic("scholar", "Scholar", UpgradeCategory.Growth, UpgradeEffectType.ExperienceGainMultiplier, 0.15f),
            Basic("swift_pickups", "Swift Pickups", UpgradeCategory.Growth, UpgradeEffectType.PickupSpeedMultiplier, 0.2f),
            Basic("renewal", "Renewal", UpgradeCategory.Growth, UpgradeEffectType.LevelUpHealing, 0.1f),
            Basic("potential", "Potential", UpgradeCategory.Growth, UpgradeEffectType.LevelUpDamage, 0.02f)
        };
        List<UpgradeDefinition> pool = new List<UpgradeDefinition>();
        foreach (UpgradeDefinition common in basics)
        {
            pool.Add(common);
            pool.Add(CreateRarityVariant(common, UpgradeRarity.Rare, 1.5f));
            pool.Add(CreateRarityVariant(common, UpgradeRarity.Epic, 2f));
            pool.Add(CreateRarityVariant(common, UpgradeRarity.Legendary, 3f));
        }
        AddSpecial(pool, "power", "Universal Power", UpgradeRarity.Rare, 5,
            new UpgradeEffect(UpgradeEffectType.DamageMultiplier, 0.15f));
        AddSpecial(pool, "haste", "Universal Haste", UpgradeRarity.Rare, 5,
            new UpgradeEffect(UpgradeEffectType.GlobalCooldownReduction, 0.1f));
        AddSpecial(pool, "piercing_blast", "Piercing Blast", UpgradeRarity.Epic, 1,
            new UpgradeEffect(UpgradeEffectType.PierceCount, 1f),
            new UpgradeEffect(UpgradeEffectType.FinalPierceExplosion, 1f));
        AddSpecial(pool, "dash_frenzy", "Dash Frenzy", UpgradeRarity.Epic, 1,
            new UpgradeEffect(UpgradeEffectType.MoveSpeedMultiplier, 0.16f),
            new UpgradeEffect(UpgradeEffectType.DashAttackSpeed, 0.3f));
        AddSpecial(pool, "desperate_precision", "Desperate Precision", UpgradeRarity.Epic, 1,
            new UpgradeEffect(UpgradeEffectType.LowHealthCriticalChance, 0.5f));
        foreach (UpgradeRarity rarity in new[] { UpgradeRarity.Rare, UpgradeRarity.Epic })
        {
            UpgradeDefinition bundle = CreateRuntimeUpgrade("fortune_bundle", "Fortune Bundle",
                "Immediately gain 2 different " + ((UpgradeRarity)((int)rarity + 1)) +
                " basic buffs. Both consume their normal family levels.",
                rarity, 3, new UpgradeEffect(UpgradeEffectType.RandomHigherRarityBuffs, 2f));
            bundle.category = UpgradeCategory.Universal;
            pool.Add(bundle);
        }
        AddSpecial(pool, "vanguard_provisions", "Vanguard Provisions", UpgradeRarity.Rare, 3,
            new UpgradeEffect(UpgradeEffectType.MaxHealth, 20f), new UpgradeEffect(UpgradeEffectType.Regeneration, .5f));
        AddSpecial(pool, "mobile_gunnery", "Mobile Gunnery", UpgradeRarity.Rare, 3,
            new UpgradeEffect(UpgradeEffectType.PrimaryWeaponDamageMultiplier, .08f), new UpgradeEffect(UpgradeEffectType.MoveSpeedMultiplier, .06f));
        AddSpecial(pool, "demolition_kit", "Demolition Kit", UpgradeRarity.Rare, 3,
            new UpgradeEffect(UpgradeEffectType.ExplosionRadiusMultiplier, .15f), new UpgradeEffect(UpgradeEffectType.BombCooldownReduction, .08f));
        AddSpecial(pool, "survivor_instinct", "Survivor Instinct", UpgradeRarity.Rare, 3,
            new UpgradeEffect(UpgradeEffectType.DamageReduction, .04f), new UpgradeEffect(UpgradeEffectType.HitInvulnerability, .15f));
        AddSpecial(pool, "field_scholar", "Field Scholar", UpgradeRarity.Rare, 3,
            new UpgradeEffect(UpgradeEffectType.ExperienceGainMultiplier, .1f), new UpgradeEffect(UpgradeEffectType.PickupRadiusMultiplier, .15f));
        AddSpecial(pool, "salvage_engine", "Salvage Engine", UpgradeRarity.Rare, 3,
            new UpgradeEffect(UpgradeEffectType.PickupSpeedMultiplier, .2f), new UpgradeEffect(UpgradeEffectType.PickupRadiusMultiplier, .15f));
        AddSpecial(pool, "renewing_potential", "Renewing Potential", UpgradeRarity.Rare, 3,
            new UpgradeEffect(UpgradeEffectType.LevelUpHealing, .05f), new UpgradeEffect(UpgradeEffectType.LevelUpDamage, .01f));
        AddSpecial(pool, "dash_logistics", "Dash Logistics", UpgradeRarity.Rare, 3,
            new UpgradeEffect(UpgradeEffectType.DashCooldownReduction, .08f), new UpgradeEffect(UpgradeEffectType.DashSpeedMultiplier, .1f));
        AddSpecial(pool, "battle_medic", "Battle Medic", UpgradeRarity.Epic, 2,
            new UpgradeEffect(UpgradeEffectType.MaxHealth, 30f), new UpgradeEffect(UpgradeEffectType.Regeneration, 1f),
            new UpgradeEffect(UpgradeEffectType.LevelUpHealing, .05f));
        AddSpecial(pool, "critical_engine", "Critical Engine", UpgradeRarity.Epic, 2,
            new UpgradeEffect(UpgradeEffectType.CriticalChance, .05f), new UpgradeEffect(UpgradeEffectType.CriticalDamageMultiplier, .3f));
        AddSpecial(pool, "arcane_overdrive", "Arcane Overdrive", UpgradeRarity.Epic, 2,
            new UpgradeEffect(UpgradeEffectType.GlobalCooldownReduction, .06f), new UpgradeEffect(UpgradeEffectType.DamageMultiplier, .1f));
        AddSpecial(pool, "growth_core", "Growth Core", UpgradeRarity.Epic, 2,
            new UpgradeEffect(UpgradeEffectType.ExperienceGainMultiplier, .15f), new UpgradeEffect(UpgradeEffectType.LevelUpDamage, .015f));
        return pool.ToArray();
    }

    // Creates a common basic buff with its description derived from its actual effect.
    private UpgradeDefinition Basic(string id, string title, UpgradeCategory category,
        UpgradeEffectType type, float value, int maxLevel = 5)
    {
        UpgradeEffect effect = new UpgradeEffect(type, value);
        UpgradeDefinition buff = CreateRuntimeUpgrade(id, title, DescribeEffect(effect), UpgradeRarity.Common, maxLevel, effect);
        buff.category = category;
        return buff;
    }

    // Adds a rare-only family and progressively stronger variants with one shared level cap.
    private void AddSpecial(List<UpgradeDefinition> pool, string id, string title, UpgradeRarity minimum,
        int maxLevel, params UpgradeEffect[] effects)
    {
        List<string> descriptions = new List<string>();
        foreach (UpgradeEffect effect in effects) descriptions.Add(DescribeEffect(effect));
        UpgradeDefinition buff = CreateRuntimeUpgrade(id, title, string.Join("\n", descriptions), minimum, maxLevel, effects);
        buff.category = UpgradeCategory.Universal;
        pool.Add(buff);
        for (int tier = (int)minimum + 1; tier <= (int)UpgradeRarity.Legendary; tier++)
            pool.Add(CreateRarityVariant(buff, (UpgradeRarity)tier, 1f + 0.5f * (tier - (int)minimum)));
    }

    // Integer effects scale in whole steps; all variants share their family's level cap.
    private UpgradeDefinition CreateRarityVariant(UpgradeDefinition common, UpgradeRarity rarity, float strength)
    {
        UpgradeEffect[] effects = new UpgradeEffect[common.effects.Length];
        List<string> details = new List<string>();
        for (int i = 0; i < effects.Length; i++)
        {
            UpgradeEffect source = common.effects[i];
            bool count = source.effectType == UpgradeEffectType.ProjectileCount || source.effectType == UpgradeEffectType.PierceCount;
            float value = count ? source.value + ((int)rarity - (int)common.rarity) : source.value * strength;
            effects[i] = new UpgradeEffect(source.effectType, value);
            details.Add(DescribeEffect(effects[i]));
        }
        UpgradeDefinition variant = CreateRuntimeUpgrade(common.upgradeId, common.DisplayName,
            string.Join("\n", details), rarity, common.maxLevel, effects);
        variant.category = common.category;
        variant.name = common.upgradeId + "_" + rarity;
        return variant;
    }

    // Descriptions distinguish additive stat bonuses from multiplicative cooldown reductions.
    private string DescribeEffect(UpgradeEffect effect)
    {
        string value = effect.value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        string percent = (effect.value * 100f).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "%";
        switch (effect.effectType)
        {
            case UpgradeEffectType.PrimaryWeaponDamageMultiplier: return "Primary weapon damage +" + percent + "; excludes auxiliary weapons and RMB bombs.";
            case UpgradeEffectType.DamageMultiplier: return "All damage bonus +" + percent + ".";
            case UpgradeEffectType.PrimaryWeaponAttackSpeed: return "Primary weapon attack speed +" + percent + "; cooldown is divided by attack-speed multiplier.";
            case UpgradeEffectType.BombCooldownReduction: return "Bomb cooldown -" + percent + " (multiplicative).";
            case UpgradeEffectType.DashCooldownReduction: return "Dash cooldown -" + percent + " (multiplicative).";
            case UpgradeEffectType.GlobalCooldownReduction: return "All weapon and dash cooldowns -" + percent + ".";
            case UpgradeEffectType.MoveSpeedMultiplier: return "Movement speed bonus +" + percent + ".";
            case UpgradeEffectType.DashSpeedMultiplier: return "Dash distance bonus +" + percent + " via dash speed.";
            case UpgradeEffectType.DamageReduction: return "Damage reduction +" + percent + " (90% cap).";
            case UpgradeEffectType.PickupRadiusMultiplier: return "Pickup radius bonus +" + percent + ".";
            case UpgradeEffectType.PickupSpeedMultiplier: return "Pickup attraction speed bonus +" + percent + ".";
            case UpgradeEffectType.ExperienceGainMultiplier: return "Experience gain bonus +" + percent + ".";
            case UpgradeEffectType.ExplosionRadiusMultiplier: return "Explosion radius bonus +" + percent + ".";
            case UpgradeEffectType.MaxHealth: return "Max health +" + value + "; heal the same amount.";
            case UpgradeEffectType.Heal: return "Restore " + value + " health.";
            case UpgradeEffectType.ProjectileCount: return "Primary weapon projectiles +" + value + "; excludes auxiliary weapons.";
            case UpgradeEffectType.PierceCount: return "Additional targets pierced +" + value + ".";
            case UpgradeEffectType.CriticalChance: return "Critical chance +" + percent + " (100% cap).";
            case UpgradeEffectType.CriticalDamageMultiplier: return "Critical damage multiplier +" + value + "x.";
            case UpgradeEffectType.Regeneration: return "Restore " + value + " health every 5 seconds.";
            case UpgradeEffectType.HitInvulnerability: return "Post-hit invulnerability +" + value + " seconds.";
            case UpgradeEffectType.LevelUpHealing: return "Future level-ups restore +" + percent + " of max health.";
            case UpgradeEffectType.LevelUpDamage: return "Future level-ups add +" + percent + " all damage bonus.";
            case UpgradeEffectType.FinalPierceExplosion: return "Final projectile hit explodes in radius 2 for " + percent + " bullet damage.";
            case UpgradeEffectType.DashAttackSpeed: return "After dashing: +" + percent + " primary weapon attack speed for 3s, multiplied by move-speed multiplier.";
            case UpgradeEffectType.LowHealthCriticalChance: return "Gain up to +" + percent + " critical chance, proportional to missing health.";
            default: return effect.effectType + ": " + value;
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
