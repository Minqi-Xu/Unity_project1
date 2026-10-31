using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class MeleeSmokeTests
{
    [MenuItem("Tools/Weapons/Run Melee Smoke Tests")]
    public static void Run()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Run outside Play Mode.");
        GameObject root = new GameObject("MeleeChecks");
        root.SetActive(false);
        GameObject target = new GameObject("MeleeTarget");
        UpgradeDefinition[] catalog = WeaponUpgradeCatalog.Create();
        float previousTimeScale = Time.timeScale;
        try
        {
            Time.timeScale = 1f;
            PlayerController player = root.AddComponent<PlayerController>();
            PlayerStats stats = root.AddComponent<PlayerStats>();
            typeof(PlayerController).GetProperty("Stats").SetValue(player, stats);
            PlayerHealth health = root.AddComponent<PlayerHealth>();
            health.currentHealth = health.maxHealth = 100f;
            PlayerWeaponSystem axe = root.AddComponent<PlayerWeaponSystem>();
            axe.Configure(player, Resources.Load<WeaponDefinition>("Weapons/Axe"));
            Check(!axe.Definition.Automatic, "Axe remains a manual weapon");
            Near(axe.Definition.recovery, .5f, "Axe recovery is half a second");
            Near(axe.Definition.cooldown, axe.Definition.windup + axe.Definition.swingDuration + axe.Definition.recovery,
                "Attack interval matches the complete swing cycle");
            Enemy enemy = target.AddComponent<Enemy>();
            target.AddComponent<BoxCollider2D>().size = Vector2.one * .1f;
            GameObject second = new GameObject("ExtraCollider");
            second.transform.SetParent(target.transform, false);
            second.AddComponent<BoxCollider2D>().size = Vector2.one * .1f;
            foreach (Vector2 position in new[] { Vector2.right, Vector2.right * 2.7f, Vector2.left, Vector2.up, new Vector2(1f, 2.3f) })
            {
                target.transform.position = position;
                enemy.currentHealth = 5000f;
                Physics2D.SyncTransforms();
                HashSet<Enemy> struck = new HashSet<Enemy>();
                axe.DamageAxeSweep(Vector2.right, axe.Radius, axe.Radius * axe.Definition.meleeSideScale, axe.AxeArc, 1f, struck, new WeaponAttackContext());
                float expected = position.y == 0f && position.x > 0f ? 50f : 0f;
                Near(5000f - enemy.currentHealth, expected, "Forward-only elliptical cleave at " + position);
                axe.DamageAxeSweep(Vector2.right, axe.Radius, axe.Radius * axe.Definition.meleeSideScale, axe.AxeArc, 1f, struck, new WeaponAttackContext());
                Near(5000f - enemy.currentHealth, expected, "No repeated hit from multiple colliders or sweep frames");
            }
            WeaponInventory inventory = WeaponInventory.Ensure(player);
            UpgradeDefinition unlock = catalog.Single(x => x.grantedWeapon != null && x.grantedWeapon.kind == WeaponKind.Melee);
            Check(inventory.Acquire(unlock.grantedWeapon) && axe.Definition.kind == WeaponKind.Axe, "Whirlwind drops alongside the axe");
            PlayerWeaponSystem whirlwind = inventory.Find(WeaponKind.Melee);
            stats.primaryWeaponDamageMultiplier = 2f;
            target.transform.position = Vector2.right;
            enemy.currentHealth = 5000f;
            whirlwind.Hit(enemy, whirlwind.Definition.damage, Vector2.zero);
            Near(5000f - enemy.currentHealth, 12f, "Reduced autonomous damage is independent of primary buffs");
            Near(target.transform.position.x, 1.6f, "Innate knockback works without a buff");
            whirlwind.ApplyBuff(WeaponBuffEffect.MeleeKnockback, .25f);
            target.transform.position = Vector2.right;
            whirlwind.Hit(enemy, whirlwind.Definition.damage, Vector2.zero);
            Near(target.transform.position.x, 1.85f, "Dedicated knockback stacks on innate push");
            Check(catalog.Count(x => x.weaponSpecific && x.targetWeapon == WeaponKind.Melee) == 6, "Six whirlwind buffs");
            Check(catalog.Count(x => x.weaponSpecific && x.targetWeapon == WeaponKind.Axe) == 8, "Six original axe buffs and two Myth specializations");
            PlayerBuffCollection collection = PlayerBuffCollection.Ensure(player);
            collection.Record(unlock, unlock.upgradeId);
            Check(collection.Entries[0].IsWeaponUnlock && collection.Entries[0].DisplayColor == UpgradeUIColors.NewWeapon,
                "Acquired automatic weapon names are red in the pause list");
            UpgradeDefinition damage = catalog.Single(x => x.weaponSpecific && x.targetWeapon == WeaponKind.Melee && x.weaponEffect == WeaponBuffEffect.Damage);
            collection.Record(damage, damage.upgradeId);
            Check(!collection.Entries[1].IsWeaponUnlock && collection.Entries[1].DisplayColor == UpgradeUIColors.ForRarity(damage.rarity),
                "Weapon buffs retain their own rarity color");
            Debug.Log("[MeleeSmokeTests] PASS: axe shape, hit deduplication, autonomous unlock/damage/knockback, buffs and red list entries.");
        }
        finally
        {
            foreach (UpgradeDefinition definition in catalog) UnityEngine.Object.DestroyImmediate(definition);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(root);
            Time.timeScale = previousTimeScale;
        }
    }
    private static void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); }
    private static void Near(float actual, float expected, string label) => Check(Mathf.Abs(actual - expected) < .001f, label);
}
