using System;
using UnityEngine;

public static class CharacterCatalog
{
    public const string SelectionKey = "SelectedCharacterId";

    public static CharacterDefinition[] Load()
    {
        CharacterDefinition[] entries = Resources.LoadAll<CharacterDefinition>("Characters");
        Array.Sort(entries, (a, b) => string.CompareOrdinal(a.characterId, b.characterId));
        return entries;
    }

    public static CharacterDefinition Resolve(CharacterDefinition[] entries, string id, int legacyIndex)
    {
        if (entries == null || entries.Length == 0) return null;
        foreach (CharacterDefinition entry in entries)
            if (entry != null && entry.characterId == id) return entry;
        return entries[Mathf.Clamp(legacyIndex, 0, entries.Length - 1)];
    }
}
