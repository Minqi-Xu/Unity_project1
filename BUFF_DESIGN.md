# Buff catalog and rules

The default runtime catalog contains 20 basic families (80 rarity variants), plus
14 universal/synergy variants. It is used when UpgradeManager.availableUpgrades
is empty. Weapon unlocks and 52 dedicated weapon buffs are appended to the default
or configured pool. See [WEAPON_BUFF_DESIGN.md](WEAPON_BUFF_DESIGN.md).

## Basic buffs (Common values)

| Category | Buff | Effect |
| --- | --- | --- |
| Attack | Primary Weapon Damage | Primary damage +10%; excludes auxiliaries and RMB bombs |
| Attack | Primary Weapon Attack Speed | Primary attack frequency +8%; cooldown divided by speed multiplier |
| Attack | Multishot | +1 projectile per shot |
| Attack | Piercing Rounds | +1 additional target per bullet |
| Attack | Blast Radius | Explosion radius +15% |
| Attack | Bomb Tech | Bomb cooldown -10% |
| Attack | Precision | Critical chance +5 percentage points |
| Attack | Deadly Aim | Critical damage multiplier +0.25x |
| Survival | Vitality | Max health +20, heal 20 |
| Survival | Armor Training | Damage reduction +8 percentage points |
| Survival | Regeneration | Heal 1 every 5 active seconds |
| Survival | Second Wind | Post-hit immunity +0.2 seconds |
| Mobility | Battle Boots | Movement speed bonus +8% |
| Mobility | Dash Practice | Dash cooldown -10% |
| Mobility | Long Dash | Dash speed bonus +15%, unchanged duration |
| Mobility | Magnet | Pickup radius bonus +20% |
| Growth | Scholar | Experience bonus +15% |
| Growth | Swift Pickups | Attraction speed bonus +20% |
| Growth | Renewal | Future level-ups heal 10% of maximum health |
| Growth | Potential | Future level-ups add 2% global damage bonus |

Precision and Deadly Aim complete the requested list of 18 to 20 basics.
Basic continuous effects scale by Common/Rare/Epic/Legendary = 1/1.5/2/3.
Projectile and pierce bonuses use +1/+2/+3/+4 instead of fractions.
Family caps: damage 10, multishot and piercing 3, other basics 5.
Rarities share a family's level cap, and offers never repeat a family.

## High-rarity buffs

- Universal Power (Rare+): +15/22.5/30% global damage bonus, max 5.
- Universal Haste (Rare+): -10/15/20% all weapon and dash cooldowns, max 5.
- Fortune Bundle (Rare/Epic): immediately grants two distinct Epic/Legendary
  basic buffs respectively. No recursive bundles or capped families. Hidden
  when fewer than two eligible families remain. Shared max level 3.
- Piercing Blast (Epic/Legendary): +1/+2 pierce and an explosion at the final
  allowed hit for 100/150% bullet damage. Base radius 2, affected by Blast Radius.
  The final target can take both direct and area damage; explosion cannot chain.
- Dash Frenzy (Epic/Legendary): +16/24% movement speed bonus and +30/45% attack
  speed for 3 seconds after a dash ends. The attack-speed bonus is multiplied
  by the current movement-speed multiplier. Repeated dashes refresh duration.
- Desperate Precision (Epic/Legendary): missing health fraction times 50/75
  percentage points of extra critical chance. At half health: +25/37.5 points.

Each synergy family can be acquired once regardless of rarity.

## Combat and stacking

- Damage = weapon base * legacy multiplier * global multiplier * primary-only
  multiplier (primary weapon only) * critical multiplier (on a successful roll).
- Base critical chance is 0%; base critical multiplier is 2x. Chance caps at 100%.
- Stat and primary attack-speed bonuses add within their own bucket. Cooldown reductions multiply.
  Per-bucket cooldown multipliers bottom out at 0.1. Final cooldown floors are
  0.02s for bullets and 0.05s for bombs/dashes. Damage reduction caps at 90%.
- Pierce 0 hits one target; pierce N hits N+1 distinct enemies. The penetration
  budget is captured when firing; damage modifiers are evaluated on impact.
- Additional bullets form a symmetric fan, limited to 60 degrees total.
- Timers use scaled gameplay time. No regeneration while paused or after death.
- Growth rewards apply only to levels earned after acquiring the buff, not the
  level that offered it. A multi-level gain evaluates owned bonuses per level.
- Fortune Bundle grants are reported in the Console with name and rarity.

## Validation

Experience progress now stores both current experience and the next-level threshold
as floats. RewardManager passes the full boosted amount to the player without integer
rounding or a separate remainder bucket. The top-left HUD shows a horizontal blue
experience bar, current integer level and next-level percentage. For example, 1 XP
with a 15% bonus fills 23% of a 5-XP threshold. Multi-level grants preserve leftover XP.
Run `Tools > Player > Run Experience Smoke Tests` for fractional reward, threshold,
overflow and rounding checks; `Tests/ExperienceMathChecks.cs` tests the production
progress and boundary helpers independently. Play Mode layout verification remains pending.

The pause menu now includes an acquired-buff list. Each family has one named row
with its acquired level; hover or keyboard focus shows the exact rarity stacks
and each stack's per-pick description. Long lists and descriptions scroll
independently. Bundle rewards and auxiliary unlocks are included, failed grants
are excluded, and the list starts empty on each new run. The most recently
inspected detail remains visible so it can be scrolled. This is an acquisition
history, including instant rewards, rather than a recalculated live stat sheet.
The panel leaves the top HUD band clear for survival time. Names and edge accents
use the highest acquired rarity: Common green, Rare blue, Epic purple, Legendary
gold; acquired auxiliary weapon names override rarity with red, while their buffs
retain their rarity colors. Hover details retain the actual mixed-rarity breakdown. New auxiliary weapon
choices use red titles, red badges/borders and a dark red card background.

Run `Tools > Buffs > Run Acquired List Smoke Tests` outside Play Mode for mixed
rarity grouping, snapshot descriptions, cap rejection and bundle acquisition
tracking. Play Mode checks still needed: open pause with no buffs, acquire mixed
rarities, hover names, scroll both panes, resume/count down, and restart the run.

Run Unity menu `Tools > Buffs > Run Smoke Tests` outside Play Mode to check the
catalog, rarity scaling, damage and critical formulas, growth, cooldowns,
shared caps, unique offers, immunity, and bundle exhaustion.

Play Mode checks still needed: fire multishot into several enemies; verify
penetration and final-hit explosion; check a multi-collider enemy takes each
area hit once; wait for regeneration and pause midway; dash and observe the
three-second attack-speed window; inspect long descriptions in the scrollable
selection window at the supported resolutions.
