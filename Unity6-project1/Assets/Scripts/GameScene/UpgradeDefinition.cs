using UnityEngine;

// ScriptableObject data asset that defines one upgrade option and its effects.
[CreateAssetMenu(fileName = "UpgradeDefinition", menuName = "Roguelite/Upgrade Definition")]
public class UpgradeDefinition : ScriptableObject
{
    // Stable id used for upgrade level tracking and future save data.
    public string upgradeId;

    // Name shown to the player in upgrade UI.
    public string displayName;

    // Description shown to the player in upgrade UI.
    [TextArea]
    public string description;

    // Optional icon for future upgrade UI.
    public Sprite icon;

    // Upgrade rarity tier.
    public UpgradeRarity rarity = UpgradeRarity.Common;

    // Maximum times this upgrade can be applied. Use 0 or less for unlimited.
    public int maxLevel = 1;

    // Effects applied when this upgrade is selected.
    public UpgradeEffect[] effects;

    // Returns a safe display name even when displayName is empty.
    public string DisplayName
    {
        get
        {
            return string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        }
    }
}
