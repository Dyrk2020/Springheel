# Damage Immunity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a local, hotkey-controlled damage immunity feature to UCH Jump Mod while preserving falling deaths.

**Architecture:** A `DamageImmunityController` owns the BepInEx configuration and runtime hotkey state. It repairs an empty hotkey to `F8`, checks both Unity input and IMGUI key events, and draws a match-only status subtitle. A Harmony Prefix on `Character.setupDeath` skips only local-authority trap deaths, using an explicit allow-list for falling, drowning, lava, and game-rule deaths.

**Tech Stack:** C#, BepInEx 5 configuration, Harmony, UnityEngine, WinForms, .NET 9 regression console.

---

### Task 1: Define Regression Expectations

**Files:**
- Modify: `ConfigEditor.Tests/Program.cs`

- [x] **Step 1: Write the failing test**

```csharp
CheckPresent(plugin, "DamageImmunityEnabled", "The plugin has no damage-immunity setting.", failures);
CheckPresent(plugin, "ToggleHotkey", "The plugin has no configurable immunity hotkey.", failures);
CheckPresent(plugin, "DamageImmunityController", "The plugin has no runtime immunity controller.", failures);
CheckPresent(plugin, "setupDeath", "The plugin does not patch the unified death entry point.", failures);
CheckPresent(plugin, "Falling", "The plugin does not preserve falling deaths.", failures);
```

- [x] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet run --project ConfigEditor.Tests\ConfigEditor.Tests.csproj -- . "C:\Program Files (x86)\Steam\steamapps\common\Ultimate Chicken Horse\BepInEx\config\uch.jumpmod.cfg"
```

Expected: failure reporting missing immunity functionality.

### Task 2: Implement Plugin Runtime Behavior

**Files:**
- Modify: `JumpMod/Plugin.cs`

- [x] **Step 1: Add configuration and controller**

```csharp
DamageImmunityEnabled = Config.Bind("DamageImmunity", "Enabled", false, ...);
ToggleHotkey = Config.Bind("DamageImmunity", "ToggleHotkey", new KeyboardShortcut(KeyCode.F8), ...);
gameObject.AddComponent<DamageImmunityController>();
```

- [x] **Step 2: Add the death Prefix**

```csharp
[HarmonyPatch(typeof(Character), "setupDeath")]
[HarmonyPrefix]
private static bool SetupDeath_Prefix(Character __instance, string cause)
{
    return !DamageImmunityController.ShouldBlockDeath(__instance, cause);
}
```

- [x] **Step 3: Run test to verify it passes**

Run the Task 1 command. Expected: `Config editor regression check passed.`

### Task 3: Expose Settings in Configuration Editor

**Files:**
- Modify: `ConfigEditor/MainForm.cs`
- Modify: `ConfigEditor.Tests/Program.cs`

- [x] **Step 1: Extend the test**

```csharp
CheckPresent(mainForm, "Damage Immunity", "The editor has no immunity control.", failures);
CheckPresent(mainForm, "ToggleHotkey", "The editor does not persist the immunity hotkey.", failures);
```

- [x] **Step 2: Add controls and persistence**

Add a checkbox for `DamageImmunity.Enabled` and a validated text input for `DamageImmunity.ToggleHotkey`. Read and write those entries in the BepInEx config format.

- [x] **Step 3: Run the regression check**

Run the Task 1 command. Expected: `Config editor regression check passed.`

### Task 4: Build and Deploy

**Files:**
- Modify: `README.md`
- Modify: `BepInEx/config/uch.jumpmod.cfg` through BepInEx configuration generation or editor output
- Replace: `BepInEx/plugins/UCHJumpMod.dll`
- Replace: `BepInEx/UCHJumpModConfigEditor.exe`

- [x] **Step 1: Update usage documentation**

Document the immunity scope, hotkey defaults, BepInEx hotkey syntax, and falling-death exception.

- [x] **Step 2: Build**

```powershell
dotnet build JumpMod\UCHJumpMod.csproj -c Release
dotnet publish ConfigEditor\ConfigEditor.csproj -c Release
```

- [x] **Step 3: Deploy and verify hashes**

Copy the build outputs to BepInEx, then compare source and destination SHA-256 values. Run the regression command again.
