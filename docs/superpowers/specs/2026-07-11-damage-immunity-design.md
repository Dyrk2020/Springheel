# 伤害免疫设计

## 目标

为现有 `UCHJumpMod` 添加一个可热键切换的本地伤害免疫功能。它拦截普通陷阱伤害，但不改变地形杀死亡。

## 范围

- 在 `Character.setupDeath(string cause, bool deathFreezeOn, int causedByPlayerNumber)` 前执行 Harmony Prefix。
- 只处理拥有本地网络控制权的 `Character`，不干扰远程角色通过 RPC 同步的死亡状态。
- 功能打开时，拦截陷阱、投射物和黑洞等可规避伤害。
- 始终放行 `Falling`、`Drowning`、`Drowning_In_Lava`、`Suicide`、`Retry`、`AFK Auto-Kill` 和 `Run Timer`。
- BepInEx 配置使用 `DamageImmunity.Enabled` 和 `DamageImmunity.ToggleHotkey`；默认关闭，默认热键为 `F8`。
- 空热键在启动时自动修复为 `F8`；热键在游戏运行时立即切换配置值并保存到 `uch.jumpmod.cfg`。
- 热键同时通过 Unity 输入和 IMGUI 键盘事件检测，避免单一路径错过按键。
- 外部配置编辑器提供伤害免疫复选框和热键输入框，并拒绝保存空热键。
- 仅在有 `CurrentGameController` 的实际对局中，于右上角显示当前免疫状态与热键。

## 设计

`DamageImmunityController` 附加在 `BaseUnityPlugin` 的 Unity 对象上。它在 `Update` 检查 Unity 输入，并在 `OnGUI` 检查键盘事件作为回退；同一帧只允许切换一次。命中后切换 `DamageImmunityEnabled`，持久化配置并记录状态；`OnGUI` 同时绘制右上角状态字幕。

Harmony 通过 `Character.setupDeath` 的 Prefix 决定是否执行原方法。Prefix 仅在 `Character.hasAuthority`、主插件启用和伤害免疫启用时开始拦截。它根据死亡原因的固定系统白名单放行坠崖、溺水、岩浆和规则性死亡，其余原因继续被视为可免疫陷阱伤害并跳过原方法。

## 验证

无 Unity 运行时测试框架时，项目的无依赖回归检查验证源码中存在配置项、控制器、补丁目标、权威范围和允许死亡原因；随后执行插件编译，并验证部署 DLL 的 SHA-256 与构建产物一致。
