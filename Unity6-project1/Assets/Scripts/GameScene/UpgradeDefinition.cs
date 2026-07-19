using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeDefinition", menuName = "Roguelite/Upgrade Definition")]
public class UpgradeDefinition : ScriptableObject
{
    public string upgradeId;
    public string displayName;
    [TextArea]
    public string description;
    public Sprite icon;
    public UpgradeRarity rarity = UpgradeRarity.Common;
    public int maxLevel = 1;
    public UpgradeEffect[] effects;

    public string DisplayName
    {
        get
        {
            return string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        }
    }
}
