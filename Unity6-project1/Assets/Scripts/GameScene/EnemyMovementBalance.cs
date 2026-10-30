using System;

// Compare unscaled speeds; player and enemies apply the same camera scale afterwards.
public static class EnemyMovementBalance
{
    public const float MaximumRelativeSpeed = 1.05f;
    public const float MaximumBossRelativeSpeed = 1.2f;

    public static float LimitSpeed(float requestedSpeed, float startingCharacterAverage, EnemyRank rank, float relativeSpeedLimit = MaximumRelativeSpeed)
    {
        float speed = Math.Max(0f, requestedSpeed);
        if (rank == EnemyRank.Normal) return speed;
        float reference = startingCharacterAverage > 0f ? startingCharacterAverage : 5f;
        float limit = rank >= EnemyRank.MiniBoss
            ? Math.Max(.1f, Math.Min(relativeSpeedLimit, MaximumBossRelativeSpeed)) : MaximumRelativeSpeed;
        return Math.Min(speed, reference * limit);
    }
}
