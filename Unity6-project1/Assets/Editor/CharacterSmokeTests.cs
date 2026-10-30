using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class CharacterSmokeTests
{
    [MenuItem("Tools/Characters/Run Smoke Tests")]
    public static void Run()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Run outside Play Mode.");
        CharacterDefinition[] roster = CharacterCatalog.Load();
        Check(roster.Length == 6, "Six starting characters");
        Check(roster.Select(x => x.characterId).Distinct().Count() == roster.Length, "Unique stable IDs");
        Check(roster.Select(x => x.startingWeapon.kind).Distinct().Count() == 6, "Six starting weapon behaviors");
        Check(roster.All(x => x.startingWeapon.kind != WeaponKind.Drone && x.startingWeapon.kind != WeaponKind.Creature),
            "Autonomous summons are rewards, not starting classes");
        Check(roster.All(x => !x.startingWeapon.IsAuxiliary), "No autonomous weapon is used as a primary");
        Check(roster.Single(x => x.characterId == "01_warrior").startingWeapon.kind == WeaponKind.Axe, "Warrior starts with the active axe");
        foreach (CharacterDefinition character in roster)
        {
            Check(CharacterCatalog.Resolve(roster, character.characterId, 0) == character, "ID resolves across scenes");
            Check(character.playerPrefab != null && character.startingWeapon != null, "Linked prefab and weapon");
            Check(character.playerPrefab.GetComponent<PlayerController>() != null &&
                character.playerPrefab.GetComponent<Rigidbody2D>() != null &&
                character.playerPrefab.GetComponent<PlayerHealth>() != null,
                character.characterId + " must reference a playable prefab, not an image-only preview");
            Check(!string.IsNullOrEmpty(character.passive) && !string.IsNullOrEmpty(character.limitation), "Distinct traits exposed");
            GameObject root = UnityEngine.Object.Instantiate(character.playerPrefab);
            root.SetActive(false);
            try
            {
                PlayerController player = root.GetComponent<PlayerController>();
                Check(player != null && player.bulletPrefab.GetComponent<Bullet>() != null && player.bombPrefab.GetComponent<Bomb>() != null,
                    "Combat prefab references");
                character.ApplyTo(player);
                PlayerStats stats = root.GetComponent<PlayerStats>();
                typeof(PlayerController).GetProperty("Stats").SetValue(player, stats);
                PlayerHealth health = root.GetComponent<PlayerHealth>();
                Near(health.maxHealth, character.maxHealth, "Health applied before Start");
                Near(stats.GetMoveSpeed(player.moveSpeed), character.moveSpeed, "Preview and game movement agree");
                PlayerWeaponSystem weapon = root.GetComponent<PlayerWeaponSystem>();
                Check(weapon.Definition == character.startingWeapon, "Starting weapon equipped");
                float before = weapon.Cooldown;
                stats.globalCooldownMultiplier = 0.8f;
                Near(weapon.Cooldown, before * 0.8f, "Equipped weapons use cooldown buffs");
                stats.levelUpDamage = 0.02f;
                float damage = stats.damageMultiplier;
                stats.OnLevelUp(health);
                Near(stats.damageMultiplier - damage, 0.02f * character.damageGrowthMultiplier, "Character growth scales future buffs");
                if (character.startingWeapon.kind == WeaponKind.Bow)
                {
                    GameObject arrow = new GameObject("ArrowCheck");
                    arrow.SetActive(false);
                    try
                    {
                        stats.pierceCount = 2;
                        Bullet bullet = arrow.AddComponent<Bullet>();
                        bullet.Initialize(Vector2.right, player, character.startingWeapon.pierce);
                        int hits = (int)typeof(Bullet).GetField("remainingHits", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(bullet);
                        Check(hits == 4, "Innate bow pierce stacks with two buff pierces");
                    }
                    finally { UnityEngine.Object.DestroyImmediate(arrow); }
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        Check(CharacterCatalog.Resolve(roster, "removed-character", 999) == roster[roster.Length - 1], "Invalid saved selection is clamped");
        Check(CharacterCatalog.Resolve(Array.Empty<CharacterDefinition>(), "", 0) == null, "Legacy empty catalog fallback");
        Debug.Log("[CharacterSmokeTests] PASS: roster, references, selection, starting stats/weapons, cooldown buffs, growth, bow pierce.");
    }

    private static void Check(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException("Character smoke test failed: " + label);
    }

    private static void Near(float actual, float expected, string label)
    {
        Check(Mathf.Abs(actual - expected) < 0.0001f, label);
    }
}
