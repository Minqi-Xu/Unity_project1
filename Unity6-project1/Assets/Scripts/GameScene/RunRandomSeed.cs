using System;
using System.Diagnostics;
using System.Security.Cryptography;

public static class RunRandomSeed
{
    // Reseed once per new run, never per draw/frame. Time alone can repeat on rapid restarts.
    public static int Initialize()
    {
        int seed = CreateSeed();
        UnityEngine.Random.InitState(seed);
        return seed;
    }

    public static int CreateSeed()
    {
        byte[] entropy = new byte[4];
        using (RandomNumberGenerator source = RandomNumberGenerator.Create()) source.GetBytes(entropy);
        long ticks = DateTime.UtcNow.Ticks;
        int seed = unchecked((int)ticks ^ (int)(ticks >> 32) ^ (int)Stopwatch.GetTimestamp() ^ BitConverter.ToInt32(entropy, 0));
        return seed;
    }
}
