using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class WeaponBalanceSmokeTests
{
    [MenuItem("Tools/Weapons/Run Balance Smoke Tests")]
    public static void Run()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Run outside Play Mode.");
        GameObject root = new GameObject("WeaponBalanceChecks");
        root.SetActive(false);
        GameObject target = new GameObject("BalanceTarget");
        UpgradeDefinition primaryBuff = ScriptableObject.CreateInstance<UpgradeDefinition>();
        UpgradeDefinition[] weaponBuffs = WeaponUpgradeCatalog.Create();
        try
        {
            PlayerController player = root.AddComponent<PlayerController>();
            PlayerStats stats = root.AddComponent<PlayerStats>();
            typeof(PlayerController).GetProperty("Stats").SetValue(player, stats);
            PlayerHealth health = root.AddComponent<PlayerHealth>();
            health.currentHealth = health.maxHealth = 100f;
            PlayerWeaponSystem primary = root.AddComponent<PlayerWeaponSystem>();
            primary.Configure(player, Resources.Load<WeaponDefinition>("Weapons/Pistol"));
            WeaponInventory inventory = WeaponInventory.Ensure(player);
            inventory.auxiliarySlots = 5;
            foreach (WeaponKind kind in new[] { WeaponKind.Drone, WeaponKind.Creature, WeaponKind.FireOrb, WeaponKind.Lightning, WeaponKind.Melee })
                Check(inventory.Acquire(weaponBuffs.First(x => x.grantedWeapon != null && x.grantedWeapon.kind == kind).grantedWeapon), "Acquire auxiliary");
            primaryBuff.effects = new[] { new UpgradeEffect(UpgradeEffectType.PrimaryWeaponDamageMultiplier, .1f),
                new UpgradeEffect(UpgradeEffectType.PrimaryWeaponAttackSpeed, .08f) };
            stats.ApplyUpgrade(primaryBuff, health);
            Near(primary.Cooldown, .25f / 1.08f, "Primary attack speed is a rate multiplier");
            Enemy enemy = target.AddComponent<Enemy>();
            target.AddComponent<BoxCollider2D>().size = Vector2.one * .05f;
            GameObject extraCollider = new GameObject("SecondCollider");
            extraCollider.transform.SetParent(target.transform, false);
            extraCollider.AddComponent<BoxCollider2D>().size = Vector2.one * .05f;
            enemy.currentHealth = 5000f;
            primary.Hit(enemy, 8f, Vector2.zero);
            Near(5000f - enemy.currentHealth, 8.8f, "Primary damage applies");
            foreach (WeaponKind kind in new[] { WeaponKind.Drone, WeaponKind.Creature, WeaponKind.FireOrb, WeaponKind.Lightning, WeaponKind.Melee })
            {
                PlayerWeaponSystem auxiliary = inventory.Find(kind);
                Near(auxiliary.Cooldown, auxiliary.Definition.cooldown, "Primary speed cannot leak into " + kind);
                enemy.currentHealth = 5000f;
                auxiliary.Hit(enemy, auxiliary.Definition.damage, Vector2.zero);
                Near(5000f - enemy.currentHealth, auxiliary.Definition.damage, "Primary damage cannot leak into " + kind);
                UpgradeDefinition damage = weaponBuffs.Single(x => x.weaponSpecific && x.targetWeapon == kind && x.weaponEffect == WeaponBuffEffect.Damage);
                auxiliary.ApplyBuff(damage.weaponEffect, damage.weaponValue);
                enemy.currentHealth = 5000f;
                auxiliary.Hit(enemy, auxiliary.Definition.damage, Vector2.zero);
                Near(5000f - enemy.currentHealth, auxiliary.Definition.damage * 1.2f, "Dedicated auxiliary damage applies");
            }
            Near(stats.CalculateDamage(100f, 1f, false), 100f, "RMB bomb unaffected by primary damage");
            primary.Configure(player, Resources.Load<WeaponDefinition>("Weapons/BombLauncher"));
            Near(primary.Definition.radius, 1.8f, "Reduced launcher radius");
            Near(primary.Cooldown, 2.8f / 1.08f, "Launcher benefits from primary attack speed");
            float radius = primary.Definition.radius;
            float[] fractions = { 0f, .35f, .675f, 1f, 1.01f };
            float[] scales = { 1f, 1f, .625f, .25f, 0f };
            for (int i = 0; i < fractions.Length; i++)
            {
                target.transform.position = Vector2.right * radius * fractions[i];
                enemy.currentHealth = 5000f;
                Physics2D.SyncTransforms();
                primary.DamageArea(Vector2.zero, radius, 40f, null, false, false, true);
                Near(5000f - enemy.currentHealth, 44f * scales[i], "Launcher falloff with multiple colliders at " + fractions[i]);
            }
            Check((int)UpgradeEffectType.PrimaryWeaponAttackSpeed == 3 && (int)UpgradeEffectType.PrimaryWeaponDamageMultiplier == 11,
                "Serialized enum IDs preserved");
            Debug.Log("[WeaponBalanceSmokeTests] PASS: primary/auxiliary isolation, dedicated damage, launcher speed and spatial falloff.");
        }
        finally
        {
            foreach (UpgradeDefinition buff in weaponBuffs) UnityEngine.Object.DestroyImmediate(buff);
            UnityEngine.Object.DestroyImmediate(primaryBuff);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(root);
        }
    }
    private static void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); }
    private static void Near(float actual, float expected, string label) => Check(Mathf.Abs(actual - expected) < .001f, label);
}
