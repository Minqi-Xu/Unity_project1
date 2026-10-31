using UnityEditor;
using UnityEngine;

public static class ZoneRewardPreview
{
    [MenuItem("Tools/Zones/Spawn First Wave Now (Play Mode)")]
    public static void FirstWave() => Spawn(false);

    [MenuItem("Tools/Zones/Spawn Sealed Wave Now (Play Mode)")]
    public static void SealedWave() => Spawn(true);

    private static void Spawn(bool sealedWave)
    {
        if (!Application.isPlaying || ZoneRewardDirector.Instance == null)
        {
            Debug.LogWarning("Enter Play Mode in GameScene before previewing map events.");
            return;
        }
        ZoneRewardDirector.Instance.SpawnWave(sealedWave);
    }
}
