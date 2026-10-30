using UnityEngine;

public static class EnemyCatalog
{
    // Used by existing scenes without additional wiring; assets can override this on the spawner.
    public static EnemyDefinition[] CreateDefaults() => new[]
    {
        Create("Chaser", EnemyKind.Chaser, 0, 1, 1, 1, new Color(.85f,.28f,.28f)),
        Create("Runner", EnemyKind.Runner, 60, .45f, 1.65f, .75f, new Color(1f,.8f,.2f)),
        Create("Tank", EnemyKind.Tank, 120, 3f, .55f, 1.5f, new Color(.55f,.35f,.85f)),
        Create("Ranged", EnemyKind.Ranged, 360, .8f, .8f, .9f, new Color(.2f,.8f,.9f))
    };
    private static EnemyDefinition Create(string name, EnemyKind kind, float unlock, float health, float speed, float size, Color tint)
    {
        EnemyDefinition result = ScriptableObject.CreateInstance<EnemyDefinition>();
        result.name = result.displayName = name;
        result.kind = kind;
        result.unlockTime = unlock;
        result.healthMultiplier = health;
        result.speedMultiplier = speed;
        result.sizeMultiplier = size;
        result.tint = tint;
        return result;
    }
}
