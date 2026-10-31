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
            case UpgradeRarity.Myth: return new Color(.5f, 1f, .95f);
            default: return new Color(.65f, .87f, .7f);
        }
    }

    public static void StyleMythCard(GameObject card)
    {
        card.GetComponent<UnityEngine.UI.Image>().color = Color.white;
        if (card.GetComponent<MythCardBackground>() == null) card.AddComponent<MythCardBackground>();
        foreach (TMPro.TMP_Text label in card.GetComponentsInChildren<TMPro.TMP_Text>(true)) StyleMythText(label);
    }

    public static void StyleMythText(TMPro.TMP_Text label)
    {
        label.color = Color.white;
        label.overrideColorTags = true;
        label.enableVertexGradient = true;
        label.colorGradient = new TMPro.VertexGradient(new Color(.5f, 1f, 1f), new Color(1f, .6f, .95f),
            new Color(1f, .92f, .5f), new Color(.65f, 1f, .6f));
    }
}
