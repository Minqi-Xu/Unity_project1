using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Scene-independent checks runnable from Tools/Buffs/Run Smoke Tests or batchmode.
public static class BuffSmokeTests
{
    // Tests real catalog and stat implementations without changing the open scene.
    [MenuItem("Tools/Buffs/Run Smoke Tests")]
    public static void Run()
    {
        if (Application.isPlaying || UpgradeManager.Instance != null)
            throw new InvalidOperationException("Run these checks outside Play Mode without an active UpgradeManager.");
        GameObject root = new GameObject("BuffSmokeTests");
        root.SetActive(false);
        UnityEngine.Random.State randomState = UnityEngine.Random.state;
        float previousTimeScale = Time.timeScale;
        UpgradeManager manager = null;
        try
        {
            Time.timeScale = 1f;
            PlayerStats stats = root.AddComponent<PlayerStats>();
            PlayerHealth health = root.AddComponent<PlayerHealth>();
            PlayerController player = root.AddComponent<PlayerController>();
            typeof(PlayerController).GetProperty("Stats").SetValue(player, stats);
            health.maxHealth = 100f;
            health.currentHealth = 50f;
            manager = root.AddComponent<UpgradeManager>();
            Call(manager, "Awake");
            UpgradeDefinition[] pool = manager.availableUpgrades;
            Check(pool.Count(x => x.rarity == UpgradeRarity.Common) == 20, "Twenty common basics");
            Check(pool.Length == 94, "All basic and special rarity variants");
            foreach (var family in pool.Where(x => x.category != UpgradeCategory.Universal).GroupBy(x => x.upgradeId))
            {
                var variants = family.OrderBy(x => x.rarity).ToArray();
                Check(variants.Length == 4, "Four rarities per basic family");
                for (int i = 1; i < variants.Length; i++)
                    Check(variants[i].effects[0].value > variants[i - 1].effects[0].value, "Strictly stronger rarities");
            }
            UpgradeDefinition damage = pool.First(x => x.upgradeId == "damage_training" && x.rarity == UpgradeRarity.Common);
            Check(manager.ApplyUpgrade(damage, player), "Apply bullet buff");
            Near(stats.CalculateDamage(10f, 1f, true), 11f, "Bullet-only bonus");
            Near(stats.CalculateDamage(10f, 1f, false), 10f, "Bomb unaffected by bullet buff");
            stats.criticalChance = 1f;
            Near(stats.CalculateDamage(10f, 1f, true), 22f, "Guaranteed critical hit");
            stats.criticalChance = 0f;
            stats.lowHealthCriticalChance = 0.5f;
            Near(stats.GetCriticalChance(), 0.25f, "Missing-health critical chance");
            stats.levelUpHealing = 0.1f;
            stats.levelUpDamage = 0.02f;
            stats.OnLevelUp(health);
            Near(health.currentHealth, 60f, "Level-up healing");
            Near(stats.damageMultiplier, 1.02f, "Level-up damage growth");
            stats.globalCooldownMultiplier = 0.9f;
            Near(stats.GetBombCooldown(10f), 9f, "Global cooldown reduction");
            Near(stats.GetDashCooldown(5f), 4.5f, "Global dash cooldown reduction");
            stats.dashAttackSpeed = 0.3f;
            stats.moveSpeedMultiplier = 1.5f;
            stats.OnDashCompleted();
            Near(stats.GetFireCooldown(1f), 0.9f / 1.45f, "Movement-scaled dash attack speed");
            UpgradeDefinition commonPierce = pool.First(x => x.upgradeId == "piercing" && x.rarity == UpgradeRarity.Common);
            UpgradeDefinition legendaryPierce = pool.First(x => x.upgradeId == "piercing" && x.rarity == UpgradeRarity.Legendary);
            for (int i = 0; i < 3; i++) Check(manager.ApplyUpgrade(commonPierce, player), "Pierce stacking");
            Check(stats.GetPierceCount() == 3 && !manager.ApplyUpgrade(legendaryPierce, player), "Shared rarity level cap");
            UpgradeDefinition bundle = pool.First(x => x.upgradeId == "fortune_bundle" && x.rarity == UpgradeRarity.Rare);
            var levels = (System.Collections.Generic.Dictionary<string, int>)typeof(UpgradeManager)
                .GetField("upgradeLevels", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(manager);
            int before = levels.Values.Sum();
            Check(manager.ApplyUpgrade(bundle, player), "Bundle can be granted");
            Check(levels.Values.Sum() == before + 3, "Two bundle rewards plus bundle level");
            var candidates = (System.Collections.Generic.List<UpgradeDefinition>)Call(manager, "GetBundleCandidates", bundle);
            Check(candidates.All(x => x.rarity == UpgradeRarity.Epic && x.category != UpgradeCategory.Universal),
                "Bundle only offers next-tier basics");
            for (int i = 0; i < 100; i++)
            {
                var choices = (System.Collections.Generic.List<UpgradeDefinition>)Call(manager, "PickUpgradeChoices");
                Check(choices.Count == 3 && choices.Select(x => x.upgradeId).Distinct().Count() == 3,
                    "Three different families per offer");
                Check(choices.All(x => x.upgradeId != "piercing"), "Maxed families excluded");
            }
            stats.hitInvulnerability = 0.2f;
            health.TakeDamage(10f, player);
            Near(health.currentHealth, 50f, "First incoming hit");
            health.TakeDamage(10f, player);
            Near(health.currentHealth, 50f, "Post-hit immunity blocks repeated hit");
            foreach (UpgradeDefinition basic in pool.Where(x => x.category != UpgradeCategory.Universal))
                levels[basic.upgradeId] = basic.maxLevel;
            Check(!manager.ApplyUpgrade(bundle, player), "Exhausted bundle cannot consume a reward");
            health.currentHealth = 0f;
            health.Heal(100f);
            Near(health.currentHealth, 0f, "Healing cannot revive");
            Debug.Log("[BuffSmokeTests] PASS: catalog, scaling, combat stats, growth, cooldowns, caps, and bundles.");
        }
        finally
        {
            if (manager != null && manager.availableUpgrades != null)
                foreach (UpgradeDefinition definition in manager.availableUpgrades) UnityEngine.Object.DestroyImmediate(definition);
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Random.state = randomState;
            Time.timeScale = previousTimeScale;
        }
    }

    // Calls a private lifecycle or selection method without starting gameplay.
    private static object Call(object target, string method, params object[] args)
    {
        return target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
    }

    // Fails batch execution on a broken invariant instead of relying on log inspection.
    private static void Check(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException("Buff smoke test failed: " + label);
    }

    // Uses a small tolerance for floating-point stat arithmetic.
    private static void Near(float actual, float expected, string label)
    {
        Check(Mathf.Abs(actual - expected) < 0.0001f, label + " (" + actual + " != " + expected + ")");
    }
}
