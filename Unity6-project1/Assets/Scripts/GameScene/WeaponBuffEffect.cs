// Separate from global PlayerStats: each effect belongs to one equipped weapon instance.
public enum WeaponBuffEffect
{
    Damage, Cooldown, Count, Radius,
    PistolCombo, PistolRamp, PistolKillShot,
    ShotgunChoke, ShotgunClose, ShotgunConcentrated,
    BowPierce, BowPierceDamage, BowCharge, BowReturn,
    MeleeKnockback, MeleeCombo, MeleeHeal,
    BombCluster, BombRepeat, BombBurn, BombCenter,
    TurretLifetime, TurretDistance, TurretDeployment, TurretLink, TurretExpiry,
    DroneOrbit, DroneFocus, DroneIndependent, DroneBurst, DroneOverload,
    CreatureSpeed, CreatureReach, CreaturePounce, CreatureBleed, CreatureBreed,
    LightningRepeat,
    AxeArc, AxeWindup, AxeExecute
}
