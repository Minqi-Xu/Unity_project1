using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class PrimarySpecializationSmokeTests
{
    [MenuItem("Tools/Weapons/Run Primary Specialization Reward Checks")]
    public static void RunRewards()
    {
        if (Application.isPlaying || UpgradeManager.Instance != null || RewardManager.Instance != null)
            throw new InvalidOperationException("Run outside Play Mode without active run managers.");
        GameObject root = new GameObject("PrimaryRewardChecks"); root.SetActive(false);
        UpgradeDefinition[] pool = null;
        UnityEngine.Random.State random = UnityEngine.Random.state;
        try
        {
            PlayerController player = root.AddComponent<PlayerController>();
            PlayerStats stats = root.AddComponent<PlayerStats>();
            typeof(PlayerController).GetProperty("Stats").SetValue(player, stats);
            PlayerHealth health = root.AddComponent<PlayerHealth>(); health.maxHealth = health.currentHealth = 100f;
            PlayerWeaponSystem primary = root.AddComponent<PlayerWeaponSystem>();
            UpgradeManager upgrades = root.AddComponent<UpgradeManager>(); Invoke(upgrades, "Awake");
            RewardManager rewards = root.AddComponent<RewardManager>(); Invoke(rewards, "Awake");
            upgrades.autoPickFirstChoiceForPrototype = true;
            pool = upgrades.availableUpgrades;
            Check(pool.Count(x => x.primarySpecialization) == 9 && pool.Where(x => x.primarySpecialization).All(x => x.rarity == UpgradeRarity.Myth), "Nine exclusive Myth entries without old rarity duplicates");
            CheckChestDistribution(upgrades);
            Check(pool.Where(x => x.primarySpecialization).Select(x => x.upgradeId).Distinct().Count() == 9, "Nine new families");
            foreach (WeaponKind kind in new[] { WeaponKind.Pistol, WeaponKind.Shotgun, WeaponKind.Bow, WeaponKind.Turret, WeaponKind.Axe, WeaponKind.Bomb })
            {
                primary.Configure(player, Resources.Load<WeaponDefinition>("Weapons/" + kind));
                List<UpgradeDefinition> choices = (List<UpgradeDefinition>)Invoke(upgrades, "PickPrimaryWeaponChoices", player);
                Check(choices.Count > 0 && choices.All(x => x.primarySpecialization && x.targetWeapon == kind), "Guaranteed candidates match primary: " + kind);
                List<UpgradeDefinition> offer = (List<UpgradeDefinition>)Invoke(upgrades, "BuildPrimaryWeaponOffer", player);
                Check(offer.Count == 3 && offer[0].primarySpecialization && offer[0].targetWeapon == kind &&
                    offer.Skip(1).All(x => x.rarity >= UpgradeRarity.Epic), "Primary guarantee has two Epic+ alternatives: " + kind);
                Check(offer.Select(x => x.upgradeId).Distinct().Count() == 3, "Guaranteed offer has three different families");
                List<UpgradeDefinition> normalChest = (List<UpgradeDefinition>)Invoke(upgrades, "PickRareChoices", player);
                Check(normalChest.Count == 3 && normalChest.All(x => x.rarity >= UpgradeRarity.Rare) &&
                    normalChest.Select(x => x.upgradeId).Distinct().Count() == 3, "Advanced chest offers three distinct Rare+ choices");
                int before = LevelTotal(upgrades, pool, kind);
                Check(rewards.GrantZoneReward(player, ZoneRewardType.DangerousChest), "Danger chest grants: " + kind);
                Check(LevelTotal(upgrades, pool, kind) == before + 1, "Danger chest applies one specialization: " + kind);
            }
            primary.Configure(player, Resources.Load<WeaponDefinition>("Weapons/Pistol"));
            UpgradeDefinition ricochet = pool.First(x => x.weaponEffect == WeaponBuffEffect.PistolRicochet);
            while (upgrades.GetUpgradeLevel(ricochet) < ricochet.maxLevel) Check(upgrades.ApplyUpgrade(ricochet, player), "Fill shared family cap");
            Check(pool.Where(x => x.weaponEffect == WeaponBuffEffect.PistolRicochet).All(x => !upgrades.ApplyUpgrade(x, player)), "Rarity cannot bypass family cap");
            List<UpgradeDefinition> fallback = (List<UpgradeDefinition>)Invoke(upgrades, "PickPrimaryWeaponChoices", player);
            Check(fallback.Count > 0 && fallback.All(x => !x.primarySpecialization && x.targetWeapon == WeaponKind.Pistol), "Maxed new families fall back to existing primary buffs");
            foreach (UpgradeDefinition buff in pool.Where(x => x.weaponSpecific && x.targetWeapon == WeaponKind.Pistol))
                while (upgrades.GetUpgradeLevel(buff) < buff.maxLevel) Check(upgrades.ApplyUpgrade(buff, player), "Fill primary pool");
            Check(((List<UpgradeDefinition>)Invoke(upgrades, "PickPrimaryWeaponChoices", player)).Count == 0, "Fully maxed primary pool is exhausted");
            Check(upgrades.TryOfferPrimaryWeaponRewards(player), "Exhausted primary pool falls back to an ordinary rare upgrade");
            upgrades.availableUpgrades = Array.Empty<UpgradeDefinition>(); health.currentHealth = 10f;
            Check(rewards.GrantZoneReward(player, ZoneRewardType.DangerousChest), "Fully exhausted chest still completes");
            Near(health.currentHealth, 60f, "Exhausted reward heals instead of disappearing");
            Debug.Log("[PrimarySpecializationSmokeTests] PASS: six primaries, variant caps, guaranteed dangerous chest, both exhaustion fallbacks.");
        }
        finally
        {
            if (pool != null) foreach (UpgradeDefinition buff in pool) UnityEngine.Object.DestroyImmediate(buff);
            UnityEngine.Object.DestroyImmediate(root); UnityEngine.Random.state = random;
        }
    }

    private static void CheckChestDistribution(UpgradeManager manager)
    {
        List<UpgradeDefinition> skewed = new List<UpgradeDefinition>();
        UnityEngine.Random.State saved = UnityEngine.Random.state;
        try
        {
            // Deliberately skew the catalog: fifty Rare cards must not drown out the single Epic card.
            for (int i = 0; i < 53; i++)
            {
                UpgradeDefinition buff = ScriptableObject.CreateInstance<UpgradeDefinition>();
                buff.upgradeId = "distribution_probe_" + i;
                buff.rarity = i < 50 ? UpgradeRarity.Rare : (UpgradeRarity)(i - 48);
                skewed.Add(buff);
            }
            UnityEngine.Random.InitState(893721);
            int[] counts = new int[5];
            const int draws = 8000;
            for (int i = 0; i < draws; i++)
            {
                var choices = (List<UpgradeDefinition>)Invoke(manager, "PickChestChoices", new List<UpgradeDefinition>(skewed), 1);
                counts[(int)choices[0].rarity]++;
            }
            Check(counts[(int)UpgradeRarity.Epic] > counts[(int)UpgradeRarity.Rare] &&
                counts[(int)UpgradeRarity.Rare] > counts[(int)UpgradeRarity.Legendary] &&
                counts[(int)UpgradeRarity.Legendary] > counts[(int)UpgradeRarity.Myth], "Chest rarity order ignores unequal catalog sizes");
            NearRatio(counts[1], draws, .25f); NearRatio(counts[2], draws, .55f);
            NearRatio(counts[3], draws, .15f); NearRatio(counts[4], draws, .05f);
        }
        finally { foreach (UpgradeDefinition buff in skewed) UnityEngine.Object.DestroyImmediate(buff); UnityEngine.Random.state = saved; }
    }

    private static void NearRatio(int count, int total, float expected) =>
        Check(Mathf.Abs((float)count / total - expected) < .025f, "Rarity odds stay within statistical tolerance of " + expected);

    public static void RunCombat()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Combat checks require Play Mode.");
        GameObject root = new GameObject("PrimaryCombatChecks"); root.SetActive(false);
        GameObject targets = new GameObject("PrimaryCombatTargets");
        WeaponDefinition data = ScriptableObject.CreateInstance<WeaponDefinition>();
        EnemyDefinition enemyData = ScriptableObject.CreateInstance<EnemyDefinition>();
        try
        {
            PlayerController player = root.AddComponent<PlayerController>();
            PlayerStats stats = root.AddComponent<PlayerStats>();
            typeof(PlayerController).GetProperty("Stats").SetValue(player, stats);
            root.AddComponent<PlayerHealth>();
            PlayerWeaponSystem weapon = root.AddComponent<PlayerWeaponSystem>();
            root.transform.position = new Vector3(1000, 1000);
            Enemy a = Target(targets, 0), b = Target(targets, 1), c = Target(targets, 2), far = Target(targets, 5);
            GameObject duplicate = new GameObject("Duplicate collider"); duplicate.transform.SetParent(b.transform, false); duplicate.AddComponent<CircleCollider2D>().radius = .05f;
            Physics2D.SyncTransforms();

            data.kind = WeaponKind.Turret; weapon.Configure(player, data); weapon.ApplyBuff(WeaponBuffEffect.TurretSplash, .35f);
            stats.criticalChance = 1f; stats.flatHitDamage = stats.projectileFlatDamage = 100f;
            weapon.Hit(a, 10f, root.transform.position, context: new WeaponAttackContext(), projectileHit: true);
            Near(b.currentHealth, 4993f, "Splash copies one rolled crit, suppresses both flat enchants and duplicate colliders");
            Near(a.currentHealth, 4780f, "Direct target takes crit plus both flat enchants exactly once");
            Near(far.currentHealth, 5000f, "Splash respects radius");
            stats.criticalChance = stats.flatHitDamage = stats.projectileFlatDamage = 0f;

            Reset(a, b, c, far); data.kind = WeaponKind.Pistol; weapon.Configure(player, data); weapon.ApplyBuff(WeaponBuffEffect.PistolRicochet, 2f);
            WeaponAttackContext volley = new WeaponAttackContext();
            weapon.Hit(a, 10f, root.transform.position, context: volley, projectileHit: true);
            Near(b.currentHealth, 4994f, "First ricochet retains sixty percent"); Near(c.currentHealth, 4996.4f, "Second ricochet attenuates again");
            weapon.Hit(a, 10f, root.transform.position, context: volley, projectileHit: true);
            Near(b.currentHealth, 4994f, "Piercing cannot restart a volley chain");

            Reset(a, b, c, far); data.kind = WeaponKind.Shotgun; data.projectiles = 6; weapon.Configure(player, data); weapon.ApplyBuff(WeaponBuffEffect.ShotgunShockwave, .2f);
            volley = new WeaponAttackContext();
            weapon.Hit(a, 10f, root.transform.position, context: volley, projectileHit: true);
            weapon.Hit(a, 10f, root.transform.position, context: volley, projectileHit: true);
            Near(b.currentHealth, 4988f, "Shockwave runs once per volley rather than per pellet");

            Reset(a, b, c, far); data.kind = WeaponKind.Bow; weapon.Configure(player, data); weapon.ApplyBuff(WeaponBuffEffect.BowNova, .4f);
            weapon.Hit(a, 10f, root.transform.position, pierced: 0, projectileHit: true);
            Near(b.currentHealth, 5000f, "First pierced target does not trigger nova");
            weapon.Hit(b, 10f, root.transform.position, pierced: 1, projectileHit: true);
            Near(c.currentHealth, 4996f, "Second pierced target triggers nova");

            Reset(a, b, c, far); data.kind = WeaponKind.Axe; weapon.Configure(player, data); weapon.ApplyBuff(WeaponBuffEffect.AxeKillBurst, .6f);
            a.currentHealth = 1f; volley = new WeaponAttackContext();
            weapon.Hit(a, 10f, root.transform.position, context: volley);
            Near(b.currentHealth, 4994f, "Axe direct kill produces one execution blast");
            Check(volley.primaryKillBurstTriggered, "Execution blast budget belongs to the swing");

            Reset(b, c, far); data.kind = WeaponKind.Bomb; weapon.Configure(player, data); weapon.ApplyBuff(WeaponBuffEffect.BombOuterRing, .35f);
            b.transform.position = root.transform.position + Vector3.right * .5f;
            Enemy border = Target(targets, 2f); border.GetComponent<CircleCollider2D>().radius = .5f;
            c.transform.position = root.transform.position + Vector3.right * 2.5f; Physics2D.SyncTransforms();
            weapon.Explode(root.transform.position, 1.8f, 40f);
            Near(b.currentHealth, 4960f, "Outer ring cannot double-hit the main blast"); Near(c.currentHealth, 4986f, "Outer ring reaches beyond main explosion");
            Near(border.currentHealth, 4990f, "Large enemy overlapping the blast edge cannot take both blasts");
            weapon.ApplyBuff(WeaponBuffEffect.BombVacuum, 1.5f);
            c.transform.position = root.transform.position + Vector3.right * 2.5f;
            far.transform.position = root.transform.position + Vector3.right * 2.5f;
            far.Configure(enemyData, EnemyRank.MiniBoss, EnemyAffix.None, 0f); Physics2D.SyncTransforms();
            weapon.PullBombTargets(root.transform.position, 1.8f);
            Near(c.transform.position.x, 1001f, "Implosion moves normal enemies inward"); Near(far.transform.position.x, 1002.5f, "Implosion cannot move bosses");
            Enemy[] crowd = Enumerable.Range(0, 12).Select(i => Target(targets, 10f + i * .1f)).ToArray(); Physics2D.SyncTransforms();
            weapon.SecondaryArea(new Vector2(1010, 1000), 2f, 1f);
            Check(crowd.Count(x => x.currentHealth < 5000f) == 8, "Each secondary area is capped at eight additional targets");
            Debug.Log("[PrimarySpecializationSmokeTests] PASS: crit/proc suppression, collider deduplication, ricochet attenuation, volley budget, bow nova, axe execution, bomb ring and boss pull immunity.");
        }
        finally { UnityEngine.Object.DestroyImmediate(targets); UnityEngine.Object.DestroyImmediate(root); UnityEngine.Object.DestroyImmediate(data); UnityEngine.Object.DestroyImmediate(enemyData); }
    }

    private static Enemy Target(GameObject group, float offset)
    {
        GameObject target = new GameObject("AOE target"); target.transform.SetParent(group.transform);
        target.transform.position = new Vector3(1000 + offset, 1000); target.AddComponent<CircleCollider2D>().radius = .05f;
        Enemy enemy = target.AddComponent<Enemy>(); enemy.enabled = false; enemy.maxHealth = enemy.currentHealth = 5000f; return enemy;
    }
    private static void Reset(params Enemy[] enemies) { foreach (Enemy enemy in enemies) if (enemy != null && enemy.IsAlive) enemy.currentHealth = enemy.maxHealth = 5000f; Physics2D.SyncTransforms(); }
    private static int LevelTotal(UpgradeManager manager, UpgradeDefinition[] pool, WeaponKind kind) => pool.Where(x => x.primarySpecialization && x.targetWeapon == kind).GroupBy(x => x.upgradeId).Sum(x => manager.GetUpgradeLevel(x.First()));
    private static object Invoke(object target, string name, params object[] args) => target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
    private static void Check(bool condition, string label) { if (!condition) throw new InvalidOperationException(label); }
    private static void Near(float actual, float expected, string label) => Check(Mathf.Abs(actual - expected) < .001f, label + $" ({actual} versus {expected})");
}
