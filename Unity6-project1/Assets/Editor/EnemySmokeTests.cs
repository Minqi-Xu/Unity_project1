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
        Check(spawner.enemyTypes.Select(x => x.kind).Distinct().Count() == 4, "Four normal kinds");
        for (int i = 0; i < 100; i++) Check(spawner.PickNormalType(0).kind == EnemyKind.Chaser, "Only chasers at start");
        Check(spawner.encounters.Select(x => x.time).SequenceEqual(new float[] { 120, 300, 480, 600, 900, 1080, 1200 }), "Encounter schedule");
        Check(spawner.encounterWarningTime == 2f, "Two-second warning");
        EnemyDefinition chaser = spawner.enemyTypes.First(x => x.kind == EnemyKind.Chaser);
        GameObject probe = new GameObject("EnemyProbe");
        Enemy sample = probe.AddComponent<Enemy>();
        sample.Configure(chaser, EnemyRank.Elite, EnemyAffix.Swift | EnemyAffix.Healthy, 20f);
        Near(sample.maxHealth, 120f * 7f * 1.8f, "Health scaling with affix");
        Near(sample.speed, 4.5f, "Swift affix");
        Check(sample.IsAlive, "Configured enemy alive before Start");
        Call(sample, "Contact", player.GetComponentInChildren<Collider2D>());
        Check(sample.IsAlive, "Elite survives player contact");
        UnityEngine.Object.DestroyImmediate(probe);

        Set(spawner, "gameTime", 1200f);
        Call(spawner, "Update");
        Enemy[] enemies = UnityEngine.Object.FindObjectsByType<Enemy>();
        Check(enemies.Length == 7, "All seven timed encounters spawn");
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
        Check(Time.timeScale == 0f && choices.Count == 2 && choices.All(x => x.rarity >= UpgradeRarity.Rare), "Paused rare two-choice offer");
        Check(choices.Select(x => x.upgradeId).Distinct().Count() == 2, "Different buff families");
        upgrades.TryOfferLevelUpRewards(player);
        for (int roll = 1; roll <= RewardManager.EliteChestBuffCount; roll++)
        {
            Check(upgrades.IsSelecting && Time.timeScale == 0f, "Elite chest stays paused through both rewards");
            choices = Choices(upgrades);
            Check(choices.Count == 2 && choices.All(x => x.rarity >= UpgradeRarity.Rare), "Each chest roll is rare two-choice");
            Call(upgrades, "SelectUpgrade", choices[0]);
        }
        Check(upgrades.IsSelecting && Choices(upgrades).Count == 3, "Queued level-up preserves three choices");
        Call(upgrades, "SelectUpgrade", Choices(upgrades)[0]);
        Check(!upgrades.IsSelecting && Time.timeScale == 1f, "Reward queue resumes play");
        Enemy miniBoss = enemies.First(x => x.Rank == EnemyRank.MiniBoss);
        miniBoss.TakeDamage(float.MaxValue);
        RewardChest miniChest = UnityEngine.Object.FindObjectsByType<RewardChest>().First(x => x.enemyRank == EnemyRank.MiniBoss);
        miniChest.transform.position = player.transform.position;
        Call(miniChest, "Update");
        for (int roll = 0; roll < RewardManager.BossChestBuffCount; roll++)
        {
            Check(upgrades.IsSelecting && Choices(upgrades).All(x => x.rarity >= UpgradeRarity.Rare), "Mini boss chest grants three rare choices");
            Call(upgrades, "SelectUpgrade", Choices(upgrades)[0]);
        }
        Check(!upgrades.IsSelecting && Time.timeScale == 1f, "Mini boss chest completes after three rewards");
        CheckExhaustedChest(upgrades, player, health);

        Set(game, "survivalTime", 1201f);
        Call(game, "Update");
        Check(!(bool)Get(game, "gameEnded"), "20 minutes does not skip final boss");
        Enemy final = enemies.First(x => x.Rank == EnemyRank.FinalBoss);
        final.TakeDamage(float.MaxValue);
        Check(game.FinalBossDefeated, "Final kill recorded");
        Call(game, "Update");
        Check(!(bool)Get(game, "gameEnded"), "Victory waits for chest");
        RewardChest finalChest = UnityEngine.Object.FindObjectsByType<RewardChest>().First(x => x.finalBossReward);
        finalChest.transform.position = player.transform.position;
        Call(finalChest, "Update");
        Check(upgrades.IsSelecting, "Final chest offers rare reward");
        for (int roll = 0; roll < RewardManager.BossChestBuffCount; roll++)
        {
            Call(game, "Update");
            Check(!(bool)Get(game, "gameEnded"), "Victory waits for every final chest roll");
            Call(upgrades, "SelectUpgrade", Choices(upgrades)[0]);
        }
        Call(game, "Update");
        Check((bool)Get(game, "gameEnded") && Time.timeScale == 0f, "Victory after final reward");
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

