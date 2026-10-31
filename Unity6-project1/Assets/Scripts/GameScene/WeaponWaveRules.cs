using System;
using System.Collections;
using UnityEngine;

public static class WeaponWaveRules
{
    public const float DefaultInterval = .12f;
    public static bool Supports(WeaponKind kind) => kind == WeaponKind.Shotgun;
    public static bool SupportsQuantity(WeaponKind kind) => kind == WeaponKind.Pistol || kind == WeaponKind.Shotgun ||
        kind == WeaponKind.Bow || kind == WeaponKind.Turret;
    public static int Count(int basicCount, float dedicatedBonus) => Mathf.Clamp(basicCount + Mathf.RoundToInt(dedicatedBonus), 1, 32);
    public static float Interval(float preferred, float cooldown, int count) =>
        count <= 1 ? 0f : Mathf.Min(Mathf.Max(.001f, preferred), Mathf.Max(.02f, cooldown) / count);

    // Absolute scaled-time deadlines prevent frame quantization from extending a burst past its cooldown.
    public static IEnumerator Emit(int count, float interval, Func<bool> canContinue, Action emit,
        Func<float> clock = null, Func<bool> isPaused = null)
    {
        clock = clock ?? (() => Time.time);
        isPaused = isPaused ?? (() => Time.timeScale <= 0f);
        float started = clock();
        for (int wave = 0; wave < Mathf.Clamp(count, 1, 32); wave++)
        {
            float due = started + wave * interval;
            while (clock() < due || isPaused())
            {
                if (!canContinue()) yield break;
                yield return null;
            }
            if (!canContinue()) yield break;
            emit();
        }
    }
}
