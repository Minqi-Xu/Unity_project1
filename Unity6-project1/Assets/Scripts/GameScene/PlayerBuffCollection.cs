using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// A per-run acquisition ledger; snapshot descriptions preserve mixed rarity stacks.
[DisallowMultipleComponent]
public class PlayerBuffCollection : MonoBehaviour
{
    public sealed class Stack
    {
        public UpgradeRarity Rarity { get; internal set; }
        public string Description { get; internal set; }
        public int Count { get; internal set; }
    }

    public sealed class Entry
    {
        public string Key { get; internal set; }
        public string Name { get; internal set; }
        public int Level { get; internal set; }
        public int MaxLevel { get; internal set; }
        public bool IsWeaponUnlock { get; internal set; }
        public Color DisplayColor => IsWeaponUnlock ? UpgradeUIColors.NewWeapon : UpgradeUIColors.ForRarity(HighestRarity);
        private readonly List<Stack> stacks = new List<Stack>();
        public IReadOnlyList<Stack> Stacks => stacks.AsReadOnly();
        public string LevelLabel => MaxLevel > 0 ? "Lv " + Level + "/" + MaxLevel : "Lv " + Level;
        public UpgradeRarity HighestRarity
        {
            get
            {
                UpgradeRarity highest = UpgradeRarity.Common;
                foreach (Stack stack in stacks)
                    if ((int)stack.Rarity > (int)highest) highest = stack.Rarity;
                return highest;
            }
        }

        internal void Record(UpgradeDefinition definition)
        {
            Level++;
            IsWeaponUnlock |= definition.grantedWeapon != null;
            string description = definition.description ?? "";
            Stack stack = stacks.Find(item => item.Rarity == definition.rarity && item.Description == description);
            if (stack == null)
            {
                stack = new Stack { Rarity = definition.rarity, Description = description };
                stacks.Add(stack);
            }
            stack.Count++;
        }

        public string Details()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine(Name).AppendLine(LevelLabel).AppendLine();
            text.AppendLine("ACQUIRED EFFECTS (per pick)");
            foreach (Stack stack in stacks)
            {
                text.AppendLine().Append(stack.Rarity).Append(" x").Append(stack.Count).AppendLine();
                text.AppendLine(string.IsNullOrWhiteSpace(stack.Description) ? "No additional description." : stack.Description);
            }
            return text.ToString().TrimEnd();
        }
    }

    private readonly List<Entry> entries = new List<Entry>();
    private readonly Dictionary<string, Entry> byKey = new Dictionary<string, Entry>();
    public IReadOnlyList<Entry> Entries => entries.AsReadOnly();
    public event Action Changed;

    // Called only after the upgrade succeeds, including grants made by reward bundles.
    public void Record(UpgradeDefinition definition, string key)
    {
        if (definition == null || string.IsNullOrWhiteSpace(key)) return;
        if (!byKey.TryGetValue(key, out Entry entry))
        {
            entry = new Entry { Key = key, Name = definition.DisplayName, MaxLevel = definition.maxLevel };
            byKey.Add(key, entry);
            entries.Add(entry);
        }
        entry.Record(definition);
        Changed?.Invoke();
    }

    public static PlayerBuffCollection Ensure(PlayerController player)
    {
        PlayerBuffCollection collection = player.GetComponent<PlayerBuffCollection>();
        return collection != null ? collection : player.gameObject.AddComponent<PlayerBuffCollection>();
    }
}
