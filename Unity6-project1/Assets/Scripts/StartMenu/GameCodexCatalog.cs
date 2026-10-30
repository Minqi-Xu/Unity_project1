using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

// Read-only catalog: uses the same factories as gameplay, without creating any run managers.
public sealed class GameCodexCatalog : IDisposable
{
    public CharacterDefinition[] Characters { get; }
    public WeaponDefinition[] Weapons { get; }
    public WeaponDefinition[] AuxiliaryWeapons { get; }
    public UpgradeDefinition[] Upgrades { get; }
    public UpgradeDefinition[] Buffs { get; }
    public static readonly string[] BuffGroups = { "Attack", "Survival", "Mobility", "Growth", "Universal & Compound", "Weapon Specific" };

    public GameCodexCatalog()
    {
        Characters = Resources.LoadAll<CharacterDefinition>("Characters").OrderBy(x => x.characterId, StringComparer.Ordinal).ToArray();
        Weapons = Resources.LoadAll<WeaponDefinition>("Weapons").OrderBy(x => x.kind).ToArray();
        AuxiliaryWeapons = Weapons.Where(x => x.IsAuxiliary).ToArray();
        Upgrades = UpgradeManager.CreateDefaultUpgradePool().Concat(WeaponUpgradeCatalog.Create()).ToArray();
        Buffs = Upgrades.Where(x => x.grantedWeapon == null).ToArray();
    }

    public IEnumerable<UpgradeDefinition> WeaponBuffs(WeaponKind kind) =>
        Buffs.Where(x => x.weaponSpecific && x.targetWeapon == kind).OrderBy(x => x.rarity).ThenBy(x => x.DisplayName, StringComparer.Ordinal);
    public IEnumerable<UpgradeDefinition> GroupBuffs(string group) =>
        Buffs.Where(x => Group(x) == group).OrderBy(x => x.rarity).ThenBy(x => x.DisplayName, StringComparer.Ordinal);
    public static string Group(UpgradeDefinition buff)
    {
        if (buff.weaponSpecific) return "Weapon Specific";
        switch (buff.category)
        {
            case UpgradeCategory.Attack: return "Attack";
            case UpgradeCategory.Survival: return "Survival";
            case UpgradeCategory.Mobility: return "Mobility";
            case UpgradeCategory.Growth: return "Growth";
            default: return "Universal & Compound";
        }
    }
    public static string Rarity(UpgradeRarity rarity) =>
        rarity == UpgradeRarity.Common ? "Common" : rarity == UpgradeRarity.Rare ? "Rare" :
        rarity == UpgradeRarity.Epic ? "Epic" : "Legendary";
    public static string WeaponName(WeaponDefinition weapon) => weapon != null ? WeaponName(weapon.kind) + " / " + weapon.displayName : "No weapon";
    public static string WeaponName(WeaponKind kind)
    {
        switch (kind)
        {
            case WeaponKind.Pistol: return "Pistol";
            case WeaponKind.Shotgun: return "Shotgun";
            case WeaponKind.Bow: return "Bow";
            case WeaponKind.Axe: return "Axe";
            case WeaponKind.Bomb: return "Bomb Launcher";
            case WeaponKind.Turret: return "Turret";
            case WeaponKind.Melee: return "Whirlwind";
            case WeaponKind.Drone: return "Drone";
            case WeaponKind.Creature: return "Spirit";
            case WeaponKind.FireOrb: return "Fireballs";
            case WeaponKind.Lightning: return "Lightning";
            default: return kind.ToString();
        }
    }
    private static string N(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    private static string P(float value) => N(value * 100f) + "%";

    public static string DescribeWeapon(WeaponDefinition weapon)
    {
        if (weapon == null) return "No weapon configured";
        StringBuilder text = new StringBuilder();
        text.AppendLine(weapon.description);
        text.AppendLine("Base values before character passives and run buffs.");
        text.AppendLine($"Base damage: {N(weapon.damage)}    Attack interval: {N(weapon.cooldown)}s");
        switch (weapon.kind)
        {
            case WeaponKind.Axe:
                text.AppendLine($"Forward reach: {N(weapon.radius)}    Side reach ratio: {N(weapon.meleeSideScale)}    Arc: {N(weapon.meleeArc)}°");
                text.AppendLine($"Windup / swing / recovery: {N(weapon.windup)} / {N(weapon.swingDuration)} / {N(weapon.recovery)}s");
                text.AppendLine("Knockback: " + N(weapon.knockback));
                break;
            case WeaponKind.Melee:
                text.AppendLine($"Attack radius: {N(weapon.radius)}    Base pulses: 1    Knockback: {N(weapon.knockback)}");
                break;
            case WeaponKind.Creature:
                text.AppendLine($"Permanent units: {weapon.summonCount}    Move speed: {N(weapon.summonSpeed)}    Melee reach: {N(weapon.radius)}    Leash range: {N(weapon.range)}");
                break;
            case WeaponKind.Drone:
                text.AppendLine($"Permanent units: {weapon.summonCount}    Projectiles per unit: {weapon.projectiles}    Follow speed: {N(weapon.summonSpeed)}    Target range: {N(weapon.range)}");
                text.AppendLine($"Projectile speed: {N(weapon.projectileSpeed)}    Base pierce: {weapon.pierce}");
                break;
            case WeaponKind.FireOrb:
            case WeaponKind.Bomb:
                text.AppendLine($"Projectiles per cast: {(weapon.kind == WeaponKind.Bomb ? 1 : weapon.projectiles)}    Explosion radius: {N(weapon.radius)}    Travel range: {N(weapon.range)}    Projectile speed: {N(weapon.projectileSpeed)}");
                if (weapon.kind == WeaponKind.Bomb)
                    text.AppendLine($"Core radius ratio: {P(weapon.explosionCoreFraction)}    Edge damage: {P(weapon.explosionEdgeDamageMultiplier)}");
                break;
            case WeaponKind.Lightning:
                text.AppendLine($"Distinct targets per cast: {weapon.projectiles}    Target range: {N(weapon.range)}    Visible targets only");
                text.AppendLine("Stays READY while searching. Cooldown starts only after a successful strike.");
                break;
            case WeaponKind.Turret:
                text.AppendLine($"Deployment cooldown: {N(weapon.deploymentCooldown)}s    Unit limit: {weapon.summonCount}    Lifetime: {N(weapon.summonLifetime)}s");
                text.AppendLine($"Projectiles per volley: {weapon.projectiles}    Projectile speed: {N(weapon.projectileSpeed)}    Target range: {N(weapon.range)}    Pierce: {weapon.pierce}");
                break;
            default:
                text.AppendLine($"Pellets per volley: {weapon.projectiles}    Spread: {N(weapon.spread)}°    Projectile speed: {N(weapon.projectileSpeed)}    Base pierce: {weapon.pierce}");
                break;
        }
        if (weapon.IsAuxiliary) text.AppendLine("Rare unlock card. Uses one auxiliary slot; default limit: 3 weapon types. Basic Multishot does not affect auxiliaries.");
        return text.ToString().TrimEnd();
    }

    public static string DescribeCharacter(CharacterDefinition character)
    {
        StringBuilder text = new StringBuilder();
        text.AppendLine(character.availableAtStart ? "Status: available at start" : "Status: configured, not yet selectable");
        text.AppendLine($"HP: {N(character.maxHealth)}    Move speed: {N(character.moveSpeed)}    Difficulty: {character.difficulty}");
        text.AppendLine("Passive: " + character.passive);
        text.AppendLine("Limitation: " + character.limitation);
        text.AppendLine($"Global damage multiplier: x{N(character.damageMultiplier)}    Primary damage multiplier: x{N(character.primaryDamageMultiplier)}");
        text.AppendLine($"Primary cooldown multiplier: x{N(character.attackCooldownMultiplier)}    Bomb cooldown multiplier: x{N(character.bombCooldownMultiplier)}");
        text.AppendLine($"Explosion radius multiplier: x{N(character.explosionRadiusMultiplier)}    Dash cooldown multiplier: x{N(character.dashCooldownMultiplier)}    Damage growth multiplier: x{N(character.damageGrowthMultiplier)}");
        text.AppendLine($"Initial damage reduction: {P(character.damageReduction)}    Bonus critical chance: {P(character.criticalChance)}    Bonus healing per 5s: {N(character.regeneration)}");
        PlayerController player = character.playerPrefab != null ? character.playerPrefab.GetComponent<PlayerController>() : null;
        if (player != null)
        {
            text.AppendLine($"Dash cooldown: {N(player.dashCooldown * character.dashCooldownMultiplier)}s, speed: {N(player.dashSpeed)}, duration: {N(player.dashDuration)}s");
            text.AppendLine($"RMB bomb cooldown: {N(player.bombCooldown * character.bombCooldownMultiplier)}s");
            Bomb bomb = player.bombPrefab != null ? player.bombPrefab.GetComponent<Bomb>() : null;
            if (bomb != null) text.AppendLine($"RMB bomb base damage: {N(bomb.basedamage)}    Starting blast radius: {N(bomb.explosionRadius * character.explosionRadiusMultiplier)}");
        }
        text.AppendLine("\nStarting weapon: " + WeaponName(character.startingWeapon));
        text.Append(DescribeWeapon(character.startingWeapon));
        return text.ToString();
    }

    public static string DescribeBuff(UpgradeDefinition buff)
    {
        if (buff.grantedWeapon != null) return "Acquire auxiliary weapon: " + WeaponName(buff.grantedWeapon);
        if (buff.weaponSpecific) return "Applies only to " + WeaponName(buff.targetWeapon) + ".\n" + buff.description;
        if (buff.effects == null) return buff.description;
        return string.Join("\n", buff.effects.Select(effect => DescribeEffect(effect, buff.rarity)));
    }

    private static string DescribeEffect(UpgradeEffect effect, UpgradeRarity rarity)
    {
        if (effect.effectType == UpgradeEffectType.RandomHigherRarityBuffs)
            return "Immediately gain " + N(effect.value) + " different " +
                Rarity((UpgradeRarity)((int)rarity + 1)) + " basic buffs. Both consume their normal family levels.";
        return UpgradeManager.DescribeEffect(effect);
    }

    public string ExportMarkdown()
    {
        StringBuilder text = new StringBuilder("# Complete Game Codex\n\n");
        text.AppendLine($"{Characters.Length} characters, {AuxiliaryWeapons.Length} auxiliary weapons, {Buffs.Length} buff rarity variants, plus {Upgrades.Count(x => x.grantedWeapon != null)} weapon unlock cards.\n");
        text.AppendLine("Values are per acquisition. Rarity variants share family level caps. Weapon-specific buffs require the corresponding equipped weapon.\n\n## Characters\n");
        foreach (CharacterDefinition character in Characters)
            text.AppendLine("### " + character.displayName + "\n\n" + DescribeCharacter(character) + "\n");
        text.AppendLine("## Auxiliary Weapons\n");
        foreach (WeaponDefinition weapon in AuxiliaryWeapons)
        {
            text.AppendLine("### " + WeaponName(weapon) + "\n\n" + DescribeWeapon(weapon) + "\n");
            AppendGroups(text, WeaponBuffs(weapon.kind));
        }
        text.AppendLine("## All Buffs: Type, then Rarity\n");
        foreach (string group in BuffGroups)
        {
            text.AppendLine("### " + group + "\n");
            AppendGroups(text, GroupBuffs(group));
        }
        return text.ToString();
    }
    private static void AppendGroups(StringBuilder text, IEnumerable<UpgradeDefinition> buffs)
    {
        foreach (var rarity in buffs.GroupBy(x => x.rarity).OrderBy(x => x.Key))
        {
            text.AppendLine("#### " + Rarity(rarity.Key) + "\n");
            foreach (UpgradeDefinition buff in rarity)
                text.AppendLine("- **" + buff.DisplayName + "**" + (buff.weaponSpecific ? " | " + WeaponName(buff.targetWeapon) : "") +
                    " | Max level " + (buff.maxLevel > 0 ? buff.maxLevel.ToString() : "Unlimited") + ": " + DescribeBuff(buff).Replace("\n", "; ") + "\n");
        }
    }
    public void Dispose()
    {
        foreach (UpgradeDefinition buff in Upgrades)
            if (buff != null) { if (Application.isPlaying) UnityEngine.Object.Destroy(buff); else UnityEngine.Object.DestroyImmediate(buff); }
    }
}
