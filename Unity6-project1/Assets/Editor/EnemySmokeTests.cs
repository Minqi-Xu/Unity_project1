using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Batch entry point intentionally stays alive across the Play Mode domain reload.
[InitializeOnLoad]
public static class EnemySmokeTests
{
    private const string Running = "EnemySmokeTests.Running";
    private static double readyAt;
    static EnemySmokeTests()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Running, false))
            {
                readyAt = EditorApplication.timeSinceStartup + 2;
                EditorApplication.update += Tick;
            }
        };
    }

    [MenuItem("Tools/Enemies/Run Integration Smoke Tests (opens GameScene)")]
    public static void RunBatch()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Run outside Play Mode.");
        BuffSmokeTests.Run();
        WeaponRewardSmokeTests.Run();
        PrimarySpecializationSmokeTests.RunRewards();
        WeaponBalanceSmokeTests.Run();
        AbilityUiSmokeTests.Run();
        EnemyDamageSmokeTests.Run();
        GameCodexSmokeTests.Run();
        SessionState.SetBool(Running, true);
        EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");
        EditorApplication.EnterPlaymode();
    }

    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup < readyAt) return;
        EditorApplication.update -= Tick;
        SessionState.SetBool(Running, false);
        try
        {
            RunChecks();
            Debug.Log("[EnemySmokeTests] PASS: roster, scaling, contact survival, seven encounters, unique chests, rare choices, queued level-up, final boss victory.");
            if (Application.isBatchMode) EditorApplication.Exit(0);
            else EditorApplication.ExitPlaymode();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            if (Application.isBatchMode) EditorApplication.Exit(1);
            else EditorApplication.ExitPlaymode();
        }
    }

    private static void RunChecks()
    {
        EnemySpawner spawner = UnityEngine.Object.FindAnyObjectByType<EnemySpawner>();
        GameManager game = GameManager.Instance;
        PlayerController player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
        Check(spawner != null && game != null && player != null, "Real gameplay scene starts");
        spawner.StopAllCoroutines();
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        health.damageMultiplier = health.damageReceiveIncreasingRate = 0f;
        foreach (PlayerWeaponSystem weapon in player.GetComponents<PlayerWeaponSystem>()) weapon.enabled = false;
        foreach (Enemy enemy in UnityEngine.Object.FindObjectsByType<Enemy>()) UnityEngine.Object.DestroyImmediate(enemy.gameObject);
        PrimarySpecializationSmokeTests.RunCombat();
        RewardManager.Instance.primaryWeaponChestChance = 0f;
        Check(spawner.enemyTypes.Select(x => x.kind).Distinct().Count() == 4, "Four normal kinds");
        for (int i = 0; i < 100; i++) Check(spawner.PickNormalType(0).kind == EnemyKind.Chaser, "Only chasers at start");
        Check(spawner.encounters.Select(x => x.time).SequenceEqual(new float[] { 120, 240, 360, 420, 540, 600, 720 }), "Twelve-minute encounter schedule");
        Check(spawner.initialSpawnDelay == 0f, "First normal enemy spawns immediately");
        Check(spawner.normalEnemySchedule.Select(x => x.unlockTime).SequenceEqual(new float[] { 0, 60, 120, 300 }), "Spawner owns every normal unlock time");
        float[] originalWeights = spawner.normalEnemySchedule.Select(x => x.weight).ToArray();
        try
        {
            foreach (NormalEnemySpawnRule rule in spawner.normalEnemySchedule) rule.weight = rule.kind == EnemyKind.Ranged ? 1f : 0f;
            Check(spawner.PickNormalType(299f) == null, "Locked kinds cannot leak through the fallback picker");
            Check(spawner.PickNormalType(300f).kind == EnemyKind.Ranged, "Spawner ranged timing overrides the legacy asset unlock time");
        }
        finally
        {
            for (int i = 0; i < originalWeights.Length; i++) spawner.normalEnemySchedule[i].weight = originalWeights[i];
        }
        Check(spawner.encounterWarningTime == 2f, "Two-second warning");
        EnemyDefinition chaser = spawner.enemyTypes.First(x => x.kind == EnemyKind.Chaser);
        GameObject probe = new GameObject("EnemyProbe");
        Enemy sample = probe.AddComponent<Enemy>();
        sample.Configure(chaser, EnemyRank.Elite, EnemyAffix.Swift | EnemyAffix.Healthy, 20f);
        Near(sample.maxHealth, 120f * 7f * 1.8f, "Health scaling with affix");
        Near(sample.speed, 4.5f * chaser.eliteSpeedMultiplier, "Elite slowdown includes Swift affix");
        Check(sample.IsAlive, "Configured enemy alive before Start");
        Call(sample, "Contact", player.GetComponentInChildren<Collider2D>());
        Check(sample.IsAlive, "Elite survives player contact");
        UnityEngine.Object.DestroyImmediate(probe);
        CharacterDefinition[] roster = Resources.LoadAll<CharacterDefinition>("Characters");
        float referenceSpeed = CharacterCatalog.GetStartingAverageMoveSpeed(roster);
        Near(referenceSpeed, roster.Where(x => x.availableAtStart).Average(x => x.moveSpeed), "Only starting characters determine the speed reference");
        Near(spawner.StartingCharacterAverageSpeed, referenceSpeed, "Spawner caches starting roster average");
        foreach (EnemyDefinition type in spawner.enemyTypes)
            foreach (EnemyRank rank in new[] { EnemyRank.Elite, EnemyRank.MiniBoss, EnemyRank.FinalBoss })
            {
                GameObject capProbe = new GameObject("SpeedCapProbe");
                Enemy enemy = capProbe.AddComponent<Enemy>();
                enemy.Configure(type, rank, EnemyAffix.Swift | EnemyAffix.PeriodicDash, 0f, referenceSpeed);
                Check(enemy.speed <= referenceSpeed * 1.05f, "Swift cannot bypass the special-enemy speed cap");
                Check(enemy.GetLimitedMovementSpeed(100f) <= referenceSpeed * 1.05f, "Dash and extra multipliers cannot bypass the speed cap");
                UnityEngine.Object.DestroyImmediate(capProbe);
            }
        foreach (EnemyRank rank in new[] { EnemyRank.MiniBoss, EnemyRank.FinalBoss })
        {
            GameObject bossProbe = new GameObject("BossSpeedProbe");
            Enemy boss = bossProbe.AddComponent<Enemy>();
            boss.Configure(chaser, rank, EnemyAffix.PeriodicDash, 0f);
            Near(boss.speed, 3f * chaser.speedMultiplier * .6f, "Boss movement reduced by 40%");
            Near(chaser.bossDashSpeedMultiplier, 2f, "Boss dash multiplier reduced to two");
            UnityEngine.Object.DestroyImmediate(bossProbe);
        }

        Set(spawner, "gameTime", 720f);
        Call(spawner, "Update");
        Enemy[] enemies = UnityEngine.Object.FindObjectsByType<Enemy>();
        Check(enemies.Length == 7, "All seven timed encounters spawn");
        foreach (EnemyEncounter encounter in spawner.encounters.Where(x => x.rank >= EnemyRank.MiniBoss))
        {
            Enemy boss = enemies.Single(x => x.DisplayName == encounter.title);
            EnemyDefinition type = spawner.enemyTypes.First(x => x.kind == encounter.kind);
            float expected = spawner.enemyPrefab.GetComponent<Enemy>().speed * type.speedMultiplier * type.bossSpeedMultiplier * encounter.bossMovementMultiplier;
            Near(boss.speed, Mathf.Min(expected, referenceSpeed * encounter.bossRelativeSpeedLimit), "Timed boss receives its own movement settings");
            Near(boss.GetLimitedMovementSpeed(100f), referenceSpeed * encounter.bossRelativeSpeedLimit, "Timed boss dash respects its encounter cap");
        }
        Call(spawner, "Update");
        Check(UnityEngine.Object.FindObjectsByType<Enemy>().Length == 7, "No duplicate timed spawn");
        foreach (Enemy enemy in enemies) enemy.enabled = false;
        Enemy elite = enemies.First(x => x.Rank == EnemyRank.Elite);
        int before = UnityEngine.Object.FindObjectsByType<RewardChest>().Length;
        elite.TakeDamage(float.MaxValue);
        elite.TakeDamage(float.MaxValue);
        Check(UnityEngine.Object.FindObjectsByType<RewardChest>().Length == before + 1, "Exactly one guaranteed chest");

        UpgradeManager upgrades = UpgradeManager.Instance;
        RewardChest eliteChest = UnityEngine.Object.FindObjectsByType<RewardChest>().First(x => x.enemyRank == EnemyRank.Elite);
        eliteChest.transform.position = player.transform.position;
        Call(eliteChest, "Update");
        List<UpgradeDefinition> choices = Choices(upgrades);
        Check(Time.timeScale == 0f && choices.Count == 3 && choices.All(x => x.rarity >= UpgradeRarity.Rare), "Paused Rare+ three-choice offer");
        Check(choices.Select(x => x.upgradeId).Distinct().Count() == 3, "Different buff families");
        upgrades.TryOfferLevelUpRewards(player);
        for (int roll = 1; roll <= RewardManager.EliteChestBuffCount; roll++)
        {
            Check(upgrades.IsSelecting && Time.timeScale == 0f, "Elite chest stays paused through both rewards");
            choices = Choices(upgrades);
            Check(choices.Count == 3 && choices.All(x => x.rarity >= UpgradeRarity.Rare), "Each chest roll offers three Rare+ choices");
            Call(upgrades, "SelectUpgrade", choices[0]);
        }
        Check(upgrades.IsSelecting && Choices(upgrades).Count == 3, "Queued level-up preserves three choices");
        RewardManager.Instance.GrantZoneReward(player, ZoneRewardType.DangerousChest);
        Call(upgrades, "SelectUpgrade", Choices(upgrades)[0]);
        Check(upgrades.IsSelecting && Choices(upgrades).Count == 3 && Choices(upgrades)[0].weaponSpecific && Choices(upgrades)[0].targetWeapon == player.GetComponent<PlayerWeaponSystem>().Definition.kind && Choices(upgrades).Skip(1).All(x => x.rarity >= UpgradeRarity.Epic), "Queued danger chest preserves primary guarantee and Epic+ alternatives");
        Call(upgrades, "SelectUpgrade", Choices(upgrades)[0]);
        Check(!upgrades.IsSelecting && Time.timeScale == 1f, "Reward queue resumes play");
        Enemy miniBoss = enemies.First(x => x.Rank == EnemyRank.MiniBoss && x.GuaranteedPrimaryWeaponReward);
        miniBoss.TakeDamage(float.MaxValue);
        RewardChest miniChest = UnityEngine.Object.FindObjectsByType<RewardChest>().First(x => x.enemyRank == EnemyRank.MiniBoss);
        Check(miniChest.guaranteePrimaryWeaponBuff, "First mini-boss encounter preserves guarantee on its dropped chest");
        miniChest.transform.position = player.transform.position;
        Call(miniChest, "Update");
        Check(Choices(upgrades).Count == 3 && Choices(upgrades)[0].weaponSpecific && Choices(upgrades)[0].targetWeapon == player.GetComponent<PlayerWeaponSystem>().Definition.kind && Choices(upgrades).Skip(1).All(x => x.rarity >= UpgradeRarity.Epic), "First mini-boss chest guarantees primary option and two Epic+ alternatives");
        for (int roll = 0; roll < RewardManager.BossChestBuffCount; roll++)
        {
            Check(upgrades.IsSelecting && Choices(upgrades).All(x => x.rarity >= UpgradeRarity.Rare), "Mini boss chest grants three rare choices");
            Call(upgrades, "SelectUpgrade", Choices(upgrades)[0]);
        }
        Check(!upgrades.IsSelecting && Time.timeScale == 1f, "Mini boss chest completes after three rewards");
        CheckExhaustedChest(upgrades, player, health);

        Set(game, "survivalTime", 10000f);
        Call(game, "Update");
        Check(!(bool)Get(game, "gameEnded"), "Elapsed time alone can never grant victory");
        Set(game, "survivalTime", 1f);
        Enemy final = enemies.First(x => x.Rank == EnemyRank.FinalBoss);
        final.TakeDamage(float.MaxValue);
        Check(game.FinalBossDefeated, "Final kill recorded");
        Check((bool)Get(game, "gameEnded") && Time.timeScale == 0f, "Final boss kill ends the run immediately, even before the default schedule time");
        Check(!upgrades.IsSelecting, "Victory does not wait for chest pickup or another upgrade selection");
    }

    private static void CheckExhaustedChest(UpgradeManager manager, PlayerController player, PlayerHealth health)
    {
        UpgradeDefinition[] originalPool = manager.availableUpgrades;
        UpgradeDefinition[] limitedPool = new UpgradeDefinition[2];
        for (int i = 0; i < limitedPool.Length; i++)
        {
            limitedPool[i] = ScriptableObject.CreateInstance<UpgradeDefinition>();
            limitedPool[i].upgradeId = "chest_exhaustion_probe_" + i;
            limitedPool[i].displayName = "Chest Probe " + i;
            limitedPool[i].rarity = UpgradeRarity.Rare;
            limitedPool[i].effects = new[] { new UpgradeEffect(UpgradeEffectType.DamageMultiplier, .1f) };
        }
        try
        {
            manager.availableUpgrades = limitedPool;
            health.currentHealth = health.maxHealth * .1f;
            RewardManager.Instance.GrantEnemyChest(player, EnemyRank.MiniBoss);
            Check(Choices(manager).Count == 2, "Limited pool starts with two choices");
            Call(manager, "SelectUpgrade", Choices(manager)[0]);
            Check(manager.IsSelecting && Choices(manager).Count == 1, "Second roll refreshes after the first buff reaches its cap");
            Call(manager, "SelectUpgrade", Choices(manager)[0]);
            Check(!manager.IsSelecting && Time.timeScale == 1f, "Exhausted third roll drains the queue");
            Near(health.currentHealth, health.maxHealth * .6f, "Queued exhausted roll grants healing fallback");
        }
        finally
        {
            manager.availableUpgrades = originalPool;
            foreach (UpgradeDefinition upgrade in limitedPool) UnityEngine.Object.DestroyImmediate(upgrade);
        }
    }

    private static List<UpgradeDefinition> Choices(UpgradeManager manager) => (List<UpgradeDefinition>)Get(manager, "activeChoices");
    private static object Get(object target, string field) => target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);
    private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
    private static void Call(object target, string method, params object[] args) => target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, args);
    private static void Near(float actual, float expected, string label) => Check(Mathf.Abs(actual - expected) < .01f, label);
    private static void Check(bool condition, string label) { if (!condition) throw new InvalidOperationException(label); }
}

