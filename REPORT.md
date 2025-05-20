# Ultimate Chicken Horse 源码架构分析

## 范围与限制

本报告基于 `GameSource/` 中的 2,564 个反编译 C# 文件。它是已安装游戏的托管程序集源码视图，而不是完整 Unity 工程：没有 `Assets/`、场景、预制体、ScriptableObject 资源、原始项目设置或 BrainCloud 服务端脚本。因此，代码可说明运行时控制流和数据边界，但不能还原所有序列化配置的实际值。

游戏自有逻辑主要位于 `GameSource/Assembly-CSharp/`；其余目录大多是 SDK 或第三方库的反编译源码。

## 总体结构

```text
MainMenuControl
  -> GameState / GameSettings / StatTracker
  -> Matchmaker (SteamMatchmaker -> GamesparksMatchmaker -> UnityMatchmaker)
  -> LobbyManager (UNet NetworkLobbyManager)
  -> TreeHouseLobby / LevelSelectController
  -> GameControl
       -> VersusControl | FreePlayControl | ChallengeControl
       -> QuickSaver | ScoreKeeper | Character | Placeable
```

- `GameState` 是跨场景的全局运行时状态，保存游戏模式、分数、选图、玩家保留状态与当前快照信息。
- `GameSettings` 是从 `Resources/GameWideSettings` 读取的规则容器，负责游戏模式、回合/时间限制、规则预设和修正器应用。
- `GameEventManager` 是进程内同步事件总线；它按事件具体类型维护监听器列表，供 UI、规则、计分、网络对象和存档系统协作。
- `SceneManagerWrapper` 负责温和卸载旧场景、释放资源并加载主菜单、树屋或关卡场景。

原生规则书并非只在房主本地生效：`TabletRulesScreen` 发送 `MsgApplyRuleset`，其中包含预设 XML 与规则/积分/方块/修正器的加载开关；其他客户端随后将同一 `GameRulePreset` 写入本地 `GameSettings` 和 `Modifiers`。

## 对局生命周期

1. 主菜单创建 `GameState`、`StatTracker`、`Matchmaker` 与后端管理器。
2. 创建或加入大厅后，`LobbyManager` 启动 UNet 主机或客户端，并进入 `TreeHouseLobby`。
3. `LevelSelectController` 在树屋中同步玩家、规则书、传送门、解锁和自定义地图槽位。
4. 房主选择关卡后生成 Match GUID，保存当前规则/地图快照信息，并切换到实际关卡场景。
5. `GameControl` 等待各客户端加载，再依次推进 `START -> PLAY -> PLACE -> SUDDENDEATH -> END`。
6. `VersusControl` 处理派对/创意模式的选块、放置、比赛、计分与下一轮；`ChallengeControl` 管理计时、重试和排行榜；`FreePlayControl` 提供自由循环。
7. 结束时返回树屋或主菜单，并保存统计和大厅状态。

## 角色、物理与规则

- `Character.FixedUpdate()` 对本地关联角色调用 `fullUpdate()`；非本地角色走 `clientUpdate()` 插值路径。`SmoothSync` 负责位置、旋转和速度的同步。
- `Modifiers` 是本地 `ScriptableObject` 单例，按 `ModsApplied` 和模式索引返回重力、跳跃、冲刺、墙跳、物品与其他规则参数。
- `Placeable` 是可放置物的基类，`ActiveBlock` 处理动态物件，`NetworkSurrogate` 用 SyncVar/Command 在网络中同步简单状态。
- `ScoreKeeper` 和 `VersusControl` 从角色成功、死亡、陷阱、硬币等事件计算积分、连胜、翻盘和突然死亡结果。

放置阶段由 `PiecePlacementCursor` 驱动。游标将方块 ID、位置、缩放、旋转和拾取状态包装为 `PiecePlaced`、`PiecePickedUp` 或 `BookPiecePicked` 消息；房主转发，客户端据此找到或创建对应 `Placeable`。动态方块附加 `NetworkSurrogate`，在所有客户端已加载快照后再分发关联消息，避免方块与网络对象的到达顺序发生错配。

## 联机与在线服务

### 实时游戏流量

`LobbyManager` 基于 Unity UNet HLAPI。房主运行实际的 `NetworkLobbyManager` 主机，客户端通过自定义消息、Command、ClientRpc、SyncVar 和 `SmoothSync` 加入同步。

`UnetRelayTransport` 在启用中继时把 UNet 数据封装并经地区中继转发。它会把房主或客户端连接到由 `UCHServices.Service` 按地区分配的中继端点；禁用中继时则使用大厅记录的公网/局域网端点。

### 平台、目录和后端

- `SteamMatchmaker` 负责 Steam 社交大厅、邀请、启动参数和 Steam 身份。
- 实际大厅目录、心跳、地图、排行榜、举报和内容元数据经 `BraincloudManager` / `BraincloudQuery` 调用 BrainCloud。
- 名称为 `GameSparksManager`、`GameSparksQuery`、`GamesparksMatchmaker` 的类是兼容抽象层；当前具体实现分别是 `BraincloudManager` 和 `BraincloudQuery`。
- 房主每 10 秒调用后端心跳；大厅数据有变更时按约 5 秒批量发送增量。客户端超时、版本不匹配、满员、游戏已开始或跨平台限制会中止加入。
- 大厅搜索会用 Moserware TrueSkill 的平均值和标准差计算匹配质量，再与大厅健康度合成为列表排序信息。

## 自定义关卡与创意工坊

`QuickSaver` 是关卡快照核心：

- 当前场景被序列化为 XML，记录关卡、修正器、方块、变换、父子关系、传送门、颜色和损坏状态。
- XML 使用 LZMA 压缩；本地快照保存为 `.snapshot`，远端地图以八位地图码索引。
- `TabletSaveAndShareScreen` 上传压缩快照及独立缩略图；`GameSparksQuery`/`BraincloudQuery` 负责码、下载 URL、发布状态、投票、举报和挑战成绩。
- `UndergroundComputer` 提供本地地图、最近地图、收藏、在线搜索、筛选、评分、举报和传送门槽位管理。
- `CustomLevelPortal` 在树屋中同步地图码、名称、作者和外观；实际 XML 通过快照加载流程取得并在关卡开始前分发。

## 存档、解锁与进度

- `SaveFileData` 保存统计、音频/输入/联机选项、技能评分、规则快照、地图历史、收藏和传送门槽位。
- `StatTracker` 将 XML 进行 Base64 编码后写入 `Application.persistentDataPath/saveData.uch`，写入前保留 `.bak` 备份；主存档加载失败时会尝试备份。
- 解锁状态位于 `CharactersUnlocked`、`LevelsUnlocked` 和 `OutfitsUnlocked` 统计项，树屋中的 `LevelSelectController` 和 `UnLockInfo` 驱动可解锁内容展示及确认。
- `GameRulePreset` 保存规则预设；`ModSource` 负责把当前修正器写入或从 XML 读回。

## 输入、UI、音频和扩展功能

- `PlayerManager`、`Controller`、`KeyboardInput` 和 Rewired 共同管理本地多人输入。
- I2 Localization 提供多语言文本；`WordFilter`/Crosstales 处理文本过滤。
- Wwise 负责音频事件和 RTPC 音量；Discord、Steamworks 和 Twitch 模块分别处理社交状态、平台身份和观众投票。
- `TwitchChatController` 仅在房主开启派对模式投票时汇总每个用户名的一票，并把排名靠前的物件放入 Party Box。

## 对跳跃插件的直接结论

插件补丁目标 `Modifiers.get_JumpSpeed()` 与实际源码匹配：

- 普通地面跳和空中/多段跳在 `Character.cs` 中直接读取 `JumpSpeed`。
- `Modifiers.WallJumpVerticalPush` 直接由 `JumpSpeed` 计算；`WallJumpHorizontalPush` 在游戏修正器启用时也间接读取它。
- `Character` 的基础竖直速度下限使用 `JumpSpeed` 参与计算。
- 装备喷气背包时，普通跳跃的初始冲量仍使用 `JumpSpeed`，之后再乘以游戏自身的喷气背包系数；持续喷气推力是独立参数。

该插件不是游戏规则同步机制：`Modifiers` 是各客户端本地单例，而角色物理由持有该角色的本地路径执行。在线房间中，如果不同客户端使用不同插件配置，行为和观感不应视为一致的规则集。

游戏内置修正器之所以能保持一致，是因为规则书会同步 `ModSource` XML；当前插件只在本地对属性返回值做 Harmony 后处理，不会写入该 XML 或 `MsgApplyRuleset`。

## 已定位的源码疑点

以下结论来自反编译出的客户端代码，尚未对游戏运行时程序集施加补丁。直接修改 `GameSource/` 不会改变已安装游戏；要修复运行时行为需要单独的 BepInEx/Harmony 补丁并在本机游戏中验证。

1. `ModSource.cs:192` 与 `ModSource.cs:298` 将 `DoomsdayLavaMode` 和 `DoomsdayMeteorsMode` 交叉比较，而不是比较同名字段。这会使“仅修改流星模式”被错误地视为默认修正器，或使规则预设相等性判断错误。
2. `BraincloudManager.cs:87` 的等待条件为 `!Authenticated || startTime + 20f < now`。认证在 20 秒后才完成时，第二个条件会一直为真，协程不会按预期超时退出。
3. `BraincloudQuery.cs:107` 的文件上传失败回调只设置 `error`，没有设置 `done = true` 或注销回调；随后 `while (!done)` 会持续等待。这会让失败的地图/缩略图上传停留在进行中状态。

## 关键入口文件

| 领域 | 主要文件 |
|---|---|
| 全局状态与规则 | `GameState.cs`, `GameSettings.cs`, `Modifiers.cs` |
| 大厅和匹配 | `Matchmaker.cs`, `SteamMatchmaker.cs`, `GamesparksMatchmaker.cs`, `LobbyManager.cs` |
| 对局控制 | `GameControl.cs`, `VersusControl.cs`, `ChallengeControl.cs`, `FreePlayControl.cs` |
| 角色和物件 | `Character.cs`, `Placeable.cs`, `NetworkSurrogate.cs` |
| 快照和 UGC | `QuickSaver.cs`, `UndergroundComputer.cs`, `CustomLevelPortal.cs`, `GameSparksQuery.cs` |
| 后端与中继 | `BraincloudManager.cs`, `BraincloudQuery.cs`, `UCHServices/Service.cs`, `MLAPI.Relay.Transports/UnetRelayTransport.cs` |
| 存档与统计 | `SaveFileData.cs`, `StatTracker.cs`, `XMLSaver.cs` |

## 第三方程序集

- `com.rlabrecque.steamworks.net`: Steamworks.NET 封装。
- `BrainCloud`: BrainCloud SDK 和 WebSocket/HTTP 支持。
- `GameSparks`: 旧 API 类型，供兼容抽象层使用。
- `Telepathy`: 网络传输库。
- `Assembly-CSharp-firstpass`: Discord、Dreamteck Splines、Smooth Sync、Crosstales 等较早加载的第三方代码。
- `Moserware.Skills`: TrueSkill 匹配质量计算；`SevenZip`: 关卡快照的 LZMA 压缩；`nn.*`: Nintendo Switch 平台 API。
- 其余运行时依赖包括 Unity UNet、Rewired、Wwise、I2 Localization、Google Protobuf、UniTask 和 LZMA 实现。
