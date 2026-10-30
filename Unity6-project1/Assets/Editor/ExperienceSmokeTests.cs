using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class ExperienceSmokeTests
{
    [MenuItem("Tools/Player/Run Experience Smoke Tests")]
    public static void Run()
    {
        if (Application.isPlaying || UpgradeManager.Instance != null || RewardManager.Instance != null)
            throw new InvalidOperationException("Run outside Play Mode without active reward managers.");
        GameObject root = new GameObject("ExperienceChecks");
        root.SetActive(false);
        try
        {
            PlayerController player = CreatePlayer(root.transform, "FractionalReward");
            RewardManager rewards = root.AddComponent<RewardManager>();
            player.Stats.experienceGainMultiplier = 1.15f;
            rewards.GrantExperience(player, 1f);
            Near(player.CurrentExperience, 1.15f, "Fractional boosted XP is stored immediately");
            Near(player.ExperienceProgress, .23f, "Bar shows 23%, rather than integer-truncated 20%");
            player.CollectExperience(3.85f);
            Check(player.level == 2, "Full threshold grants one level");
            Near(player.CurrentExperience, 0f, "Exact threshold resets bar");
            player.CollectExperience(12.25f);
            Check(player.level == 4, "One grant supports multiple levels");
            Near(player.CurrentExperience, 2.25f, "Fractional overflow is retained");
            Near(player.ExperienceProgress, .45f, "Overflow shows next level's progress");
            player.experienceThreshold = 2.5f;
            Near(player.ExperienceProgress, .9f, "Fractional threshold changes progress");
            player.CollectExperience(.25f);
            Check(player.level == 5, "Fractional threshold grants a level");
            Near(player.CurrentExperience, 0f, "Fractional threshold remainder");
            rewards.GrantExperience(player, .3f);
            Near(player.CurrentExperience, .345f, "Fractional base reward accepts experience bonus");
            player.CollectExperience(-1f);
            player.CollectExperience(float.NaN);
            player.CollectExperience(float.PositiveInfinity);
            Near(player.CurrentExperience, .345f, "Invalid grants cannot corrupt progress");
            PlayerController small = CreatePlayer(root.transform, "SmallPickups");
            for (int i = 0; i < 50; i++) small.CollectExperience(.1f);
            Check(small.level == 2, "Float accumulation error cannot delay leveling");
            Near(small.CurrentExperience, 0f, "No negative rounding residue");
            Check(typeof(PlayerController).GetField("experienceThreshold").FieldType == typeof(float) &&
                typeof(PlayerController).GetField("currentExperience", BindingFlags.NonPublic | BindingFlags.Instance).FieldType == typeof(float),
                "Both experience fields use float");
            Debug.Log("[ExperienceSmokeTests] PASS: fractional grants/thresholds, gain multipliers, multi-level overflow, bar ratio and rounding boundaries.");
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    private static PlayerController CreatePlayer(Transform parent, string name)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        PlayerController player = obj.AddComponent<PlayerController>();
        PlayerStats stats = obj.AddComponent<PlayerStats>();
        PlayerHealth health = obj.AddComponent<PlayerHealth>();
        health.currentHealth = health.maxHealth = 100f;
        typeof(PlayerController).GetProperty("Stats").SetValue(player, stats);
        return player;
    }
    private static void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); }
    private static void Near(float actual, float expected, string label) => Check(Mathf.Abs(actual - expected) < .0001f, label);
}
