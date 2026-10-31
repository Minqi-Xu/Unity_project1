# 敌人与精英系统（首版）

现有 GameScene 自动启用，无需重新绑定预制体。普通怪沿用 `Assets/Enemy.prefab`，通过 ScriptableObject 改变属性、体型与颜色；当前仍使用占位美术。

## 普通怪

配置文件位于 `Unity6-project1/Assets/Resources/Enemies/`。以下倍率作用于原敌人预制体；生命先加上原有随时间增长的数值，再乘配置倍率。

| 类型 | 加入时间 | 生命 | 移速 | 体型 | 行为 |
|---|---|---|---|---|---|
| Chaser | 0:00 | ×1 | ×1 | ×1 | 持续追踪 |
| Runner | 1:00 | ×0.45 | ×1.65 | ×0.75 | 快速接近 |
| Tank | 2:00 | ×3 | ×0.55 | ×1.5 | 高血慢速 |
| Ranged | 5:00 | ×0.8 | ×0.8 | ×0.9 | 保持距离，3 秒一次定向射击 |

小怪加入时间和权重统一由 EnemySpawner 的 `Normal Enemy Schedule` 配置（`Unlock Time` 单位秒，`Weight` 为相对权重，0 表示禁用）。默认 Chaser 0 秒、Runner 60 秒、Tank 120 秒、Ranged 300 秒；高血慢速怪保留 2:00 加入。EnemyDefinition 上旧的解锁时间/权重字段不再决定刷新。已解锁普通怪按此表的权重混合刷新。可以在 EnemySpawner 的 `enemyTypes` 中指定自己的 EnemyDefinition；未指定时读取 Resources/Enemies，没有资产时使用运行时默认目录。可通过 Create > Roguelite > Enemy Definition 新建配置。

小怪刷新频率适度提高：初始间隔由 5 秒缩短为 **4 秒**，后期最短间隔由 1 秒缩短为 **0.8 秒**，对应频率均提高约 25%。首次刷新改为开局立即出现（`Initial Spawn Delay = 0`），每次刷新后间隔仍减少 0.1 秒。在 EnemySpawner Inspector 的 `Spawn Rate`（初始间隔，越小越快）、`Spawn Increase Rate`（每次缩短量）、`Minimum Spawn Interval`（最短间隔）可直接调整。

## 定时遭遇

| 时间 | 遭遇 | 词缀 |
|---|---|---|
| 2:00 | 精英 Runner | 高速 |
| 4:00 | 精英 Tank | 高血、加速光环 |
| 6:00 | 小 Boss Chaser | 周期冲刺 |
| 7:00 | 精英 Ranged | 死亡爆炸 |
| 9:00 | 小 Boss Tank | 高血、死亡爆炸 |
| 10:00 | 精英 Runner | 高速、加速光环、周期冲刺 |
| 12:00 | 最终 Boss Ranged | 高血、周期冲刺 |

EnemySpawner 的 `encounters` 可编辑时间、名称、类型、等级和词缀。每个 Boss 批次还可单独调整 `Boss Movement Multiplier`（叠加在怪物配置上的移速倍率）与 `Boss Relative Speed Limit`（相对初始角色平均移速的上限倍率，最高 1.2）；精英不使用这些 Boss 参数。每条遭遇每局只触发一次。全程使用游戏时间，暂停、选升级不会推进计时。默认在出场前 **2 秒**显示提示。

精英、小 Boss、最终 Boss 的额外生命倍率分别为 ×7、×16、×35，额外体型倍率分别为 ×1.8、×2.7、×3.5。小 Boss 和最终 Boss 会周期性攻击玩家当时的位置，地面圈预警 1.2 秒后结算，玩家可以移出范围躲避。最终 Boss 攻击间隔为 3.5 秒，小 Boss 为 5 秒。

Boss 移动已削弱：小 Boss 和最终 Boss 的普通移速额外乘 **0.6**，冲刺倍率由 3.5 改为 **2**。在 `Assets/Resources/Enemies/` 对应配置的 Inspector 中，`Boss Speed Multiplier` 与 `Boss Dash Speed Multiplier` 可直接调整，普通怪和精英不受这两个参数影响。

精英额外移速倍率为 **0.65**（降低 35%），在同一怪物配置的 `Elite Speed Multiplier` 调整。精英与首个小 Boss 的所有位移速度上限为**初始可选角色平均移速 ×1.05**。9 分钟的小 Boss 在当前移速基础上增加 **25%**，上限为平均移速 **×1.15**；最终 Boss 增加 **35%**，上限为平均移速 **×1.20**。上限涵盖高速词缀、加速乘区和周期冲刺，Boss 最高不能超过平均移速的 120%。当前 6 个初始角色平均移速约 4.783，三档上限分别约为 **5.023、5.501、5.740**；更慢的敌人保持原速度，不会被抬高到上限。参考值在开局按角色配置计算，排除尚未开放的角色，不追随局内移速 Buff。玩家与敌人最后应用相同相机缩放，保持此速度比例。

词缀效果：高速 ×1.5 移速；高血 ×1.8 生命；光环为附近普通怪提供 ×1.35 移速且不叠加；死亡爆炸延迟 0.9 秒；周期冲刺每 6 秒锁定方向，预警 0.7 秒后以 ×3.5 速度（Boss 为 ×2）冲刺 0.5 秒，实际速度仍受对应批次上限约束。远程弹丸预警 0.45 秒，发射后不追踪玩家，并使用扫掠碰撞避免高速穿过玩家。

普通怪保留原来的接触后消失规则；精英与 Boss 接触玩家不会消失，接触伤害间隔为 1 秒，继续遵守玩家减伤与无敌时间。

## 奖励与胜利

精英和 Boss 被击杀时必掉一个金色宝箱；重复命中死亡目标不会重复掉落。靠近宝箱自动拾取，通过 RewardManager 按掉落来源提供奖励：**精英宝箱连续 2 次 Rare 及以上 Buff 三选一；小 Boss 和最终 Boss 宝箱连续 3 次**，分别获得 2 个和 3 个 Buff。全程暂停游戏，界面分别显示 1/2、2/2 或 1/3、2/3、3/3 进度。每次选择后重新生成下一次候选，复用现有 Buff 选择界面。候选仍遵守等级上限、武器容量和持有武器限制，并避免同族 Buff 同时出现。只有一个合格候选时显示一个；某次候选全部耗尽时，该次改为回复最大生命的 50%（包括已排队的后续选择）。宝箱与升级奖励共用队列，保留各自的候选数量和标题。

最终 Boss 默认在 **12:00（720 秒）** 出现；击杀后**立即结算胜利**，不再检查存活时长，也不再等待拾取最终宝箱或完成奖励选择。已移除 GameManager 的 Target Game Time 和 EnemySpawner 的 Final Boss Required 开关，结算不会因为单纯经过指定时间而触发。最终 Boss 死亡仍生成掉落，但胜利后不会要求领取；局内精英与小 Boss 的宝箱奖励维持 2 次／3 次选择。

## 验证

主武器 AOE 扩展：危险宝箱区域保底当前主武器专属 Buff；第一个小 Boss 的宝箱首轮也保底专属，其余两轮保持稀有奖励。其他敌人宝箱首轮默认有 25% 概率切换为专属轮次，可在 GameScene 的 RewardManager 组件调整。EnemySpawner 每个批次的 Guarantee Primary Weapon Buff 可控制保底来源；首个小 Boss 默认开启。详细词条和满级回退规则见 PRIMARY_AOE_BUFFS.md。

在仓库根目录运行 `./Tests/Verify-EnemySystem.ps1`：使用本机 Unity 引用编译全部运行时脚本和编辑器测试，并独立执行生产时间表的出场时间、预警边界、等级及词缀覆盖检查。需要 .NET 8+ 和已导入依赖的 Unity 项目；可用 `-UnityEditor` 指定编辑器目录。

Unity 菜单 `Tools > Enemies > Run Integration Smoke Tests (opens GameScene)` 会打开 GameScene 并进入 Play Mode，检查真实场景启动、属性倍率、精英接触存活、七次遭遇去重、死亡宝箱去重、精英宝箱两次与小 Boss 宝箱三次Rare+ 三选一、升级排队、候选耗尽后的治疗补偿和最终 Boss 击杀立即结算。运行前保存当前场景。也可用批处理入口 `-executeMethod EnemySmokeTests.RunBatch`（不要加 `-quit`，测试会自行退出）。

本次全量编译、独立时间表检查和精英/Boss 移速上限检查通过。Unity 批处理在加载项目阶段退出，未进入 Play Mode，因此集成烟测及完整一局战斗、UI 外观和数值平衡仍需编辑器实测。
