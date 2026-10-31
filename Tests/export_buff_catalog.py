"""Export all currently authored buff variants without launching Unity.

Reads the same source declarations and scaling rules used by the gameplay factory.
Unity's GameCodexCatalog.ExportMarkdown is also available to editor callers.
"""
import collections
import json
import pathlib
import re

ROOT = pathlib.Path(__file__).resolve().parent.parent
SCRIPTS = ROOT / "Unity6-project1/Assets/Scripts/GameScene"
manager = (SCRIPTS / "UpgradeManager.cs").read_text(encoding="utf-8-sig")
weapons = (SCRIPTS / "WeaponUpgradeCatalog.cs").read_text(encoding="utf-8-sig")
RARITIES = ["Common", "Rare", "Epic", "Legendary"]
RARITY_CN = ["普通", "稀有", "史诗", "传说"]
TYPES = {"Attack": "攻击", "Survival": "生存", "Mobility": "机动", "Growth": "成长", "Universal": "通用与复合", "Weapon": "武器专属"}
WEAPON_CN = dict(Pistol="手枪", Shotgun="霰弹枪", Bow="穿透弓", Melee="旋风守卫", Axe="战斧", Bomb="炸弹发射器", Turret="炮塔", Drone="浮游炮", Creature="灵体召唤", FireOrb="环形火球", Lightning="精准闪电")
NUMBER = r"[.\d]+f?"
rows = []


def number(value):
    return float(value.removesuffix("f"))


def fmt(value):
    return f"{value:.6f}".rstrip("0").rstrip(".")


descriptions = dict(re.findall(r"^\s*case UpgradeEffectType\.(\w+): return (.*);$", manager, re.M))


def describe(effect, value, rarity):
    if effect == "RandomHigherRarityBuffs":
        return f"立即获得 {fmt(value)} 个不同的{RARITY_CN[rarity + 1]}基础 Buff，消耗正常家族等级。"
    if effect not in descriptions:
        raise ValueError(f"Missing effect description: {effect}")
    tokens = re.findall(r'"(?:[^"\\]|\\.)*"|\bvalue\b|\bpercent\b', descriptions[effect])
    return "".join(fmt(value) if token == "value" else fmt(value * 100) + "%" if token == "percent" else json.loads(token) for token in tokens)


def add_family(identifier, title, category, cap, effects, minimum, basic=False):
    for rarity in range(minimum, 4):
        strength = [1, 1.5, 2, 3][rarity] if basic else 1 + 0.5 * (rarity - minimum)
        scaled = [(effect, value + rarity - minimum if effect in ("ProjectileCount", "PierceCount") else value * strength) for effect, value in effects]
        rows.append(dict(id=identifier, title=title, type=category, rarity=rarity, cap=int(cap),
                         description="；".join(describe(effect, value, rarity) for effect, value in scaled), weapon=None))


basic_pattern = rf'Basic\("([^"]+)", "([^"]+)", UpgradeCategory\.(\w+), UpgradeEffectType\.(\w+), ({NUMBER})(?:, (\d+))?\)'
for identifier, title, category, effect, value, cap in re.findall(basic_pattern, manager):
    add_family(identifier, title, category, cap or 5, [(effect, number(value))], 0, basic=True)

special_pattern = r'AddSpecial\(pool, "([^"]+)", "([^"]+)", UpgradeRarity\.(\w+), (\d+),(.*?)\);'
for identifier, title, minimum, cap, body in re.findall(special_pattern, manager, re.S):
    effects = [(effect, number(value)) for effect, value in re.findall(rf'new UpgradeEffect\(UpgradeEffectType\.(\w+), ({NUMBER})\)', body)]
    if not effects:
        raise ValueError(f"Empty family {identifier}")
    add_family(identifier, title, "Universal", cap, effects, RARITIES.index(minimum))

for rarity in (1, 2):
    rows.append(dict(id="fortune_bundle", title="Fortune Bundle", type="Universal", rarity=rarity, cap=3,
                     description=describe("RandomHigherRarityBuffs", 2, rarity), weapon=None))

weapon_pattern = rf'Add\(result, WeaponKind\.(\w+), WeaponBuffEffect\.(\w+), ("(?:[^"\\]|\\.)*"), ("(?:[^"\\]|\\.)*"), ({NUMBER}), (\d+)(?:, UpgradeRarity\.(\w+))?\);'
for weapon, effect, title, description, value, cap, rarity in re.findall(weapon_pattern, weapons):
    rows.append(dict(id=f"weapon_{weapon}_{effect}", title=json.loads(title), type="Weapon", rarity=RARITIES.index(rarity or "Rare"), cap=int(cap),
                     description=json.loads(description), weapon=weapon))

if len(rows) != 196 or len({(row["id"], row["rarity"]) for row in rows}) != 196:
    raise ValueError(f"Catalog coverage/uniqueness changed: parsed {len(rows)} buff versions; update export checks with the production catalog")


def append_table(output, selection):
    output += ["| 词条 | 等级上限 | 每次获取效果 |", "|---|---:|---|"]
    for row in sorted(selection, key=lambda item: (item["weapon"] or "", item["title"])):
        name = row["title"] + (f"（{WEAPON_CN[row['weapon']]}）" if row["weapon"] else "")
        desc = row["description"].replace("\n", "；").replace("|", "\\|")
        output.append(f"| {name} | {row['cap']} | {desc} |")
    output.append("")


output = ["# 当前全部 Buff 清单", "", "从当前 UpgradeManager 与 WeaponUpgradeCatalog 源码生成。", "",
          "共 **196 个 Buff 稀有度版本**；另有 **5 张稀有自律武器获取卡**，总奖励池 201 张。", "",
          "数值为每次获取效果；同名家族不同稀有度共享叠加上限。武器专属 Buff 需持有对应武器。", "",
          "基础额外子弹：手枪/弓箭增加同时发射的弹丸，霰弹增加完整连续齐射波次，炮台增加单位；不作用于炸弹发射器、重斧或自律武器；固定附伤不被暴击/倍率再次放大，持续及衍生伤害不重复触发固定附伤。", ""]
counts = collections.Counter(row["rarity"] for row in rows)
output += ["| 稀有度 | Buff 版本数（不含获取卡） |", "|---|---:|"]
output += [f"| {RARITY_CN[tier]} {RARITIES[tier]} | {counts[tier]} |" for tier in range(4)]
output += ["", "## 一、自律武器 → 稀有度 → 专属 Buff", ""]
for weapon in ("Melee", "Drone", "Creature", "FireOrb", "Lightning"):
    output += ["### " + WEAPON_CN[weapon], ""]
    for rarity in range(4):
        selected = [row for row in rows if row["weapon"] == weapon and row["rarity"] == rarity]
        if selected:
            output += [f"#### {RARITY_CN[rarity]} {RARITIES[rarity]}", ""]
            append_table(output, selected)
output += ["## 二、全部 Buff → 类型 → 稀有度", ""]
for category, label in TYPES.items():
    output += ["### " + label, ""]
    for rarity in range(4):
        selected = [row for row in rows if row["type"] == category and row["rarity"] == rarity]
        if selected:
            output += [f"#### {RARITY_CN[rarity]} {RARITIES[rarity]}", ""]
            append_table(output, selected)
output += ["## 三、自律武器获取卡（均为稀有）", "", "- 旋风守卫 Whirlwind Guard", "- 浮游炮 Floating Cannons", "- 灵体召唤 Spirit Pack", "- 环形火球 Radial Fireballs", "- 精准闪电 Precision Lightning", "",
           "游戏内 Start Menu 的图鉴使用运行时同源工厂直接读取角色、武器和 Buff，数据随配置更新。本文件为生成时快照；更新后运行 `python Tests/export_buff_catalog.py` 刷新。", ""]
destination = ROOT / "BUFF_CATALOG.md"
destination.write_text("\n".join(output), encoding="utf-8")
print(f"Exported {len(rows)} buffs / {len({row['id'] for row in rows})} families to {destination}")
