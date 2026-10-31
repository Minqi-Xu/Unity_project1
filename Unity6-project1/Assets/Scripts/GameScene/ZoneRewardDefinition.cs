using UnityEngine;

public enum ZoneRewardType { Altar, HealingSpring, WeaponWreckage, DangerousChest, ExperienceMagnet }

[CreateAssetMenu(menuName = "Survival/Zone Reward")]
public class ZoneRewardDefinition : ScriptableObject
{
    public string displayName;
    [TextArea] public string announcement;
    public ZoneRewardType rewardType;
    [Min(.1f)] public float requiredStayTime = 10f;
    [Min(0f)] public float exitGraceTime = .5f;
    [Min(.5f)] public float radius = 2.5f;
    public bool oneShot = true;
    public bool spawnEnemiesWhileInside;
    [Min(1)] public int enemyWaveSize = 8;
    public Color color = Color.cyan;

    public static ZoneRewardDefinition[] CreateDefaults()
    {
        string[] names = { "Ancient Altar", "Healing Spring", "Weapon Wreckage", "Dangerous Chest", "Experience Field" };
        string[] hints = { "An ancient power calls to you...", "A soothing presence stirs nearby...", "The remnants of a weapon await...", "Treasure awaits, but danger draws near...", "Experience energy gathers in the air..." };
        float[] times = { 10f, 5f, 8f, 20f, 6f };
        Color[] colors = { new Color(.7f,.4f,1f), new Color(.25f,1f,.65f), new Color(1f,.75f,.25f), new Color(1f,.35f,.25f), new Color(.3f,.75f,1f) };
        var result = new ZoneRewardDefinition[5];
        for (int i = 0; i < result.Length; i++)
        {
            var item = CreateInstance<ZoneRewardDefinition>();
            item.name = "Zone_" + (ZoneRewardType)i;
            item.displayName = names[i]; item.announcement = hints[i];
            item.rewardType = (ZoneRewardType)i; item.requiredStayTime = times[i];
            item.color = colors[i]; item.spawnEnemiesWhileInside = i == 3;
            result[i] = item;
        }
        return result;
    }
}
