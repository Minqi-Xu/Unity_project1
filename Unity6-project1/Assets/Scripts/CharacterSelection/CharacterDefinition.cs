using UnityEngine;

[CreateAssetMenu(menuName = "Survival/Character")]
public class CharacterDefinition : ScriptableObject
{
    public string characterId;
    public string displayName;
    [TextArea] public string passive;
    [TextArea] public string limitation;
    public string difficulty = "Normal";
    public GameObject playerPrefab;
    public WeaponDefinition startingWeapon;
    public Color tint = Color.white;
    [Min(1f)] public float maxHealth = 100f;
    [Min(0.1f)] public float moveSpeed = 5f;
    public float damageMultiplier = 1f;
    public float primaryDamageMultiplier = 1f;
    public float attackCooldownMultiplier = 1f;
    public float bombCooldownMultiplier = 1f;
    public float explosionRadiusMultiplier = 1f;
    public float dashCooldownMultiplier = 1f;
    public float damageGrowthMultiplier = 1f;
    [Range(0f, 0.9f)] public float damageReduction;
    [Range(0f, 1f)] public float criticalChance;
    [Min(0f)] public float regeneration;

    // Called once by the spawner, before health/controller Start initialize the run.
    public void ApplyTo(PlayerController player)
    {
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats == null) stats = player.gameObject.AddComponent<PlayerStats>();
        player.moveSpeed = moveSpeed;
        stats.damageMultiplier *= damageMultiplier;
        stats.bulletDamageMultiplier *= primaryDamageMultiplier;
        stats.fireCooldownMultiplier *= attackCooldownMultiplier;
        stats.bombCooldownMultiplier *= bombCooldownMultiplier;
        stats.explosionRadiusMultiplier *= explosionRadiusMultiplier;
        stats.dashCooldownMultiplier *= dashCooldownMultiplier;
        stats.levelDamageGrowthMultiplier *= damageGrowthMultiplier;
        stats.damageReduction = Mathf.Clamp(stats.damageReduction + damageReduction, 0f, 0.9f);
        stats.criticalChance = Mathf.Clamp01(stats.criticalChance + criticalChance);
        stats.regeneration += regeneration;
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null) health.maxHealth = maxHealth;
        SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
        if (renderer != null) renderer.color = tint;
        if (startingWeapon != null)
        {
            PlayerWeaponSystem system = player.GetComponent<PlayerWeaponSystem>();
            if (system == null) system = player.gameObject.AddComponent<PlayerWeaponSystem>();
            system.Configure(player, startingWeapon);
        }
    }
}
