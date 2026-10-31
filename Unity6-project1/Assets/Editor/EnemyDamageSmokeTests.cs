using System;
using UnityEditor;
using UnityEngine;

public static class EnemyDamageSmokeTests
{
    [MenuItem("Tools/Buffs/Run Target Damage Smoke Tests")]
    public static void Run()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Run outside Play Mode.");
        GameObject playerRoot = new GameObject("TargetDamageChecks");
        playerRoot.SetActive(false);
        GameObject target = new GameObject("DamageTarget");
        target.SetActive(false);
        EnemyDefinition definition = ScriptableObject.CreateInstance<EnemyDefinition>();
        UpgradeDefinition buff = ScriptableObject.CreateInstance<UpgradeDefinition>();
        try
        {
            PlayerController player = playerRoot.AddComponent<PlayerController>();
            PlayerStats stats = playerRoot.AddComponent<PlayerStats>();
            typeof(PlayerController).GetProperty("Stats").SetValue(player, stats);
            stats.damageMultiplier = 2f;
            stats.primaryWeaponDamageMultiplier = 1.5f;
            stats.criticalChance = 1f;
            buff.effects = new[] {
                new UpgradeEffect(UpgradeEffectType.NormalEnemyDamageBonus, .1f),
                new UpgradeEffect(UpgradeEffectType.EliteEnemyDamageBonus, .2f),
                new UpgradeEffect(UpgradeEffectType.BossDamageBonus, .3f),
                new UpgradeEffect(UpgradeEffectType.FlatHitDamage, 3f),
                new UpgradeEffect(UpgradeEffectType.ProjectileFlatDamage, 4f),
                new UpgradeEffect(UpgradeEffectType.ExecuteDamageBonus, .35f) };
            stats.ApplyUpgrade(buff, null);
            PlayerWeaponSystem weapon = playerRoot.AddComponent<PlayerWeaponSystem>();
            weapon.Configure(player, Resources.Load<WeaponDefinition>("Weapons/Pistol"));
            foreach (EnemyRank rank in Enum.GetValues(typeof(EnemyRank)))
            {
                Enemy enemy = target.AddComponent<Enemy>();
                enemy.Configure(definition, rank, EnemyAffix.None, 0f);
                enemy.maxHealth = enemy.currentHealth = 5000f;
                float bonus = rank == EnemyRank.Normal ? .1f : rank == EnemyRank.Elite ? .2f : .3f;
                weapon.Hit(enemy, 10f, Vector2.zero, projectileHit: true);
                Near(5000f - enemy.currentHealth, 50f * (1f + bonus) + 7f, "Global and primary damage add; target bonus once after crit; fixed damage stays seven");
                enemy.currentHealth = 5000f;
                weapon.Hit(enemy, 10f, Vector2.zero, procs: false, projectileHit: true);
                Near(5000f - enemy.currentHealth, 50f * (1f + bonus), "Secondary weapon hits suppress fixed procs");
                enemy.currentHealth = 1500f;
                weapon.Hit(enemy, 10f, Vector2.zero, projectileHit: true);
                Near(1500f - enemy.currentHealth, 50f * (1f + bonus + .35f) + 7f, "Execute uses pre-hit target health");
                UnityEngine.Object.DestroyImmediate(enemy);
            }
            Debug.Log("[EnemyDamageSmokeTests] PASS: upgrade effects, main weapon crit/target/fixed ordering, rank isolation, secondary proc suppression and execute.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(playerRoot);
            UnityEngine.Object.DestroyImmediate(definition);
            UnityEngine.Object.DestroyImmediate(buff);
        }
    }
    private static void Near(float actual, float expected, string label)
    { if (Mathf.Abs(actual - expected) > .001f) throw new InvalidOperationException(label + ": " + actual + " != " + expected); }
}
