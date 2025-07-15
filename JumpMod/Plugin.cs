using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using HarmonyLib;
using UnityEngine;

namespace UCHJumpMod;

[BepInPlugin("uch.jumpmod", "UCH Jump Mod", "1.2.0")]
public class JumpModPlugin : BaseUnityPlugin
{
    internal static ConfigEntry<float> JumpMultiplier;
    internal static ConfigEntry<bool> ModEnabled;
    internal static ConfigEntry<bool> DamageImmunityEnabled;
    internal static ConfigEntry<KeyboardShortcut> ToggleHotkey;
    internal static ConfigEntry<bool> DamageImmunityEnabled;
    internal static ConfigEntry<KeyboardShortcut> ToggleHotkey;
    internal static JumpModPlugin Instance;

    private DateTime _pendingConfigReloadAt = DateTime.MinValue;
    private FileSystemWatcher _configWatcher;

    private void Awake()
    {
        Instance = this;
        ModEnabled = Config.Bind("General", "Enabled", true,
            "Master toggle. Set false to disable the mod.");
        JumpMultiplier = Config.Bind("Jump", "JumpMultiplier", 1.15f,
            "Multiplier for jump velocity (ground, air, wall, velocity cap, and jetpack takeoff). 1.00 = vanilla; 1.15 = 15% more jump velocity.");
        DamageImmunityEnabled = Config.Bind("DamageImmunity", "Enabled", false,
            "Ignore trap deaths. Falling, drowning, lava, suicide, retry, AFK auto-kill, and run timer deaths still apply.");
        ToggleHotkey = Config.Bind("DamageImmunity", "ToggleHotkey", new KeyboardShortcut(KeyCode.F8),
            "Toggle damage immunity while playing. Examples: F8 or LeftControl + F8.");
        EnsureToggleHotkey();

        gameObject.AddComponent<DamageImmunityController>();
        DamageImmunityEnabled = Config.Bind("DamageImmunity", "Enabled", false,
            "Ignore trap deaths. Falling, drowning, lava, suicide, retry, AFK auto-kill, and run timer deaths still apply.");
        ToggleHotkey = Config.Bind("DamageImmunity", "ToggleHotkey", new KeyboardShortcut(KeyCode.F8),
            "Toggle damage immunity while playing. Examples: F8 or LeftControl + F8.");
        EnsureToggleHotkey();

        gameObject.AddComponent<DamageImmunityController>();

        try
        {
            Harmony.CreateAndPatchAll(typeof(Patches), "uch.jumpmod");
            Logger.LogInfo($"[UCH Jump Mod] Patched OK! JumpMultiplier={JumpMultiplier.Value}, Enabled={ModEnabled.Value}, DamageImmunity={DamageImmunityEnabled.Value}, ToggleHotkey={ToggleHotkey.Value}");
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"[UCH Jump Mod] FAILED to patch: {ex}");
            throw;
        }

        InstallConfigWatcher();
        Logger.LogInfo("[UCH Jump Mod] Hotkey and damage-immunity changes apply live; jump multiplier still needs a restart.");
    }

    /// <summary>
    /// Watches the config file so edits made in the external ConfigEditor (or any
    /// text editor) are picked up by the running game without a restart. The hotkey
    /// and damage-immunity toggle then take effect immediately.
    /// </summary>
    private void InstallConfigWatcher()
    {
        try
        {
            var cfgPath = Config.ConfigFilePath;
            var dir = Path.GetDirectoryName(cfgPath);
            var name = Path.GetFileName(cfgPath);
            if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(name)) return;

            _configWatcher = new FileSystemWatcher(dir, name)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                EnableRaisingEvents = true,
            };
            _configWatcher.Changed += (_, __) => _pendingConfigReloadAt = DateTime.UtcNow.AddMilliseconds(400);
            _configWatcher.Created += (_, __) => _pendingConfigReloadAt = DateTime.UtcNow.AddMilliseconds(400);
        }
        catch (Exception ex)
        {
            Logger.LogWarning("[UCH Jump Mod] Config file watcher disabled: " + ex.Message);
        }
    }

    private void Update()
    {
        if (_pendingConfigReloadAt == DateTime.MinValue) return;
        if (DateTime.UtcNow < _pendingConfigReloadAt) return;

        _pendingConfigReloadAt = DateTime.MinValue;
        try
        {
            Config.Reload();
            EnsureToggleHotkey();
            Logger.LogInfo($"[UCH Jump Mod] Config reloaded from disk. DamageImmunity={DamageImmunityEnabled.Value}, ToggleHotkey={ToggleHotkey.Value}");
        }
        catch (Exception ex)
        {
            Logger.LogWarning("[UCH Jump Mod] Config reload failed: " + ex.Message);
        }
    }

    internal void LogDamageImmunityState()
    {
        Logger.LogInfo($"[UCH Jump Mod] Damage immunity {(DamageImmunityEnabled.Value ? "enabled" : "disabled")}.");
    }

    private void EnsureToggleHotkey()
    {
        if (ToggleHotkey.Value.MainKey != KeyCode.None) return;

        ToggleHotkey.Value = new KeyboardShortcut(KeyCode.F8);
        Config.Save();
        Logger.LogWarning("[UCH Jump Mod] ToggleHotkey was not set and has been reset to F8.");
    }
}


/// <summary>
/// Owns the local damage-immunity state and its runtime keyboard shortcut.
/// </summary>
internal sealed class DamageImmunityController : MonoBehaviour
{
    private bool _prevKeyHeld;

    private void Update()
    {
        var shortcut = JumpModPlugin.ToggleHotkey.Value;
        if (shortcut.MainKey == KeyCode.None)
        {
            _prevKeyHeld = false;
            return;
        }

        bool modifiersHeld = true;
        foreach (var modifier in shortcut.Modifiers)
        {
            if (!Input.GetKey(modifier)) { modifiersHeld = false; break; }
        }

        bool keyHeld = modifiersHeld && Input.GetKey(shortcut.MainKey);
        bool edge = keyHeld && !_prevKeyHeld;
        bool unityEdge = modifiersHeld && Input.GetKeyDown(shortcut.MainKey);
        _prevKeyHeld = keyHeld;

        if (edge || unityEdge) ToggleDamageImmunity();
    }

    private void ToggleDamageImmunity()
    {
        JumpModPlugin.DamageImmunityEnabled.Value = __omp_shell("JumpModPlugin.DamageImmunityEnabled.Value;")
        try { JumpModPlugin.Instance.Config.Save(); } catch { /* best effort */ }
        JumpModPlugin.Instance.LogDamageImmunityState();
    }

    // --- death blocking ----------------------------------------------------

    internal static bool ShouldBlockDeath(Character character, string cause)
    {
        if (character == null || !character.hasAuthority || !JumpModPlugin.ModEnabled.Value || !JumpModPlugin.DamageImmunityEnabled.Value)
        {
            return false;
        }

        return !IsAlwaysAllowedDeath(cause);
    }

    private static bool IsAlwaysAllowedDeath(string cause)
    {
        return string.Equals(cause, "Falling", StringComparison.Ordinal) ||
               string.Equals(cause, "Drowning", StringComparison.Ordinal) ||
               string.Equals(cause, "Drowning_In_Lava", StringComparison.Ordinal) ||
               string.Equals(cause, "Suicide", StringComparison.Ordinal) ||
               string.Equals(cause, "Retry", StringComparison.Ordinal) ||
               string.Equals(cause, "AFK Auto-Kill", StringComparison.Ordinal) ||
               string.Equals(cause, "Run Timer", StringComparison.Ordinal);
    }
}
