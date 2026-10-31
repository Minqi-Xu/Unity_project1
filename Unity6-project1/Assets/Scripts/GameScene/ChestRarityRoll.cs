using System;

// Pure rarity selection, independent of how many cards are authored in each bucket.
public static class ChestRarityRoll
{
    public static int Select(double unitRoll, float[] weights, bool[] available)
    {
        double total = 0;
        for (int i = 0; i < weights.Length; i++) if (available[i]) total += Math.Max(0f, weights[i]);
        if (total <= 0) return -1;
        double roll = Math.Max(0, Math.Min(1, unitRoll)) * total;
        int last = -1;
        for (int i = 0; i < weights.Length; i++)
        {
            if (!available[i] || weights[i] <= 0) continue;
            last = i;
            roll -= weights[i];
            if (roll < 0) return i;
        }
        return last; // Unity's Random.value may return exactly one.
    }
}
