using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class ZoneRewardSmokeTests
{
    private const string Running = "ZoneRewardSmokeTests.Running";
    private static double readyAt;
    static ZoneRewardSmokeTests()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Running, false)) return;
            readyAt = EditorApplication.timeSinceStartup + 2;
            EditorApplication.update += Tick;
        };
    }

    [MenuItem("Tools/Zones/Run Integration Smoke Tests (opens GameScene)")]
    public static void RunBatch()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Run outside Play Mode.");
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
            Debug.Log("[ZoneRewardSmokeTests] PASS: schedule, five rewards, one-shot/repeatable zones, guard wave, pause, seals, runes, XP deduplication and fallback.");
            if (Application.isBatchMode) EditorApplication.Exit(0);
            else EditorApplication.ExitPlaymode();
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            if (Application.isBatchMode) EditorApplication.Exit(1);
            else EditorApplication.ExitPlaymode();
        }
    }

    private static void RunChecks()
    {
        PlayerController player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
        ZoneRewardDirector director = ZoneRewardDirector.Instance;
        RewardManager rewards = RewardManager.Instance;
        UpgradeManager upgrades = UpgradeManager.Instance;
        EnemySpawner spawner = UnityEngine.Object.FindAnyObjectByType<EnemySpawner>();
        Check(player != null && director != null && rewards != null && spawner != null, "Systems bootstrap from the real scene");
        ZoneRewardDirector prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ZoneRewardDirector.prefab").GetComponent<ZoneRewardDirector>();
        Check(director.firstWaveTime == prefab.firstWaveTime && director.sealedWaveTime == prefab.sealedWaveTime, "Scene inherits editable prefab schedule");
        foreach (PlayerWeaponSystem system in player.GetComponentsInChildren<PlayerWeaponSystem>()) system.enabled = false;
        spawner.StopAllCoroutines();
        foreach (Enemy enemy in UnityEngine.Object.FindObjectsByType<Enemy>()) UnityEngine.Object.DestroyImmediate(enemy.gameObject);
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        health.currentHealth = health.maxHealth = 100f;
        upgrades.autoPickFirstChoiceForPrototype = true;
        player.experienceThreshold = 10000f;
        ZoneRewardDefinition[] definitions = director.definitions;
        Check(definitions.Length == 5 && definitions.Select(x => x.requiredStayTime).SequenceEqual(new[] {10f,5f,8f,20f,6f}), "Five default durations");

        director.firstWaveTime = director.sealedWaveTime = 0f;
        Call(director, "Update");
        Check(director.Areas.Count == 10 && director.Areas.Count(x => x.IsLocked) == 5, "Both scheduled waves spawn exactly once");
        foreach (ZoneRewardArea sealedArea in director.Areas.Where(x => x.IsLocked))
            Near(sealedArea.Seal.requiredDamage, Mathf.Max(1f, director.sealedWaveSealHealth), "Second-wave seals inherit prefab health");
        Call(director, "Update"); Check(director.Areas.Count == 10, "No duplicate scheduled spawns");
        Check(director.Areas.All(x => !x.IsRevealed && x.NavigationName == "Unknown Altar"), "Remote arrows never disclose reward types");
        foreach (ZoneRewardArea area in director.Areas)
        {
            Camera spawnCamera = Camera.main;
            CameraScaler spawnScaler = spawnCamera.GetComponent<CameraScaler>();
            float worldScale = spawnScaler != null ? spawnCamera.orthographicSize / spawnScaler.baseOrthographicSize : 1f;
            float travel = Vector2.Distance(area.transform.position, player.transform.position) - area.radius;
            float minDistance = ZoneTravelDistance.Estimate(Mathf.Min(director.minTravelTime, director.maxTravelTime), director.referenceMoveSpeed, worldScale);
            float maxDistance = ZoneTravelDistance.Estimate(Mathf.Max(director.minTravelTime, director.maxTravelTime), director.referenceMoveSpeed, worldScale);
            Check(travel >= minDistance - .01f && travel <= maxDistance + .01f, "Each area uses the random reference-speed travel bounds");
            ZoneRewardDefinition definition = definitions.First(x => x.rewardType == area.rewardType);
            Near(area.radius, Mathf.Max(.5f, director.areaRadius), "Spawned areas use the absolute prefab radius");
            Near(area.requiredStayTime, director.GetRequiredStayTime(definition.rewardType), "Spawned areas use the prefab occupation seconds");
        }
        ZoneRewardArea discovered = director.Areas[0];
        player.transform.position = discovered.transform.position + Vector3.right * (director.revealDistance + .1f);
        Call(director, "Update"); Check(!discovered.IsRevealed, "No discovery outside proximity");
        player.transform.position = discovered.transform.position + Vector3.right * (director.revealDistance - .1f);
        Call(director, "Update"); Check(!discovered.IsRevealed, "Queued discovery does not reveal arrow before its notice");
        for (int i = 0; i < 3 && !discovered.IsRevealed; i++)
        {
            typeof(ZoneRewardDirector).GetField("noticeRemaining", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(director, 0f);
            Call(director, "Update");
        }
        Check(discovered.IsRevealed && discovered.NavigationName == discovered.displayName, "Actual discovery notice reveals the destination");
        Check(director.Areas.Where(x => Vector2.Distance(player.transform.position, x.transform.position) > director.revealDistance)
            .All(x => !x.IsRevealed), "Discovering a nearby area leaves remote types unknown");

        ZoneRewardArea spring = director.Areas.First(x => x.rewardType == ZoneRewardType.HealingSpring && !x.IsLocked);
        spring.requiredStayTime = .00001f;
        player.transform.position = spring.transform.position;
        health.currentHealth = 10f;
        Call(spring, "Update"); Near(health.currentHealth, 60f, "Spring restores exactly half max health");
        Check(spring.IsCompleted && !spring.gameObject.activeSelf, "One-shot area deactivates");
        Call(spring, "Update"); Near(health.currentHealth, 60f, "Completed zone cannot grant twice");

        ZoneRewardArea chest = director.Areas.First(x => x.rewardType == ZoneRewardType.DangerousChest && !x.IsLocked);
        chest.requiredStayTime = 100f;
        player.transform.position = chest.transform.position;
        Call(chest, "Update");
        Check(UnityEngine.Object.FindObjectsByType<Enemy>().Length == chest.enemyWaveSize, "Chest spawns a guard wave on entry");
        player.transform.position += Vector3.right * 20f; Call(chest, "Update");
        player.transform.position = chest.transform.position; Call(chest, "Update");
        Check(UnityEngine.Object.FindObjectsByType<Enemy>().Length == chest.enemyWaveSize, "Re-entry cannot duplicate guard wave");
        float progress = chest.Elapsed;
        Time.timeScale = 0f; Call(chest, "Update"); Near(chest.Elapsed, progress, "Pause freezes zone"); Time.timeScale = 1f;
        chest.requiredStayTime = .00001f;
        Call(chest, "Update"); Check(chest.IsCompleted, "Danger chest grants a rare reward and completes");

        ZoneRewardArea wreck = director.Areas.First(x => x.rewardType == ZoneRewardType.WeaponWreckage && !x.IsLocked);
        wreck.requiredStayTime = .00001f;
        player.transform.position = wreck.transform.position;
        PlayerWeaponSystem weapon = player.GetComponent<PlayerWeaponSystem>();
        float assetDamage = weapon.Definition.damage;
        Call(wreck, "Update"); Check(weapon.WeaponLevel == 2, "Wreck upgrades current weapon");
        Near(weapon.LevelDamageMultiplier, 1.05f, "Weapon level adds five percent damage");
        Near(weapon.Definition.damage, assetDamage, "Shared weapon definition is unchanged");

        ZoneRewardArea altar = director.Areas.First(x => x.rewardType == ZoneRewardType.Altar && !x.IsLocked);
        altar.requiredStayTime = .00001f; altar.oneShot = false;
        player.transform.position = altar.transform.position; Call(altar, "Update");
        float completedProgress = altar.Elapsed; Call(altar, "Update");
        Near(altar.Elapsed, completedProgress, "Repeatable area waits for exit rather than granting every frame");
        player.transform.position += Vector3.right * 20f; Call(altar, "Update"); Near(altar.Elapsed, 0f, "Repeatable area rearms after exit");

        Camera camera = Camera.main;
        foreach (ExperienceDrop existing in UnityEngine.Object.FindObjectsByType<ExperienceDrop>())
            UnityEngine.Object.DestroyImmediate(existing.gameObject);
        GameObject visible = new GameObject("VisibleXP"); visible.transform.position = camera.ViewportToWorldPoint(new Vector3(.5f,.5f,10f));
        ExperienceDrop pickup = visible.AddComponent<ExperienceDrop>();
        GameObject hidden = new GameObject("OffscreenXP"); hidden.transform.position = camera.ViewportToWorldPoint(new Vector3(4f,4f,10f));
        ExperienceDrop offscreenPickup = hidden.AddComponent<ExperienceDrop>();
        float xp = player.CurrentExperience;
        Check(rewards.GrantZoneReward(player, ZoneRewardType.ExperienceMagnet), "Magnet grants");
        Near(player.CurrentExperience, xp + player.Stats.GetExperienceAmount(1f) * 2f, "Magnet collects visible and offscreen map XP with gain multiplier");
        float gained = player.CurrentExperience;
        Check(!pickup.Collect(player), "Trigger after magnet cannot grant duplicate XP");
        Check(!offscreenPickup.Collect(player), "Offscreen pickup is consumed by the map-wide magnet");
        Near(player.CurrentExperience, gained, "No duplicate experience");
        GameObject later = new GameObject("LaterXP");
        ExperienceDrop laterPickup = later.AddComponent<ExperienceDrop>();
        Check(laterPickup.Collect(player), "Drops created after the reward are not pre-collected");

        foreach (Enemy enemy in UnityEngine.Object.FindObjectsByType<Enemy>()) UnityEngine.Object.DestroyImmediate(enemy.gameObject);
        ZoneRewardArea locked = director.Areas.First(x => x.IsLocked);
        player.transform.position = locked.transform.position; Call(locked, "Update");
        Near(locked.Elapsed, 0f, "Locked area cannot accumulate");
        ZoneSeal seal = locked.Seal;
        Check(seal.GetComponent<Enemy>() == null && SummonedUnit.FindTarget(seal.transform.position, 1000f) == null, "Seal excluded from automatic enemy targeting");
        seal.TakeDamage(seal.requiredDamage * .5f); Check(locked.IsLocked, "Partial damage does not unlock");
        seal.TakeDamage(seal.requiredDamage); Check(!locked.IsLocked && seal.IsBroken, "Sufficient damage unlocks once");
        ZoneRewardArea runeArea = director.Areas.First(x => x.IsLocked);
        runeArea.Seal.runeStayTime = .00001f;
        for (int i = 0; i < 3; i++) { player.transform.position = runeArea.Seal.NextRunePosition; Call(runeArea.Seal, "Update"); }
        Check(!runeArea.IsLocked && runeArea.Seal.RunesActivated == 3, "Three sequential runes unlock without a weapon requirement");

        upgrades.availableUpgrades = Array.Empty<UpgradeDefinition>();
        health.currentHealth = 10f;
        Check(rewards.GrantZoneReward(player, ZoneRewardType.Altar), "Exhausted altar is consumed");
        Near(health.currentHealth, Mathf.Min(health.maxHealth, 10f + health.maxHealth * .5f), "Exhausted altar falls back to healing");
    }

    private static void Call(object target, string method) => target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, null);
    private static void Check(bool value, string label) { if (!value) throw new Exception(label); }
    private static void Near(float value, float expected, string label) => Check(Mathf.Abs(value - expected) < .001f, label);
}
