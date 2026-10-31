# 第五阶段：地图奖励

GameScene 已放置 `Assets/ZoneRewardDirector.prefab` 的实例。选中 Project 中的这个 Prefab，即可在 Inspector 修改生成时间，场景实例会继承配置。其他场景若未放置此组件，GameManager 会自动创建默认的 ZoneRewardDirector。

每批生成五个区域，分布在玩家周围、当前视野外。第二批需要先解封。具体时间以 `ZoneRewardDirector.prefab` 中的 First Wave Time / Sealed Wave Time 为准（秒）；无 Prefab 的运行时后备值为 3:00 和 12:00。时间使用游戏时间，升级选择、暂停和结算时停止。

| 区域 | 累计时间 | 奖励 |
| --- | --- | --- |
| 复苏祭坛 | 10 秒 | 随机生成三个升级选项，暂停后选择一个 |
| 治疗泉 | 5 秒 | 回复最大生命的 50% |
| 武器残骸 | 8 秒 | 当前主武器等级 +1，每级增加基础武器伤害的 15% |
| 危险宝箱 | 20 秒 | 首次进入生成 8 个守卫；完成后提供稀有以上升级二选一 |
| 经验磁场 | 6 秒 | 立即收取当前摄像机画面内的全部经验，保留经验倍率 |

默认均为一次性。区域使用玩家到中心的世界距离判断进入，避免多个玩家碰撞体导致误退出。

离开后最多再累计 `exitGraceTime = 0.5` 秒，之后暂停且保留已有进度。返回后继续累计，并重新获得下一次离开的宽限。完成时立即失效，防止重复发奖。`oneShot = false` 时完成后必须先离开才能开始下一轮；危险宝箱每一轮只生成一次守卫。

第二批区域的外部粉色水晶封印有两条替代解法：累计造成 150 伤害，或依次在三个黄色符文中各停留 1.5 秒。踩符文中途离开会重置当前符文的停留计时，已激活的符文保留。枪弹、炸弹、范围攻击和斧击可以伤害封印。封印不是 Enemy，不会进入闪电、召唤物等自动锁敌搜索；攻击的实际碰撞或范围仍可伤害它。封印解除前区域不计时、不刷守卫、不发奖。

地图事件界面暂时统一使用英文，不引入多语言系统。每批生成时只播放一次中性提示“Altars have appeared in the distance...”。未探索时，边缘箭头统一为白色“Unknown Altar”，不显示具体类型、类型颜色或累计所需秒数。玩家距离区域中心不超过 `revealDistance`（默认 10）时排队播放“Discovered: 具体名称”和氛围提示；只有提示实际开始显示后，箭头和区域标签才揭示类型，且此后永久保留。若播放前已离开探索范围，会取消此次提示，等下次靠近再触发。完成和解封提示分别使用“Reward Claimed”和“Seal Broken”，在未揭示时也只使用“Unknown Altar”。

中央半透明提示的中心位于距屏幕顶部 38% 高度处。每个区域独立随机抽取 10–25 秒，按固定参考移速 4.5 计算到达区域边界的距离，即标准比例下 45–112.5 单位；距离随摄像机的角色移动比例同步缩放。实际角色移速加成和冲刺不再把祭坛推得更远；不再叠加旧的最小距离或视野外距离限制。区域半径按配置乘以 0.6，默认从 2.5 缩至 1.5，生成后位置固定。

边缘箭头使用玩家到目标的方向与摄像机安全边缘求交，支持不同宽高比和摄像机视口。箭头沿所在边缘避让，文字单独在画面内排布；文字避让不会把箭头从底部移到顶部。箭头旋转在界面缩放前计算，避免旋转中心发生偏移。

## 配置与预览

- 在非运行模式选中 `Assets/ZoneRewardDirector.prefab`，可以在 Inspector 调整 `firstWaveTime`（第一批，秒）、`sealedWaveTime`（第二批，秒）、`announcementDuration` 和区域列表。直接修改场景实例时需保存场景；若要同步回 Prefab，使用 Overrides > Apply All。
- `revealDistance` 控制靠近揭示距离，`areaRadiusMultiplier` 控制区域范围比例，`announcementVerticalPosition` 控制提示中心距顶部的高度比例（默认 0.38）。
- `Reference Move Speed` 为固定参考移速，默认 4.5；`Min Travel Time` / `Max Travel Time` 为每个区域独立抽取的赶路秒数区间，默认 10 / 25。
- `Assets > Create > Survival > Zone Reward` 创建配置，放入 `Resources/Zones` 或直接赋给 Director。无配置时自动使用五种默认值。
- ZoneRewardArea 暴露 `requiredStayTime`、`exitGraceTime`、`rewardType`、`oneShot`、`spawnEnemiesWhileInside`、`radius` 和 `enemyWaveSize`。
- Play Mode 中使用 `Tools > Zones > Spawn First Wave Now` / `Spawn Sealed Wave Now`，立即预览两批区域，无需等到指定时间。预览不会消耗自动定时生成次数。
- `Tools > Zones > Run Integration Smoke Tests` 会打开 GameScene、运行测试并退出 Play Mode；请先保存正在编辑的场景。

奖励全部经 RewardManager 发放。祭坛和宝箱复用 UpgradeManager 的排队、暂停与等级上限规则；升级池耗尽时改为回复 50% 最大生命，包括排队期间才耗尽的情况。经验磁场与普通拾取共用防重复收取入口。武器升级只改变武器实例，不修改共享 ScriptableObject。

## 验证

运行 `pwsh -NoProfile -File Tests/Verify-ZoneRewards.ps1`：编译全部运行时脚本及编辑器测试，并执行独立的区域计时边界测试和已有敌人规则回归测试。

Unity 集成测试入口为 `ZoneRewardSmokeTests.RunBatch`，覆盖场景自动初始化、两批生成、五种奖励、防重复、守卫波、暂停、伤害解封、踩点解封、经验收取和奖励池耗尽回退。已有编辑器占用工程时，请从编辑器菜单运行。
