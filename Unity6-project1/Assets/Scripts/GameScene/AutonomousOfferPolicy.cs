using UnityEngine;

// One roll per level-up offer. Offering a weapon resets pity even if declined.
[System.Serializable]
public class AutonomousOfferPolicy
{
    [Range(0f, 1f)] public float initialChance = 0.4f;
    [Range(0f, 1f)] public float chancePerMiss = 0.1f;
    [Min(0)] public int missesBeforeGuarantee = 4;
    public float Chance(int misses) => misses >= Mathf.Max(0, missesBeforeGuarantee)
        ? 1f : Mathf.Clamp01(initialChance + Mathf.Max(0, misses) * chancePerMiss);
    public bool ShouldOffer(int misses, float roll) => Chance(misses) >= 1f || roll < Chance(misses);
}
