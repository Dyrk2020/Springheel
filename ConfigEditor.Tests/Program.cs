using System;
using System.Collections.Generic;
using System.IO;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: ConfigEditor.Tests <project-root> <deployed-config-path>");
    return 2;
}

var projectRoot = Path.GetFullPath(args[0]);
var configPath = Path.GetFullPath(args[1]);
var mainFormPath = Path.Combine(projectRoot, "ConfigEditor", "MainForm.cs");
var pluginPath = Path.Combine(projectRoot, "JumpMod", "Plugin.cs");
var pluginProjectPath = Path.Combine(projectRoot, "JumpMod", "UCHJumpMod.csproj");
var modifiersPath = Path.Combine(projectRoot, "GameSource", "Assembly-CSharp", "Modifiers.cs");
var failures = new List<string>();

CheckFileExists(mainFormPath, failures);
CheckFileExists(pluginPath, failures);
CheckFileExists(pluginProjectPath, failures);
CheckFileExists(modifiersPath, failures);
CheckFileExists(configPath, failures);

if (failures.Count == 0)
{
    var mainForm = File.ReadAllText(mainFormPath);
    var plugin = File.ReadAllText(pluginPath);
    var pluginProject = File.ReadAllText(pluginProjectPath);
    var modifiers = File.ReadAllText(modifiersPath);
    var config = File.ReadAllText(configPath);

    CheckAbsent(mainForm, "JetpackMultiplier", "The editor still reads or writes an inert JetpackMultiplier setting.", failures);
    CheckAbsent(mainForm, "_jetpack", "The editor still contains the inert Jetpack UI controls.", failures);
    CheckAbsent(mainForm, "Jetpack Multiplier", "The editor still presents an inert Jetpack Multiplier control.", failures);
    CheckPresent(mainForm, "steam://run/386940", "The editor does not launch Ultimate Chicken Horse's Steam app ID (386940).", failures);
    CheckAbsent(mainForm, "steam://run/384660", "The editor still contains the incorrect Steam app ID (384660).", failures);
    CheckAbsent(config, "JetpackMultiplier", "The deployed config still contains the inert JetpackMultiplier setting.", failures);
    CheckPresent(plugin, "walljump vert", "The plugin source no longer documents the JumpSpeed-derived wall-jump path.", failures);
    CheckPresent(plugin, "jetpack jump impulse", "The plugin source no longer documents the JumpSpeed-derived jetpack takeoff path.", failures);
    CheckPresent(modifiers, "public float WallJumpVerticalPush => JumpSpeed * CharacterSizeSpeedMultiplier;", "The extracted game source no longer confirms that wall-jump vertical push derives from JumpSpeed.", failures);
    CheckPresent(modifiers, "float num2 = JumpSpeed / JumpSpeedValues[0];", "The extracted game source no longer confirms the modifier-enabled wall-jump horizontal dependency.", failures);
    CheckPresent(plugin, "DamageImmunityEnabled", "The plugin has no damage-immunity setting.", failures);
    CheckPresent(plugin, "ToggleHotkey", "The plugin has no configurable immunity hotkey.", failures);
    CheckPresent(plugin, "DamageImmunityController", "The plugin has no runtime damage-immunity controller.", failures);
    CheckPresent(plugin, "LogDamageImmunityState", "The controller has no plugin-owned logging boundary.", failures);
    CheckPresent(plugin, "EnsureToggleHotkey", "The plugin does not repair an unset immunity hotkey.", failures);
    CheckPresent(plugin, "MainKey == KeyCode.None", "The plugin does not detect an unset immunity hotkey.", failures);
    CheckPresent(plugin, "OnGUI", "The plugin has no in-game immunity status overlay.", failures);
    CheckPresent(plugin, "EventType.KeyDown", "The plugin has no IMGUI keyboard-event fallback.", failures);
    CheckPresent(plugin, "Trap immunity:", "The in-game overlay has no trap-immunity state text.", failures);
    CheckPresent(plugin, "CurrentGameController", "The in-game overlay is not scoped to an active match.", failures);
    CheckPresent(plugin, "typeof(Character), \"setupDeath\"", "The plugin does not patch the unified Character.setupDeath entry point.", failures);
    CheckPresent(plugin, "ShouldBlockDeath", "The plugin has no isolated damage-immunity decision function.", failures);
    CheckPresent(plugin, "\"Falling\"", "The plugin does not preserve Falling deaths.", failures);
    CheckPresent(plugin, "\"Drowning\"", "The plugin does not preserve drowning deaths.", failures);
    CheckPresent(plugin, "\"Drowning_In_Lava\"", "The plugin does not preserve lava drowning deaths.", failures);
    CheckPresent(pluginProject, "com.unity.multiplayer-hlapi.Runtime", "The plugin project does not reference Character's UNet base assembly.", failures);
    CheckPresent(pluginProject, "UnityEngine.IMGUIModule", "The plugin project does not reference Unity's IMGUI module for the status overlay.", failures);
    CheckPresent(pluginProject, "UnityEngine.TextRenderingModule", "The plugin project does not reference Unity's text-rendering module for the status overlay.", failures);
    CheckPresent(mainForm, "Damage Immunity", "The editor has no damage-immunity control.", failures);
    CheckPresent(mainForm, "ToggleHotkey", "The editor does not persist the immunity hotkey.", failures);
    CheckPresent(mainForm, "string.IsNullOrWhiteSpace(_toggleHotkeyText.Text)", "The editor can still save an empty immunity hotkey.", failures);
    CheckPresent(config, "[DamageImmunity]", "The deployed config has no damage-immunity section.", failures);
    CheckPresent(config, "ToggleHotkey = F8", "The deployed config has no default immunity hotkey.", failures);
}

if (failures.Count > 0)
{
    Console.Error.WriteLine("Config editor regression check failed:");
    foreach (var failure in failures)
    {
        Console.Error.WriteLine("- " + failure);
    }

    return 1;
}

Console.WriteLine("Config editor regression check passed.");
return 0;

static void CheckFileExists(string path, List<string> failures)
{
    if (!File.Exists(path))
    {
        failures.Add("Required file is missing: " + path);
    }
}

static void CheckAbsent(string text, string value, string message, List<string> failures)
{
    if (text.Contains(value, StringComparison.Ordinal))
    {
        failures.Add(message);
    }
}

static void CheckPresent(string text, string value, string message, List<string> failures)
{
    if (!text.Contains(value, StringComparison.Ordinal))
    {
        failures.Add(message);
    }
}
