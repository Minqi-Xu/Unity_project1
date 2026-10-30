using System;
using System.Linq;
using UnityEngine;

public static class CharacterCatalog
{
    public const string SelectionKey = "SelectedCharacterId";

    public static float GetStartingAverageMoveSpeed(CharacterDefinition[] entries)
    {
        float total = 0f;
        int count = 0;
        if (entries != null)
            foreach (CharacterDefinition entry in entries)
                if (entry != null && entry.availableAtStart && entry.moveSpeed > 0f)
                {
                    total += entry.moveSpeed;
                    count++;
                }
        return count > 0 ? total / count : 5f;
    }

    public static CharacterDefinition[] Load()
    {
        CharacterDefinition[] entries = Resources.LoadAll<CharacterDefinition>("Characters")
            .Where(entry => entry.availableAtStart).ToArray();
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
