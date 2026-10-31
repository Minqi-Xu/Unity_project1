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
            stats.projectileCount = 5;
            Check(primary.ProjectileCount == primary.Definition.projectiles + 4, "Basic multishot applies to the default weapon");
            foreach (WeaponKind kind in new[] { WeaponKind.Drone, WeaponKind.Creature, WeaponKind.FireOrb, WeaponKind.Lightning, WeaponKind.Melee })
            {
                PlayerWeaponSystem auxiliary = inventory.Find(kind);
                Check(auxiliary.ProjectileCount == auxiliary.Definition.projectiles, "Basic multishot cannot increase auxiliary projectiles: " + kind);
                Check(auxiliary.SummonCount == auxiliary.Definition.summonCount, "Basic multishot cannot increase auxiliary summons: " + kind);
                Check(auxiliary.MeleePulseCount == 1, "Basic multishot cannot increase auxiliary pulses: " + kind);
                WeaponBuffEffect countEffect = kind == WeaponKind.Melee ? WeaponBuffEffect.MeleeCombo :
                    kind == WeaponKind.Creature ? WeaponBuffEffect.SummonCount : WeaponBuffEffect.Count;
                UpgradeDefinition countBuff = weaponBuffs.Single(x => x.weaponSpecific && x.targetWeapon == kind && x.weaponEffect == countEffect);
                Check(countBuff.rarity >= UpgradeRarity.Epic, "Auxiliary count upgrades require Epic or higher: " + kind);
                auxiliary.ApplyBuff(countBuff.weaponEffect, countBuff.weaponValue);
                int upgradedCount = kind == WeaponKind.Melee ? auxiliary.MeleePulseCount :
                    kind == WeaponKind.Creature ? auxiliary.SummonCount : auxiliary.ProjectileCount;
                int originalCount = kind == WeaponKind.Melee ? 1 : kind == WeaponKind.Creature ? auxiliary.Definition.summonCount : auxiliary.Definition.projectiles;
                Check(upgradedCount == originalCount + 1, "Dedicated count upgrade only affects its weapon: " + kind);
                if (kind == WeaponKind.Drone)
                {
                    Check(auxiliary.SummonCount == auxiliary.Definition.summonCount, "Drone multishot does not add drones");
                    UpgradeDefinition wing = weaponBuffs.Single(x => x.weaponSpecific && x.targetWeapon == kind && x.weaponEffect == WeaponBuffEffect.SummonCount);
                    Check(wing.rarity >= UpgradeRarity.Epic, "Additional drones require Epic or higher");
                    auxiliary.ApplyBuff(wing.weaponEffect, wing.weaponValue);
                    Check(auxiliary.SummonCount == auxiliary.Definition.summonCount + 1 && auxiliary.ProjectileCount == auxiliary.Definition.projectiles + 1,
                        "Drone population and volley upgrades stay independent");
                }
                Check(primary.ProjectileCount == primary.Definition.projectiles + 4, "Auxiliary count upgrade leaves default weapon unchanged");
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
            stats.projectileCount = 1;
            Near(stats.CalculateDamage(100f, 1f, false), 100f, "RMB bomb unaffected by primary damage");
            primary.Configure(player, Resources.Load<WeaponDefinition>("Weapons/Shotgun"));
            enemy.currentHealth = 5000f;
            target.transform.position = Vector2.right;
            WeaponAttackContext volley = new WeaponAttackContext();
            for (int pellet = 0; pellet < 6; pellet++)
                primary.Hit(enemy, 7f, Vector2.zero, context: volley, projectileHit: true);
            Near(target.transform.position.x, 1.35f, "Shotgun knockback applies once per enemy per volley");
            primary.Hit(enemy, 7f, Vector2.zero, context: new WeaponAttackContext(), projectileHit: true);
            Near(target.transform.position.x, 1.7f, "A new shotgun volley can push again");
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
