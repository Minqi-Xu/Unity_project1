using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

// TMP needs a real font face, not a legacy Unity dynamic-font wrapper without embedded font data.
public static class CodexChineseFont
{
    public const string RequiredGlyphs = "图鉴角色自律武器攻击生存机动成长普通稀有史诗传说返回搜索生命伤害冷却属性0123456789CODEX";

    public static TMP_FontAsset Create()
    {
        string fontsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
        if (!string.IsNullOrEmpty(fontsDirectory))
            foreach (string file in new[] { "msyh.ttc", "simhei.ttf", "simsun.ttc" })
            {
                string path = Path.Combine(fontsDirectory, file);
                if (!File.Exists(path)) continue;
                TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(path, 0, 48, 6, GlyphRenderMode.SDFAA, 1024, 1024);
                if (Validate(asset)) return asset;
                Release(asset);
            }

        // Native TMP/FontEngine system-font lookup supports OS font faces on other platforms.
        foreach (string family in new[] { "Microsoft YaHei", "Microsoft YaHei UI", "SimHei", "Noto Sans CJK SC", "PingFang SC" })
        {
            TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(family, "Regular", 48);
            if (Validate(asset)) return asset;
            Release(asset);
        }
        Debug.LogError("Codex could not load a font face containing Chinese glyphs. Configure a Chinese TMP font asset in GameCodexUI.");
        return null;
    }

    public static bool Validate(TMP_FontAsset asset)
    {
        if (asset == null) return false;
        asset.isMultiAtlasTexturesEnabled = true;
        return asset.HasCharacters(RequiredGlyphs, out uint[] _, false, true);
    }

    public static void Release(TMP_FontAsset asset)
    {
        if (asset == null) return;
        if (asset.material != null) ReleaseObject(asset.material);
        foreach (Texture2D texture in asset.atlasTextures) if (texture != null) ReleaseObject(texture);
        ReleaseObject(asset);
    }
    private static void ReleaseObject(UnityEngine.Object value)
    {
        if (Application.isPlaying) UnityEngine.Object.Destroy(value);
        else UnityEngine.Object.DestroyImmediate(value);
    }
}
