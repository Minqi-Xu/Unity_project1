using System;
using System.Collections.Generic;

public static class ChestRewardChecks
{
    public static void Run()
    {
        float[] weights = { 25, 55, 15, 5 };
        bool[] all = { true, true, true, true };
        Check(ChestRarityRoll.Select(0, weights, all) == 0, "Inclusive lower endpoint");
        Check(ChestRarityRoll.Select(1, weights, all) == 3, "Inclusive upper endpoint");
        Check(ChestRarityRoll.Select(.25, weights, all) == 1, "Rare/Epic boundary");
        Check(ChestRarityRoll.Select(.8, weights, all) == 2, "Epic/Legendary boundary");
        Check(ChestRarityRoll.Select(.95, weights, all) == 3, "Legendary/Myth boundary");
        Check(ChestRarityRoll.Select(.8, new float[4], all) == -1, "Zero weights use caller fallback");
        Check(ChestRarityRoll.Select(0, weights, new[] { false, false, false, true }) == 3, "Missing buckets renormalize");
        Check(ChestRarityRoll.Select(.5, weights, new bool[4]) == -1, "Exhausted buckets");
        int[] counts = new int[4]; Random random = new Random(91273);
        for (int i = 0; i < 80000; i++) counts[ChestRarityRoll.Select(random.NextDouble(), weights, all)]++;
        for (int i = 0; i < 4; i++) Check(Math.Abs(counts[i] / 80000.0 - weights[i] / 100.0) < .01, "Rarity distribution " + i);
        HashSet<int> seeds = new HashSet<int>();
        for (int i = 0; i < 32; i++) seeds.Add(RunRandomSeed.CreateSeed());
        Check(seeds.Count >= 28, "Rapid consecutive runs receive fresh seeds");
        Console.WriteLine("PASS: chest rarity odds, exact boundaries, unavailable/zero buckets and fresh time/entropy seeds.");
    }
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
}
