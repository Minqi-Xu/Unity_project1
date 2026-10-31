using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class GameCodexSmokeTests
{
    [MenuItem("Tools/Codex/Run Catalog Smoke Tests")]
    public static void Run()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Run outside Play Mode.");
        float scale = Time.timeScale;
        UpgradeManager manager = UpgradeManager.Instance;
        RewardManager rewards = RewardManager.Instance;
        string selected = PlayerPrefs.GetString(CharacterCatalog.SelectionKey, "");
        using (GameCodexCatalog catalog = new GameCodexCatalog())
        {
            Check(catalog.Characters.Length == 8 && catalog.Characters.Count(x => x.availableAtStart) == 6, "Six selectable and two configured characters");
            Check(catalog.AuxiliaryWeapons.Length == 5 && catalog.AuxiliaryWeapons.All(x => x.IsAuxiliary), "Exactly the five droppable auxiliary weapons");
            Check(catalog.Buffs.Length == 205 && catalog.Upgrades.Length == 210, "All buffs and separate weapon unlock cards");
            Check(catalog.Upgrades.Select(x => x.upgradeId + ":" + x.rarity).Distinct().Count() == 210, "No duplicated catalog entries");
            Check(catalog.Buffs.Select(x => x.upgradeId).Distinct().Count() == 109, "One hundred nine buff families");
            Check(GameCodexCatalog.BuffGroups.Sum(x => catalog.GroupBuffs(x).Count()) == catalog.Buffs.Length, "Each buff belongs to exactly one type group");
            foreach (WeaponDefinition weapon in catalog.AuxiliaryWeapons)
            {
                var buffs = catalog.WeaponBuffs(weapon.kind).ToArray();
                Check(buffs.Length > 0 && buffs.All(x => x.targetWeapon == weapon.kind && x.weaponSpecific), "Weapon groups never mix another weapon's buffs");
                Check(GameCodexCatalog.DescribeWeapon(weapon).Contains("Attack interval"), "Weapon details contain baseline combat attributes");
                Check(buffs.Select(x => x.rarity).SequenceEqual(buffs.Select(x => x.rarity).OrderBy(x => x)), "Weapon entries group by rarity in order");
            }
            foreach (CharacterDefinition character in catalog.Characters)
            {
                string text = GameCodexCatalog.DescribeCharacter(character);
                Check(text.Contains(character.passive) && text.Contains(character.limitation) && text.Contains(character.startingWeapon.displayName), "Character page covers passive, limitation and starting weapon");
            }
            foreach (UpgradeDefinition buff in catalog.Buffs)
                Check(!string.IsNullOrWhiteSpace(GameCodexCatalog.DescribeBuff(buff)), "Every buff has a readable effect");
            string markdown = catalog.ExportMarkdown();
            Check(catalog.Buffs.All(x => markdown.Contains(x.DisplayName)), "Full export includes every buff family");
            Check(!markdown.Any(c => c >= '\u4e00' && c <= '\u9fff'), "Codex descriptions and exports are English-only");
            Check(Enum.GetValues(typeof(UpgradeRarity)).Cast<UpgradeRarity>().Select(GameCodexUI.BuffBackgroundColor).Distinct().Count() == 5,
                "All four rarities have distinct buff-card backgrounds");
            Check(Time.timeScale == scale && UpgradeManager.Instance == manager && RewardManager.Instance == rewards &&
                PlayerPrefs.GetString(CharacterCatalog.SelectionKey, "") == selected, "Opening a catalog never initializes or mutates run state");
        }
        Debug.Log("[GameCodexSmokeTests] PASS: character/weapon metadata, complete 210-entry reward pool, 205 buffs, grouping, descriptions, export and read-only lifecycle.");
    }

    [MenuItem("Tools/Codex/Run Chinese Font Smoke Test")]
    public static void CheckChineseFont()
    {
        TMPro.TMP_FontAsset font = CodexChineseFont.Create();
        try
        {
            Check(font != null && font.HasCharacters(CodexChineseFont.RequiredGlyphs), "Chinese menu and codex glyphs are actually present in the TMP atlas");
            using (GameCodexCatalog catalog = new GameCodexCatalog())
            {
                string all = catalog.ExportMarkdown() + "生存图鉴角色自律武器图鉴返回搜索当前页名称效果未开放全部个种版本滚轮浏览没有匹配的条目清空搜索词或选择其他分类状态";
                string chinese = new string(all.Where(c => c >= '\u4e00' && c <= '\u9fff').Distinct().ToArray());
                Check(font.TryAddCharacters(chinese, out string missing), "Every Chinese glyph in current catalog can be generated: " + missing);
            }
            Debug.Log("[GameCodexSmokeTests] PASS: actual TMP font face and Chinese character coverage.");
        }
        finally { CodexChineseFont.Release(font); }
    }

    [MenuItem("Tools/Codex/Export Current Catalog to Markdown")]
    public static void Export()
    {
        using (GameCodexCatalog catalog = new GameCodexCatalog())
        {
            string path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../../GAME_CODEX_EXPORT.md"));
            System.IO.File.WriteAllText(path, catalog.ExportMarkdown());
            Debug.Log("Exported full live catalog: " + path);
        }
    }
    private static void Check(bool condition, string label) { if (!condition) throw new InvalidOperationException(label); }
}
