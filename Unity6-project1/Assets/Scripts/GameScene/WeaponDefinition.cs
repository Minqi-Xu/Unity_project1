using UnityEngine;

public enum WeaponKind { Pistol, Shotgun, Bow, Melee, Bomb, Turret, Drone, Creature, FireOrb, Lightning }

[CreateAssetMenu(menuName = "Survival/Weapon")]
public class WeaponDefinition : ScriptableObject
{
    public string displayName;
    [TextArea] public string description;
    public WeaponKind kind;
    [Min(0.02f)] public float cooldown = 0.3f;
    [Min(0f)] public float damage = 8f;
    [Min(1)] public int projectiles = 1;
    [Range(0f, 120f)] public float spread = 8f;
    [Min(0)] public int pierce;
    [Min(0.1f)] public float projectileSpeed = 25f;
    [Min(0.1f)] public float range = 12f;
    [Min(0.1f)] public float radius = 2f;
    [Range(0f, .95f)] public float explosionCoreFraction = .35f;
    [Range(0f, 1f)] public float explosionEdgeDamageMultiplier = .25f;
    [Min(1)] public int summonCount = 1;
    [Min(0.1f)] public float summonSpeed = 7f;
    [Min(1f)] public float summonLifetime = 30f;
    [Min(0.1f)] public float deploymentCooldown = 2.5f;
    public Color tint = Color.white;
    public bool Automatic => kind == WeaponKind.Melee || kind == WeaponKind.Drone || kind == WeaponKind.Creature
        || kind == WeaponKind.FireOrb || kind == WeaponKind.Lightning;
}
