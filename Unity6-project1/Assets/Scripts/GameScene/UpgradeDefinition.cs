using UnityEngine;

/// <summary>
/// ScriptableObject data asset that defines one upgrade option and its effects.
/// </summary>
[CreateAssetMenu(fileName = "UpgradeDefinition", menuName = "Roguelite/Upgrade Definition")]
public class UpgradeDefinition : ScriptableObject
{
    /// <summary>Stable id used for upgrade level tracking and future save data.</summary>
    public string upgradeId;

    /// <summary>Name shown to the player in upgrade UI.</summary>
    public string displayName;

    /// <summary>Description shown to the player in upgrade UI.</summary>
    [TextArea]
    public string description;

    /// <summary>Optional icon for future upgrade UI.</summary>
    public Sprite icon;

    /// <summary>Upgrade rarity tier.</summary>
    public UpgradeRarity rarity = UpgradeRarity.Common;

    /// <summary>Maximum times this upgrade can be applied. Use 0 or less for unlimited.</summary>
    public int maxLevel = 1;

    /// <summary>Effects applied when this upgrade is selected.</summary>
    public UpgradeEffect[] effects;

    /// <summary>
    /// Returns a safe display name even when displayName is empty.
    /// </summary>
    public string DisplayName
    {
        get
        {
            return string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        }
    }
}
