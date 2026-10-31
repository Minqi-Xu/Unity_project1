using UnityEngine;

public partial class PlayerWeaponSystem
{
    private bool CanEmitWave => isActiveAndEnabled && owner != null && owner.Stats != null &&
        (owner.GetComponent<PlayerHealth>() == null || owner.GetComponent<PlayerHealth>().currentHealth > 0f);

    private void FirePrimaryWaves(Vector2 direction)
    {
        if (Definition.kind != WeaponKind.Shotgun)
        {
            if (Definition.kind == WeaponKind.Bomb) LaunchBomb(direction);
            else FireProjectiles(transform.position, direction);
            return;
        }
        int count = AttackWaveCount;
        float interval = WeaponWaveRules.Interval(WeaponWaveRules.DefaultInterval, Cooldown, count);
        StartCoroutine(WeaponWaveRules.Emit(count, interval, () => this != null && CanEmitWave, () =>
        {
            if (Definition.kind == WeaponKind.Bomb) LaunchBomb(direction);
            else FireProjectiles(transform.position, direction);
        }));
    }

    private void LaunchBomb(Vector2 direction)
    {
        if (owner.bombPrefab == null) return;
        GameObject shot = Instantiate(owner.bombPrefab, transform.position, Quaternion.identity);
        Bomb bomb = shot.GetComponent<Bomb>();
        if (bomb == null) { Destroy(shot); return; }
        bomb.basedamage = Definition.damage;
        bomb.explosionRadius = Definition.radius;
        bomb.maxDistance = Definition.range;
        bomb.Initialize(direction, owner, this);
        Destroy(shot, 10f);
    }
}
