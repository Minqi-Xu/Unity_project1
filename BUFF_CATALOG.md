# 当前全部 Buff 清单

从当前 UpgradeManager 与 WeaponUpgradeCatalog 源码生成。

共 **205 个 Buff 稀有度版本**；另有 **5 张稀有自律武器获取卡**，总奖励池 210 张。

数值为每次获取效果；同名家族不同稀有度共享叠加上限。武器专属 Buff 需持有对应武器。

基础额外子弹：手枪/弓箭增加同时发射的弹丸，霰弹增加完整连续齐射波次，炮台增加单位；不作用于炸弹发射器、重斧或自律武器；固定附伤不被暴击/倍率再次放大，持续及衍生伤害不重复触发固定附伤。

| 稀有度 | Buff 版本数（不含获取卡） |
|---|---:|
| 普通 Common | 20 |
| 稀有 Rare | 73 |
| 史诗 Epic | 59 |
| 传说 Legendary | 44 |
| 神话 Myth | 9 |

## 一、自律武器 → 稀有度 → 专属 Buff

### 旋风守卫

#### 稀有 Rare

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Blood Harvest（旋风守卫） | 3 | Whirlwind kills heal 1 HP per level, at most 5 HP per pulse cycle. |
| Long Reach（旋风守卫） | 3 | Increase attack radius by 15% of base per level. |
| Repulse（旋风守卫） | 3 | Add 0.25 units to the whirlwind's innate knockback per level. |
| Whirlwind Force（旋风守卫） | 3 | Automatic whirlwind damage +20% of base per level. |
| Whirlwind Rhythm（旋风守卫） | 3 | Automatic whirlwind interval -10% of base per level. |

#### 史诗 Epic

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Echo Pulse（旋风守卫） | 2 | Add 1 automatic pulse per cycle per level; pulses are separated by 0.12s. |

### 浮游炮

#### 稀有 Rare

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Drone Armaments（浮游炮） | 3 | Floating cannon damage +20% of base per level; other weapons are unaffected. |
| Focus Protocol（浮游炮） | 1 | Drones share the nearest target around the player. Replaces Independent Protocol. |
| Independent Protocol（浮游炮） | 1 | Prefer targets not reserved by another drone. Replaces Focus Protocol. |
| Wide Orbit（浮游炮） | 3 | Add 0.5 units to orbit radius per level. |

#### 史诗 Epic

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Burst Mode（浮游炮） | 1 | Fire 3 shots 0.12s apart; each deals 60% damage. |
| Cannon Wing（浮游炮） | 2 | Add 1 permanent drone per level; does not increase projectiles per drone. |
| Overload（浮游炮） | 1 | Every 10s, gain +25% damage and 40% shorter attack cooldown for 3s. |
| Split Cannons（浮游炮） | 2 | Add 1 projectile per drone volley per level; does not add drones. |

### 灵体召唤

#### 稀有 Rare

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Long Claws（灵体召唤） | 3 | Add 0.2 units to melee reach per level. |
| Rending Claws（灵体召唤） | 3 | Hits apply a 3s bleed: 20% weapon damage per level each second. Up to 3 stacks per enemy. |
| Spirit Strength（灵体召唤） | 3 | Spirit damage +20% of base per level, including bleed and temporary spirits. |
| Swift Spirits（灵体召唤） | 3 | Increase movement speed by 20% of base per level. |

#### 史诗 Epic

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Pounce（灵体召唤） | 1 | Every 4s, dash toward targets within 5 units; a hit during the dash gains +50% damage. |
| Spirit Echo（灵体召唤） | 1 | Kills summon a temporary spirit for 8s. At most 2 extra spirits; they cannot reproduce. |
| Spirit Pack（灵体召唤） | 2 | Add 1 permanent spirit per level. |

### 环形火球

#### 稀有 Rare

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Ember Cycle（环形火球） | 3 | Fireball cooldown -10% of base per level. |
| Hotter Flames（环形火球） | 3 | Fireball damage +20% of base per level. |
| Spreading Flame（环形火球） | 3 | Explosion radius +15% of base per level. |

#### 史诗 Epic

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Flame Ring（环形火球） | 3 | Add 1 radial fireball per cast per level. |

### 精准闪电

#### 稀有 Rare

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Gathering Storm（精准闪电） | 3 | Lightning cooldown -10% of base per level. |
| High Voltage（精准闪电） | 3 | Lightning damage +20% of base per level. |

#### 史诗 Epic

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Storm Front（精准闪电） | 3 | Strike 1 additional distinct visible enemy per cast per level. |
| Thunder Echo（精准闪电） | 1 | Strike the same targets again after 0.3s for 50% damage. Dead targets are skipped. |

## 二、全部 Buff → 类型 → 稀有度

### 攻击

#### 普通 Common

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Blast Radius | 5 | Explosion radius bonus +15%. |
| Bomb Tech | 5 | Bomb cooldown -10% (multiplicative). |
| Deadly Aim | 5 | Critical damage multiplier +0.25x. |
| Multishot | 3 | Primary weapon quantity +1; pistol/bow: extra simultaneous projectiles; shotgun: extra complete consecutive volleys; turret: extra units. Excludes auxiliaries, axe and RMB bombs. |
| Piercing Rounds | 3 | Additional targets pierced +1. |
| Precision | 5 | Critical chance +5% (100% cap). |
| Primary Weapon Attack Speed | 5 | Primary weapon attack speed +8%; cooldown is divided by attack-speed multiplier. |
| Primary Weapon Damage | 10 | Primary weapon damage +10%; excludes auxiliary weapons and RMB bombs. |

#### 稀有 Rare

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Blast Radius | 5 | Explosion radius bonus +22.5%. |
| Bomb Tech | 5 | Bomb cooldown -15% (multiplicative). |
| Deadly Aim | 5 | Critical damage multiplier +0.375x. |
| Multishot | 3 | Primary weapon quantity +2; pistol/bow: extra simultaneous projectiles; shotgun: extra complete consecutive volleys; turret: extra units. Excludes auxiliaries, axe and RMB bombs. |
| Piercing Rounds | 3 | Additional targets pierced +2. |
| Precision | 5 | Critical chance +7.5% (100% cap). |
| Primary Weapon Attack Speed | 5 | Primary weapon attack speed +12%; cooldown is divided by attack-speed multiplier. |
| Primary Weapon Damage | 10 | Primary weapon damage +15%; excludes auxiliary weapons and RMB bombs. |

#### 史诗 Epic

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Blast Radius | 5 | Explosion radius bonus +30%. |
| Bomb Tech | 5 | Bomb cooldown -20% (multiplicative). |
| Deadly Aim | 5 | Critical damage multiplier +0.5x. |
| Multishot | 3 | Primary weapon quantity +3; pistol/bow: extra simultaneous projectiles; shotgun: extra complete consecutive volleys; turret: extra units. Excludes auxiliaries, axe and RMB bombs. |
| Piercing Rounds | 3 | Additional targets pierced +3. |
| Precision | 5 | Critical chance +10% (100% cap). |
| Primary Weapon Attack Speed | 5 | Primary weapon attack speed +16%; cooldown is divided by attack-speed multiplier. |
| Primary Weapon Damage | 10 | Primary weapon damage +20%; excludes auxiliary weapons and RMB bombs. |

#### 传说 Legendary

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Blast Radius | 5 | Explosion radius bonus +45%. |
| Bomb Tech | 5 | Bomb cooldown -30% (multiplicative). |
| Deadly Aim | 5 | Critical damage multiplier +0.75x. |
| Multishot | 3 | Primary weapon quantity +4; pistol/bow: extra simultaneous projectiles; shotgun: extra complete consecutive volleys; turret: extra units. Excludes auxiliaries, axe and RMB bombs. |
| Piercing Rounds | 3 | Additional targets pierced +4. |
| Precision | 5 | Critical chance +15% (100% cap). |
| Primary Weapon Attack Speed | 5 | Primary weapon attack speed +24%; cooldown is divided by attack-speed multiplier. |
| Primary Weapon Damage | 10 | Primary weapon damage +30%; excludes auxiliary weapons and RMB bombs. |

### 生存

#### 普通 Common

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Armor Training | 5 | Damage reduction +8% (90% cap). |
| Regeneration | 5 | Restore 1 health every 5 seconds. |
| Second Wind | 5 | Post-hit invulnerability +0.2 seconds. |
| Vitality | 5 | Max health +20; heal the same amount. |

#### 稀有 Rare

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Armor Training | 5 | Damage reduction +12% (90% cap). |
| Regeneration | 5 | Restore 1.5 health every 5 seconds. |
| Second Wind | 5 | Post-hit invulnerability +0.3 seconds. |
| Vitality | 5 | Max health +30; heal the same amount. |

#### 史诗 Epic

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Armor Training | 5 | Damage reduction +16% (90% cap). |
| Regeneration | 5 | Restore 2 health every 5 seconds. |
| Second Wind | 5 | Post-hit invulnerability +0.4 seconds. |
| Vitality | 5 | Max health +40; heal the same amount. |

#### 传说 Legendary

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Armor Training | 5 | Damage reduction +24% (90% cap). |
| Regeneration | 5 | Restore 3 health every 5 seconds. |
| Second Wind | 5 | Post-hit invulnerability +0.6 seconds. |
| Vitality | 5 | Max health +60; heal the same amount. |

### 机动

#### 普通 Common

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Battle Boots | 5 | Movement speed bonus +4%. |
| Dash Practice | 5 | Dash cooldown -10% (multiplicative). |
| Long Dash | 5 | Dash distance bonus +7.5% via dash speed. |
| Magnet | 5 | Pickup radius bonus +20%. |

#### 稀有 Rare

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Battle Boots | 5 | Movement speed bonus +6%. |
| Dash Practice | 5 | Dash cooldown -15% (multiplicative). |
| Long Dash | 5 | Dash distance bonus +11.25% via dash speed. |
| Magnet | 5 | Pickup radius bonus +30%. |

#### 史诗 Epic

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Battle Boots | 5 | Movement speed bonus +8%. |
| Dash Practice | 5 | Dash cooldown -20% (multiplicative). |
| Long Dash | 5 | Dash distance bonus +15% via dash speed. |
| Magnet | 5 | Pickup radius bonus +40%. |

#### 传说 Legendary

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Battle Boots | 5 | Movement speed bonus +12%. |
| Dash Practice | 5 | Dash cooldown -30% (multiplicative). |
| Long Dash | 5 | Dash distance bonus +22.5% via dash speed. |
| Magnet | 5 | Pickup radius bonus +60%. |

### 成长

#### 普通 Common

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Potential | 5 | Future level-ups add +1% all damage bonus. |
| Renewal | 5 | Future level-ups restore +10% of max health. |
| Scholar | 5 | Experience gain bonus +15%. |
| Swift Pickups | 5 | Pickup attraction speed bonus +20%. |

#### 稀有 Rare

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Potential | 5 | Future level-ups add +1.5% all damage bonus. |
| Renewal | 5 | Future level-ups restore +15% of max health. |
| Scholar | 5 | Experience gain bonus +22.5%. |
| Swift Pickups | 5 | Pickup attraction speed bonus +30%. |

#### 史诗 Epic

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Potential | 5 | Future level-ups add +2% all damage bonus. |
| Renewal | 5 | Future level-ups restore +20% of max health. |
| Scholar | 5 | Experience gain bonus +30%. |
| Swift Pickups | 5 | Pickup attraction speed bonus +40%. |

#### 传说 Legendary

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Potential | 5 | Future level-ups add +3% all damage bonus. |
| Renewal | 5 | Future level-ups restore +30% of max health. |
| Scholar | 5 | Experience gain bonus +45%. |
| Swift Pickups | 5 | Pickup attraction speed bonus +60%. |

### 通用与复合

#### 稀有 Rare

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Boss Slayer | 3 | Damage to mini bosses and final bosses +20%. |
| Dash Logistics | 3 | Dash cooldown -8% (multiplicative).；Dash distance bonus +5% via dash speed. |
| Demolition Kit | 3 | Explosion radius bonus +15%.；Bomb cooldown -8% (multiplicative). |
| Elite Breaker | 3 | Damage to elites +20%; excludes bosses. |
| Field Scholar | 3 | Experience gain bonus +10%.；Pickup radius bonus +15%. |
| Fortune Bundle | 3 | 立即获得 2 个不同的史诗基础 Buff，消耗正常家族等级。 |
| Mobile Gunnery | 3 | Primary weapon damage +8%; excludes auxiliary weapons and RMB bombs.；Movement speed bonus +3%. |
| Monster Hunter | 5 | Damage to normal enemies +15%. |
| Renewing Potential | 3 | Future level-ups restore +5% of max health.；Future level-ups add +0.5% all damage bonus. |
| Salvage Engine | 3 | Pickup attraction speed bonus +20%.；Pickup radius bonus +15%. |
| Survivor Instinct | 3 | Damage reduction +4% (90% cap).；Post-hit invulnerability +0.15 seconds. |
| Universal Haste | 5 | All weapon and dash cooldowns -10%. |
| Universal Power | 5 | All damage bonus +15%. |
| Vanguard Provisions | 3 | Max health +20; heal the same amount.；Restore 0.5 health every 5 seconds. |

#### 史诗 Epic

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Arcane Impact | 3 | Direct hits add 3 fixed damage after multipliers; excludes damage-over-time and secondary procs. |
| Arcane Overdrive | 2 | All weapon and dash cooldowns -6%.；All damage bonus +10%. |
| Battle Medic | 2 | Max health +30; heal the same amount.；Restore 1 health every 5 seconds.；Future level-ups restore +5% of max health. |
| Boss Slayer | 3 | Damage to mini bosses and final bosses +30%. |
| Critical Engine | 2 | Critical chance +5% (100% cap).；Critical damage multiplier +0.3x. |
| Dash Frenzy | 1 | Movement speed bonus +8%.；After dashing: +30% primary weapon attack speed for 3s; adds to other attack-speed bonuses. |
| Dash Logistics | 3 | Dash cooldown -12% (multiplicative).；Dash distance bonus +7.5% via dash speed. |
| Demolition Kit | 3 | Explosion radius bonus +22.5%.；Bomb cooldown -12% (multiplicative). |
| Desperate Precision | 1 | Gain up to +50% critical chance, proportional to missing health. |
| Elite Breaker | 3 | Damage to elites +30%; excludes bosses. |
| Empowered Rounds | 3 | Bullet and arrow hits add 4 fixed damage after multipliers; includes turret and drone bullets, excludes secondary procs. |
| Field Scholar | 3 | Experience gain bonus +15%.；Pickup radius bonus +22.5%. |
| Fortune Bundle | 3 | 立即获得 2 个不同的传说基础 Buff，消耗正常家族等级。 |
| Growth Core | 2 | Experience gain bonus +15%.；Future level-ups add +0.75% all damage bonus. |
| Mobile Gunnery | 3 | Primary weapon damage +12%; excludes auxiliary weapons and RMB bombs.；Movement speed bonus +4.5%. |
| Monster Hunter | 5 | Damage to normal enemies +22.5%. |
| Piercing Blast | 1 | Additional targets pierced +1.；Final projectile hit explodes in radius 2 for 100% bullet damage. |
| Renewing Potential | 3 | Future level-ups restore +7.5% of max health.；Future level-ups add +0.75% all damage bonus. |
| Salvage Engine | 3 | Pickup attraction speed bonus +30%.；Pickup radius bonus +22.5%. |
| Survivor Instinct | 3 | Damage reduction +6% (90% cap).；Post-hit invulnerability +0.225 seconds. |
| Universal Haste | 5 | All weapon and dash cooldowns -15%. |
| Universal Power | 5 | All damage bonus +22.5%. |
| Vanguard Provisions | 3 | Max health +30; heal the same amount.；Restore 0.75 health every 5 seconds. |

#### 传说 Legendary

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Arcane Impact | 3 | Direct hits add 4.5 fixed damage after multipliers; excludes damage-over-time and secondary procs. |
| Arcane Overdrive | 2 | All weapon and dash cooldowns -9%.；All damage bonus +15%. |
| Battle Medic | 2 | Max health +45; heal the same amount.；Restore 1.5 health every 5 seconds.；Future level-ups restore +7.5% of max health. |
| Boss Slayer | 3 | Damage to mini bosses and final bosses +40%. |
| Critical Engine | 2 | Critical chance +7.5% (100% cap).；Critical damage multiplier +0.45x. |
| Dash Frenzy | 1 | Movement speed bonus +12%.；After dashing: +45% primary weapon attack speed for 3s; adds to other attack-speed bonuses. |
| Dash Logistics | 3 | Dash cooldown -16% (multiplicative).；Dash distance bonus +10% via dash speed. |
| Demolition Kit | 3 | Explosion radius bonus +30%.；Bomb cooldown -16% (multiplicative). |
| Desperate Precision | 1 | Gain up to +75% critical chance, proportional to missing health. |
| Elite Breaker | 3 | Damage to elites +40%; excludes bosses. |
| Empowered Rounds | 3 | Bullet and arrow hits add 6 fixed damage after multipliers; includes turret and drone bullets, excludes secondary procs. |
| Field Scholar | 3 | Experience gain bonus +20%.；Pickup radius bonus +30%. |
| Final Verdict | 2 | Damage to enemies at or below 30% HP +35%; checked before the hit. |
| Giant Bane | 1 | Damage to elites +15%; excludes bosses.；Damage to mini bosses and final bosses +15%.；Direct hits add 5 fixed damage after multipliers; excludes damage-over-time and secondary procs. |
| Growth Core | 2 | Experience gain bonus +22.5%.；Future level-ups add +1.125% all damage bonus. |
| Mobile Gunnery | 3 | Primary weapon damage +16%; excludes auxiliary weapons and RMB bombs.；Movement speed bonus +6%. |
| Monster Hunter | 5 | Damage to normal enemies +30%. |
| Piercing Blast | 1 | Additional targets pierced +2.；Final projectile hit explodes in radius 2 for 150% bullet damage. |
| Renewing Potential | 3 | Future level-ups restore +10% of max health.；Future level-ups add +1% all damage bonus. |
| Salvage Engine | 3 | Pickup attraction speed bonus +40%.；Pickup radius bonus +30%. |
| Survivor Instinct | 3 | Damage reduction +8% (90% cap).；Post-hit invulnerability +0.3 seconds. |
| Universal Haste | 5 | All weapon and dash cooldowns -20%. |
| Universal Power | 5 | All damage bonus +30%. |
| Vanguard Provisions | 3 | Max health +40; heal the same amount.；Restore 1 health every 5 seconds. |

### 武器专属

#### 稀有 Rare

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Crushing Blow（战斧） | 3 | Axe knockback +0.2 units per level. |
| Heavy Edge（战斧） | 3 | Axe damage +20% of base per level. |
| Long Haft（战斧） | 3 | Axe forward reach and side reach +15% of base per level. |
| Ready Stance（战斧） | 3 | Windup -15% of base per level; base attack interval stays unchanged. |
| Wide Cleave（战斧） | 2 | Axe arc +10 degrees per level, capped at 170 degrees. |
| Demolition（炸弹发射器） | 3 | Gain +40% damage per level in the inner 35% of the blast radius. |
| Scorched Ground（炸弹发射器） | 3 | Leave fire for 3s, dealing 10% weapon damage per level every 0.5s. |
| Heavy Draw（穿透弓） | 2 | Each shot requires 0.35s of drawing and gains +60% weapon damage per level. Release early to cancel. |
| Long Arrow（穿透弓） | 3 | Add 1 innate pierce per level. |
| Momentum（穿透弓） | 3 | Each previous target pierced adds 15% weapon damage per level. |
| Long Claws（灵体召唤） | 3 | Add 0.2 units to melee reach per level. |
| Rending Claws（灵体召唤） | 3 | Hits apply a 3s bleed: 20% weapon damage per level each second. Up to 3 stacks per enemy. |
| Spirit Strength（灵体召唤） | 3 | Spirit damage +20% of base per level, including bleed and temporary spirits. |
| Swift Spirits（灵体召唤） | 3 | Increase movement speed by 20% of base per level. |
| Drone Armaments（浮游炮） | 3 | Floating cannon damage +20% of base per level; other weapons are unaffected. |
| Focus Protocol（浮游炮） | 1 | Drones share the nearest target around the player. Replaces Independent Protocol. |
| Independent Protocol（浮游炮） | 1 | Prefer targets not reserved by another drone. Replaces Focus Protocol. |
| Wide Orbit（浮游炮） | 3 | Add 0.5 units to orbit radius per level. |
| Ember Cycle（环形火球） | 3 | Fireball cooldown -10% of base per level. |
| Hotter Flames（环形火球） | 3 | Fireball damage +20% of base per level. |
| Spreading Flame（环形火球） | 3 | Explosion radius +15% of base per level. |
| Gathering Storm（精准闪电） | 3 | Lightning cooldown -10% of base per level. |
| High Voltage（精准闪电） | 3 | Lightning damage +20% of base per level. |
| Blood Harvest（旋风守卫） | 3 | Whirlwind kills heal 1 HP per level, at most 5 HP per pulse cycle. |
| Long Reach（旋风守卫） | 3 | Increase attack radius by 15% of base per level. |
| Repulse（旋风守卫） | 3 | Add 0.25 units to the whirlwind's innate knockback per level. |
| Whirlwind Force（旋风守卫） | 3 | Automatic whirlwind damage +20% of base per level. |
| Whirlwind Rhythm（旋风守卫） | 3 | Automatic whirlwind interval -10% of base per level. |
| Follow-up（手枪） | 2 | Kills fire 1 extra shot at a nearby enemy per level. Extra shots cannot trigger follow-ups. |
| Rhythm（手枪） | 3 | Every 5th hit gains +50% weapon damage per level. |
| Warm Barrel（手枪） | 3 | Each consecutive shot reduces cooldown by 4% per level, capped at 40%. Resets after 1s idle. |
| Choke（霰弹枪） | 3 | Reduce total spread by 15% of base per level; minimum 10 degrees. |
| Concentrated Fire（霰弹枪） | 3 | The 3rd pellet hitting one enemy in a volley gains +100% weapon damage per level. |
| Extra Volleys（霰弹枪） | 3 | Add 1 consecutive shotgun volley per attack per level; pellet count per volley is unchanged. |
| Point Blank（霰弹枪） | 3 | Gain +20% weapon damage per level within 3 units of the muzzle. |
| Durable Sentry（炮塔） | 3 | Add 10 seconds to turret lifetime per level, including deployed turrets. |
| Linked Sentries（炮塔） | 3 | Each other turret within 4 units adds 10% shot damage per level, up to 3 neighbors. |
| Quick Assembly（炮塔） | 3 | Reduce deployment cooldown by 15% of base per level, up to 60%. |
| Remote Placement（炮塔） | 3 | Add 1 unit to deployment distance per level, within the camera view. |

#### 史诗 Epic

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Executioner（战斧） | 2 | Gain +35% axe damage per level against enemies at or below 30% HP. |
| Aftershock（炸弹发射器） | 2 | Repeat the explosion after 0.6s for 40% damage per level. |
| Cluster Bomb（炸弹发射器） | 2 | After explosion, create 3 small blasts per level, each dealing 20% damage in radius 1. |
| Returning Arrow（穿透弓） | 1 | Arrows return after 0.45s or exhausting pierce. May hit each enemy once again on return. |
| Pounce（灵体召唤） | 1 | Every 4s, dash toward targets within 5 units; a hit during the dash gains +50% damage. |
| Spirit Echo（灵体召唤） | 1 | Kills summon a temporary spirit for 8s. At most 2 extra spirits; they cannot reproduce. |
| Spirit Pack（灵体召唤） | 2 | Add 1 permanent spirit per level. |
| Burst Mode（浮游炮） | 1 | Fire 3 shots 0.12s apart; each deals 60% damage. |
| Cannon Wing（浮游炮） | 2 | Add 1 permanent drone per level; does not increase projectiles per drone. |
| Overload（浮游炮） | 1 | Every 10s, gain +25% damage and 40% shorter attack cooldown for 3s. |
| Split Cannons（浮游炮） | 2 | Add 1 projectile per drone volley per level; does not add drones. |
| Flame Ring（环形火球） | 3 | Add 1 radial fireball per cast per level. |
| Storm Front（精准闪电） | 3 | Strike 1 additional distinct visible enemy per cast per level. |
| Thunder Echo（精准闪电） | 1 | Strike the same targets again after 0.3s for 50% damage. Dead targets are skipped. |
| Echo Pulse（旋风守卫） | 2 | Add 1 automatic pulse per cycle per level; pulses are separated by 0.12s. |
| Last Stand（炮塔） | 2 | Natural expiry explodes in radius 2 for 150% weapon damage per level. Replacement does not trigger it. |

#### 神话 Myth

| 词条 | 等级上限 | 每次获取效果 |
|---|---:|---|
| Earthshatter（战斧） | 2 | PRIMARY ONLY: The first hit per swing splashes 40% hit damage within 1.8 units. Up to 8 other targets; no secondary hit procs. |
| Execution Burst（战斧） | 2 | PRIMARY ONLY: The first direct kill per swing explodes for 60% hit damage within 2 units. Up to 8 other targets; explosions cannot chain. |
| Implosion Core（炸弹发射器） | 2 | PRIMARY ONLY: Before exploding, pull up to 16 enemies within 1.5x blast radius inward by up to 1.5 units. Elites move half as far; bosses are immune. |
| Siege Ring（炸弹发射器） | 2 | PRIMARY ONLY: Add an outer blast from 1x to 1.7x explosion radius for 35% damage. Up to 8 enemies outside the main blast; no secondary hit procs. |
| Penetrating Nova（穿透弓） | 2 | PRIMARY ONLY: Every second enemy pierced splashes 40% arrow damage within 1.6 units. Up to 8 other targets; no secondary hit procs. |
| Ricochet Rounds（手枪） | 2 | PRIMARY ONLY: Start one chain per volley. Bounce to +1 distinct enemies within 3 units per level, up to 4; each bounce retains 60% damage. No secondary hit procs. |
| Concussive Volley（霰弹枪） | 2 | PRIMARY ONLY: The first hit per volley splashes 20% of (hit damage x pellets per volley) within 1.5 units. Up to 8 other targets; no secondary hit procs. |
| Crowd Piercer（霰弹枪） | 2 | PRIMARY ONLY: Each pellet gains +1 innate pierce. Does not change pellets per volley. |
| Explosive Fire Control（炮塔） | 2 | PRIMARY ONLY: Bullets splash 35% of hit damage to other enemies within 1.5 units. Up to 8 extra targets; no secondary hit procs. |

## 三、自律武器获取卡（均为稀有）

- 旋风守卫 Whirlwind Guard
- 浮游炮 Floating Cannons
- 灵体召唤 Spirit Pack
- 环形火球 Radial Fireballs
- 精准闪电 Precision Lightning

游戏内 Start Menu 的图鉴使用运行时同源工厂直接读取角色、武器和 Buff，数据随配置更新。本文件为生成时快照；更新后运行 `python Tests/export_buff_catalog.py` 刷新。
