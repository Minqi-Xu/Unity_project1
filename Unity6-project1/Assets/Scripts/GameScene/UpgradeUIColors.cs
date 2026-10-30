using UnityEngine;

// Shared rarity palette for reward cards and the acquired-buff list.
public static class UpgradeUIColors
{
    public static Color NewWeapon => new Color(1f, .34f, .32f);

    public static Color ForRarity(UpgradeRarity rarity)
    {
        switch (rarity)
        {
            case UpgradeRarity.Rare: return new Color(.3f, .75f, 1f);
            case UpgradeRarity.Epic: return new Color(.8f, .52f, 1f);
            case UpgradeRarity.Legendary: return new Color(1f, .76f, .25f);
            default: return new Color(.65f, .87f, .7f);
        }
    }
}
