using System.Collections.Generic;
using UnityEngine;

public static class WeaponUpgradeCatalog
{
    public static UpgradeDefinition[] Create()
    {
        List<UpgradeDefinition> result = new List<UpgradeDefinition>();
        foreach (WeaponDefinition weapon in Resources.LoadAll<WeaponDefinition>("Weapons"))
        {
            if (!weapon.IsAuxiliary) continue;
            UpgradeDefinition unlock = ScriptableObject.CreateInstance<UpgradeDefinition>();
            unlock.upgradeId = "unlock_" + weapon.kind;
            unlock.displayName = "Acquire: " + weapon.displayName;
            unlock.description = "Add an automatic auxiliary weapon without replacing your primary.\n" + weapon.description;
            unlock.rarity = UpgradeRarity.Rare;
            unlock.maxLevel = 1;
            unlock.grantedWeapon = weapon;
            unlock.effects = System.Array.Empty<UpgradeEffect>();
            result.Add(unlock);
        }
        Add(result, WeaponKind.Pistol, WeaponBuffEffect.PistolCombo, "Rhythm", "Every 5th hit gains +50% weapon damage per level.", .5f, 3);
        Add(result, WeaponKind.Pistol, WeaponBuffEffect.PistolRamp, "Warm Barrel", "Each consecutive shot reduces cooldown by 4% per level, capped at 40%. Resets after 1s idle.", .04f, 3);
        Add(result, WeaponKind.Pistol, WeaponBuffEffect.PistolKillShot, "Follow-up", "Kills fire 1 extra shot at a nearby enemy per level. Extra shots cannot trigger follow-ups.", 1f, 2);
        Add(result, WeaponKind.Shotgun, WeaponBuffEffect.ShotgunChoke, "Choke", "Reduce total spread by 15% of base per level; minimum 10 degrees.", .15f, 3);
        Add(result, WeaponKind.Shotgun, WeaponBuffEffect.Waves, "Extra Volleys", "Add 1 consecutive shotgun volley per attack per level; pellet count per volley is unchanged.", 1f, 3);
        Add(result, WeaponKind.Shotgun, WeaponBuffEffect.ShotgunClose, "Point Blank", "Gain +20% weapon damage per level within 3 units of the muzzle.", .2f, 3);
        Add(result, WeaponKind.Shotgun, WeaponBuffEffect.ShotgunConcentrated, "Concentrated Fire", "The 3rd pellet hitting one enemy in a volley gains +100% weapon damage per level.", 1f, 3);
        Add(result, WeaponKind.Bow, WeaponBuffEffect.BowPierce, "Long Arrow", "Add 1 innate pierce per level.", 1f, 3);
        Add(result, WeaponKind.Bow, WeaponBuffEffect.BowPierceDamage, "Momentum", "Each previous target pierced adds 15% weapon damage per level.", .15f, 3);
        Add(result, WeaponKind.Bow, WeaponBuffEffect.BowCharge, "Heavy Draw", "Each shot requires 0.35s of drawing and gains +60% weapon damage per level. Release early to cancel.", .6f, 2);
        Add(result, WeaponKind.Bow, WeaponBuffEffect.BowReturn, "Returning Arrow", "Arrows return after 0.45s or exhausting pierce. May hit each enemy once again on return.", 1f, 1, UpgradeRarity.Epic);
        Add(result, WeaponKind.Melee, WeaponBuffEffect.Radius, "Long Reach", "Increase attack radius by 15% of base per level.", .15f, 3);
        Add(result, WeaponKind.Melee, WeaponBuffEffect.Damage, "Whirlwind Force", "Automatic whirlwind damage +20% of base per level.", .2f, 3);
        Add(result, WeaponKind.Melee, WeaponBuffEffect.Cooldown, "Whirlwind Rhythm", "Automatic whirlwind interval -10% of base per level.", .1f, 3);
        Add(result, WeaponKind.Melee, WeaponBuffEffect.MeleeKnockback, "Repulse", "Add 0.25 units to the whirlwind's innate knockback per level.", .25f, 3);
        Add(result, WeaponKind.Melee, WeaponBuffEffect.MeleeCombo, "Echo Pulse", "Add 1 automatic pulse per cycle per level; pulses are separated by 0.12s.", 1f, 2, UpgradeRarity.Epic);
        Add(result, WeaponKind.Melee, WeaponBuffEffect.MeleeHeal, "Blood Harvest", "Whirlwind kills heal 1 HP per level, at most 5 HP per pulse cycle.", 1f, 3);
        Add(result, WeaponKind.Axe, WeaponBuffEffect.Damage, "Heavy Edge", "Axe damage +20% of base per level.", .2f, 3);
        Add(result, WeaponKind.Axe, WeaponBuffEffect.Radius, "Long Haft", "Axe forward reach and side reach +15% of base per level.", .15f, 3);
        Add(result, WeaponKind.Axe, WeaponBuffEffect.AxeArc, "Wide Cleave", "Axe arc +10 degrees per level, capped at 170 degrees.", 10f, 2);
        Add(result, WeaponKind.Axe, WeaponBuffEffect.AxeWindup, "Ready Stance", "Windup -15% of base per level; base attack interval stays unchanged.", .15f, 3);
        Add(result, WeaponKind.Axe, WeaponBuffEffect.MeleeKnockback, "Crushing Blow", "Axe knockback +0.2 units per level.", .2f, 3);
        Add(result, WeaponKind.Axe, WeaponBuffEffect.AxeExecute, "Executioner", "Gain +35% axe damage per level against enemies at or below 30% HP.", .35f, 2, UpgradeRarity.Epic);
        Add(result, WeaponKind.Bomb, WeaponBuffEffect.BombCluster, "Cluster Bomb", "After explosion, create 3 small blasts per level, each dealing 20% damage in radius 1.", 3f, 2, UpgradeRarity.Epic);
        Add(result, WeaponKind.Bomb, WeaponBuffEffect.BombRepeat, "Aftershock", "Repeat the explosion after 0.6s for 40% damage per level.", .4f, 2, UpgradeRarity.Epic);
        Add(result, WeaponKind.Bomb, WeaponBuffEffect.BombBurn, "Scorched Ground", "Leave fire for 3s, dealing 10% weapon damage per level every 0.5s.", .1f, 3);
        Add(result, WeaponKind.Bomb, WeaponBuffEffect.BombCenter, "Demolition", "Gain +40% damage per level in the inner 35% of the blast radius.", .4f, 3);
        Add(result, WeaponKind.Turret, WeaponBuffEffect.TurretLifetime, "Durable Sentry", "Add 10 seconds to turret lifetime per level, including deployed turrets.", 10f, 3);
        Add(result, WeaponKind.Turret, WeaponBuffEffect.TurretDistance, "Remote Placement", "Add 1 unit to deployment distance per level, within the camera view.", 1f, 3);
        Add(result, WeaponKind.Turret, WeaponBuffEffect.TurretDeployment, "Quick Assembly", "Reduce deployment cooldown by 15% of base per level, up to 60%.", .15f, 3);
        Add(result, WeaponKind.Turret, WeaponBuffEffect.TurretLink, "Linked Sentries", "Each other turret within 4 units adds 10% shot damage per level, up to 3 neighbors.", .1f, 3);
        Add(result, WeaponKind.Turret, WeaponBuffEffect.TurretExpiry, "Last Stand", "Natural expiry explodes in radius 2 for 150% weapon damage per level. Replacement does not trigger it.", 1.5f, 2, UpgradeRarity.Epic);
        Add(result, WeaponKind.Drone, WeaponBuffEffect.Damage, "Drone Armaments", "Floating cannon damage +20% of base per level; other weapons are unaffected.", .2f, 3);
        Add(result, WeaponKind.Drone, WeaponBuffEffect.DroneOrbit, "Wide Orbit", "Add 0.5 units to orbit radius per level.", .5f, 3);
        Add(result, WeaponKind.Drone, WeaponBuffEffect.DroneFocus, "Focus Protocol", "Drones share the nearest target around the player. Replaces Independent Protocol.", 1f, 1);
        Add(result, WeaponKind.Drone, WeaponBuffEffect.DroneIndependent, "Independent Protocol", "Prefer targets not reserved by another drone. Replaces Focus Protocol.", 1f, 1);
        Add(result, WeaponKind.Drone, WeaponBuffEffect.DroneBurst, "Burst Mode", "Fire 3 shots 0.12s apart; each deals 60% damage.", 1f, 1, UpgradeRarity.Epic);
        Add(result, WeaponKind.Drone, WeaponBuffEffect.DroneOverload, "Overload", "Every 10s, gain +25% damage and 40% shorter attack cooldown for 3s.", 1f, 1, UpgradeRarity.Epic);
        Add(result, WeaponKind.Creature, WeaponBuffEffect.Damage, "Spirit Strength", "Spirit damage +20% of base per level, including bleed and temporary spirits.", .2f, 3);
        Add(result, WeaponKind.Creature, WeaponBuffEffect.CreatureSpeed, "Swift Spirits", "Increase movement speed by 20% of base per level.", .2f, 3);
        Add(result, WeaponKind.Creature, WeaponBuffEffect.CreatureReach, "Long Claws", "Add 0.2 units to melee reach per level.", .2f, 3);
        Add(result, WeaponKind.Creature, WeaponBuffEffect.CreaturePounce, "Pounce", "Every 4s, dash toward targets within 5 units; a hit during the dash gains +50% damage.", 1f, 1, UpgradeRarity.Epic);
        Add(result, WeaponKind.Creature, WeaponBuffEffect.CreatureBleed, "Rending Claws", "Hits apply a 3s bleed: 20% weapon damage per level each second. Up to 3 stacks per enemy.", .2f, 3);
        Add(result, WeaponKind.Creature, WeaponBuffEffect.CreatureBreed, "Spirit Echo", "Kills summon a temporary spirit for 8s. At most 2 extra spirits; they cannot reproduce.", 1f, 1, UpgradeRarity.Epic);
        Add(result, WeaponKind.FireOrb, WeaponBuffEffect.Damage, "Hotter Flames", "Fireball damage +20% of base per level.", .2f, 3);
        Add(result, WeaponKind.FireOrb, WeaponBuffEffect.Count, "Flame Ring", "Add 1 radial fireball per cast per level.", 1f, 3, UpgradeRarity.Epic);
        Add(result, WeaponKind.FireOrb, WeaponBuffEffect.Radius, "Spreading Flame", "Explosion radius +15% of base per level.", .15f, 3);
        Add(result, WeaponKind.FireOrb, WeaponBuffEffect.Cooldown, "Ember Cycle", "Fireball cooldown -10% of base per level.", .1f, 3);
        Add(result, WeaponKind.Lightning, WeaponBuffEffect.Damage, "High Voltage", "Lightning damage +20% of base per level.", .2f, 3);
        Add(result, WeaponKind.Lightning, WeaponBuffEffect.Count, "Storm Front", "Strike 1 additional distinct visible enemy per cast per level.", 1f, 3, UpgradeRarity.Epic);
        Add(result, WeaponKind.Lightning, WeaponBuffEffect.LightningRepeat, "Thunder Echo", "Strike the same targets again after 0.3s for 50% damage. Dead targets are skipped.", .5f, 1, UpgradeRarity.Epic);
        Add(result, WeaponKind.Lightning, WeaponBuffEffect.Cooldown, "Gathering Storm", "Lightning cooldown -10% of base per level.", .1f, 3);
        Add(result, WeaponKind.Drone, WeaponBuffEffect.Count, "Split Cannons", "Add 1 projectile per drone volley per level; does not add drones.", 1f, 2, UpgradeRarity.Epic);
        Add(result, WeaponKind.Drone, WeaponBuffEffect.SummonCount, "Cannon Wing", "Add 1 permanent drone per level; does not increase projectiles per drone.", 1f, 2, UpgradeRarity.Epic);
        Add(result, WeaponKind.Creature, WeaponBuffEffect.SummonCount, "Spirit Pack", "Add 1 permanent spirit per level.", 1f, 2, UpgradeRarity.Epic);
        return result.ToArray();
    }

    private static void Add(List<UpgradeDefinition> pool, WeaponKind kind, WeaponBuffEffect effect,
        string title, string description, float value, int cap, UpgradeRarity rarity = UpgradeRarity.Rare)
    {
        UpgradeDefinition buff = ScriptableObject.CreateInstance<UpgradeDefinition>();
        buff.upgradeId = "weapon_" + kind + "_" + effect;
        buff.displayName = title;
        buff.description = kind + " ONLY\n" + description;
        buff.rarity = rarity;
        buff.category = UpgradeCategory.Attack;
        buff.maxLevel = cap;
        buff.weaponSpecific = true;
        buff.targetWeapon = kind;
        buff.weaponEffect = effect;
        buff.weaponValue = value;
        buff.effects = System.Array.Empty<UpgradeEffect>();
        pool.Add(buff);
    }
}
