using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class WeaponRewardSmokeTests
{
    [MenuItem("Tools/Weapons/Run Reward Smoke Tests")]
    public static void Run()
    {
        if (Application.isPlaying || UpgradeManager.Instance != null)
            throw new InvalidOperationException("Run outside Play Mode without an active UpgradeManager.");
        GameObject root = new GameObject("WeaponRewardChecks");
        root.SetActive(false);
        UnityEngine.Random.State random = UnityEngine.Random.state;
        UpgradeManager manager = null;
        try
        {
            PlayerController player = root.AddComponent<PlayerController>();
            PlayerStats stats = root.AddComponent<PlayerStats>();
            PlayerHealth health = root.AddComponent<PlayerHealth>();
            health.currentHealth = health.maxHealth = 100f;
            typeof(PlayerController).GetProperty("Stats").SetValue(player, stats);
            WeaponDefinition pistol = Resources.Load<WeaponDefinition>("Weapons/Pistol");
            PlayerWeaponSystem primary = root.AddComponent<PlayerWeaponSystem>();
            primary.Configure(player, pistol);
            WeaponInventory inventory = WeaponInventory.Ensure(player);
            manager = root.AddComponent<UpgradeManager>();
            typeof(UpgradeManager).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(manager, null);
            Check(manager.availableUpgrades.Count(x => x.grantedWeapon != null) == 4, "Four auxiliary unlocks");
            Check(manager.availableUpgrades.Count(x => x.weaponSpecific) >= 40, "Weapon-specific buff catalog");
            AutonomousOfferPolicy policy = new AutonomousOfferPolicy();
            Near(policy.Chance(0), .4f, "Initial forty percent");
            Near(policy.Chance(3), .7f, "Increasing probability");
            Check(policy.ShouldOffer(4, 1f), "Fifth offer is guaranteed even at maximal roll");
            manager.autonomousWeapons.initialChance = manager.autonomousWeapons.chancePerMiss = 0f;
            for (int i = 0; i < 4; i++)
            {
                List<UpgradeDefinition> offer = Offer(manager, player);
                Check(offer.Count == 3 && offer.All(x => x.grantedWeapon == null), "Four forced misses");
                Check(manager.GetAutonomousMisses(player) == i + 1, "Pity counts offers");
                Check(offer.All(x => !x.weaponSpecific || x.targetWeapon == WeaponKind.Pistol), "Only equipped weapon buffs");
            }
            List<UpgradeDefinition> guaranteed = Offer(manager, player);
            Check(guaranteed.Count == 3 && guaranteed.Count(x => x.grantedWeapon != null) == 1, "Guarantee reserves one of three options");
            Check(manager.GetAutonomousMisses(player) == 0 && inventory.AuxiliaryCount == 0, "Offering resets pity without auto-equipping");
            UpgradeDefinition drone = manager.availableUpgrades.First(x => x.grantedWeapon != null && x.grantedWeapon.kind == WeaponKind.Drone);
            Check(manager.ApplyUpgrade(drone, player), "Acquire drone through normal reward path");
            Check(primary.Definition == pistol && inventory.Find(WeaponKind.Drone) != null, "Primary retained alongside auxiliary");
            primary.CharacterDamageMultiplier = 1.25f;
            primary.CharacterCooldownMultiplier = .8f;
            Near(inventory.Find(WeaponKind.Drone).CharacterDamageMultiplier, 1f, "Primary character damage bonus does not leak into auxiliaries");
            Near(inventory.Find(WeaponKind.Drone).Cooldown, .6f, "Primary character fire rate does not leak into auxiliaries");
            Check(!manager.ApplyUpgrade(drone, player) && inventory.AuxiliaryCount == 1, "No duplicate weapon unlock");
            UpgradeDefinition bow = manager.availableUpgrades.First(x => x.weaponSpecific && x.targetWeapon == WeaponKind.Bow);
            Check(!manager.ApplyUpgrade(bow, player), "Cannot apply buff for unequipped weapon");
            foreach (WeaponKind kind in new[] { WeaponKind.Creature, WeaponKind.FireOrb })
                Check(manager.ApplyUpgrade(manager.availableUpgrades.First(x => x.grantedWeapon != null && x.grantedWeapon.kind == kind), player), "Fill auxiliary slots");
            UpgradeDefinition lightning = manager.availableUpgrades.First(x => x.grantedWeapon != null && x.grantedWeapon.kind == WeaponKind.Lightning);
            Check(!manager.ApplyUpgrade(lightning, player), "Capacity cannot overflow");
            PlayerWeaponSystem fire = inventory.Find(WeaponKind.FireOrb);
            float radius = fire.Definition.radius;
            UpgradeDefinition fireRadius = manager.availableUpgrades.First(x => x.weaponSpecific && x.targetWeapon == WeaponKind.FireOrb && x.weaponEffect == WeaponBuffEffect.Radius);
            Check(manager.ApplyUpgrade(fireRadius, player), "Weapon buff through manager");
            Near(fire.Radius, radius * 1.15f, "Weapon instance receives buff");
            Near(fire.Definition.radius, radius, "Shared asset remains unchanged");
            for (int i = 0; i < 50; i++)
            {
                List<UpgradeDefinition> offer = Offer(manager, player);
                Check(offer.Count == 3 && offer.Select(x => x.upgradeId).Distinct().Count() == 3, "Unique three-option offers");
                Check(offer.All(x => x.grantedWeapon == null), "No weapon drops when slots are full");
                Check(offer.All(x => !x.weaponSpecific || inventory.Find(x.targetWeapon) != null), "Only owned weapon upgrades after unlocks");
            }
            Debug.Log("[WeaponRewardSmokeTests] PASS: pity, eligibility, parallel weapons, caps, duplicates, instance-only modifiers.");
        }
        finally
        {
            if (manager != null && manager.availableUpgrades != null)
                foreach (UpgradeDefinition buff in manager.availableUpgrades) UnityEngine.Object.DestroyImmediate(buff);
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Random.state = random;
        }
    }

    private static List<UpgradeDefinition> Offer(UpgradeManager manager, PlayerController player) =>
        (List<UpgradeDefinition>)typeof(UpgradeManager).GetMethod("PickLevelUpChoices", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(manager, new object[] { player });
    private static void Check(bool condition, string label) { if (!condition) throw new InvalidOperationException(label); }
    private static void Near(float actual, float expected, string label) => Check(Mathf.Abs(actual - expected) < .0001f, label);
}
