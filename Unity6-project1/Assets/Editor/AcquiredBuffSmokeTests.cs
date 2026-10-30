using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class AcquiredBuffSmokeTests
{
    [MenuItem("Tools/Buffs/Run Acquired List Smoke Tests")]
    public static void Run()
    {
        if (Application.isPlaying || UpgradeManager.Instance != null)
            throw new InvalidOperationException("Run outside Play Mode without an active UpgradeManager.");
        GameObject root = new GameObject("AcquiredBuffChecks");
        root.SetActive(false);
        GameObject other = new GameObject("EmptyBuffCollection");
        other.SetActive(false);
        UnityEngine.Random.State random = UnityEngine.Random.state;
        UpgradeManager manager = null;
        try
        {
            PlayerController player = root.AddComponent<PlayerController>();
            PlayerStats stats = root.AddComponent<PlayerStats>();
            PlayerHealth health = root.AddComponent<PlayerHealth>();
            health.currentHealth = health.maxHealth = 100f;
            typeof(PlayerController).GetProperty("Stats").SetValue(player, stats);
            manager = root.AddComponent<UpgradeManager>();
            typeof(UpgradeManager).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(manager, null);
            PlayerBuffCollection collection = PlayerBuffCollection.Ensure(player);
            Check(collection.Entries.Count == 0, "New run starts empty");
            UpgradeDefinition common = manager.availableUpgrades.First(x => x.upgradeId == "damage_training" && x.rarity == UpgradeRarity.Common);
            UpgradeDefinition rare = manager.availableUpgrades.First(x => x.upgradeId == "damage_training" && x.rarity == UpgradeRarity.Rare);
            Check(manager.ApplyUpgrade(common, player) && manager.ApplyUpgrade(common, player) && manager.ApplyUpgrade(rare, player), "Apply mixed rarity stacks");
            Check(collection.Entries.Count == 1, "One row per buff family");
            PlayerBuffCollection.Entry entry = collection.Entries[0];
            Check(entry.Level == 3 && entry.MaxLevel == 10 && entry.Name == common.DisplayName, "Name and acquired level");
            Check(entry.Stacks.Count == 2 && entry.Stacks.Single(x => x.Rarity == UpgradeRarity.Common).Count == 2 &&
                entry.Stacks.Single(x => x.Rarity == UpgradeRarity.Rare).Count == 1, "Exact rarity breakdown");
            Check(entry.Details().Contains(common.description) && entry.Details().Contains(rare.description), "Hover includes both actual effects");
            for (int i = 3; i < 10; i++) Check(manager.ApplyUpgrade(common, player), "Reach cap");
            Check(!manager.ApplyUpgrade(rare, player) && entry.Level == 10, "Rejected rewards never enter the list");
            string snapshot = entry.Stacks[0].Description;
            string description = common.description;
            common.description = "Changed after acquisition";
            Check(entry.Stacks[0].Description == snapshot, "Descriptions are snapshots, not mutable asset references");
            common.description = description;
            int picksBefore = collection.Entries.Sum(x => x.Level);
            UpgradeDefinition bundle = manager.availableUpgrades.First(x => x.upgradeId == "fortune_bundle" && x.rarity == UpgradeRarity.Rare);
            Check(manager.ApplyUpgrade(bundle, player), "Apply reward bundle");
            Check(collection.Entries.Sum(x => x.Level) == picksBefore + 3, "Bundle and both granted buffs recorded");
            Check(other.AddComponent<PlayerBuffCollection>().Entries.Count == 0, "Acquisitions cannot leak to a fresh player");
            Debug.Log("[AcquiredBuffSmokeTests] PASS: empty run, names, mixed stacks, caps, snapshot details, bundle rewards, isolation.");
        }
        finally
        {
            if (manager != null && manager.availableUpgrades != null)
                foreach (UpgradeDefinition upgrade in manager.availableUpgrades) UnityEngine.Object.DestroyImmediate(upgrade);
            UnityEngine.Object.DestroyImmediate(other);
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Random.state = random;
        }
    }
    private static void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); }
}
