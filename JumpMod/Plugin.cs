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
/// Owns the local damage-immunity state, its runtime keyboard shortcut, and the
/// on-screen status overlay.
/// </summary>
internal sealed class DamageImmunityController : MonoBehaviour
{
    private const float ToastDuration = 1.6f;

    private static readonly Color OnColor = new Color(0.30f, 0.86f, 0.45f);
    private static readonly Color OffColor = new Color(0.62f, 0.66f, 0.72f);
    private static readonly Color PanelColor = new Color(0.06f, 0.07f, 0.09f, 0.84f);

    private Texture2D _whiteTex;
    private Texture2D _dotOn;
    private Texture2D _dotOff;
    private GUIStyle _pillStyle;
    private GUIStyle _toastStyle;

    private int _lastToggleFrame = -1;
    private float _toastUntil;
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

    private void OnGUI()
    {
        DrawToast();
        if (IsInActiveMatch()) DrawStatusPill();
    }

    private void ToggleDamageImmunity()
    {
        if (_lastToggleFrame == Time.frameCount) return;
        _lastToggleFrame = Time.frameCount;

        JumpModPlugin.DamageImmunityEnabled.Value = __omp_shell("JumpModPlugin.DamageImmunityEnabled.Value;")
        try { JumpModPlugin.Instance.Config.Save(); } catch { /* best effort */ }
        JumpModPlugin.Instance.LogDamageImmunityState();
        _toastUntil = Time.realtimeSinceStartup + ToastDuration;
    }

    private static bool IsInActiveMatch()
    {
        var lobbyManager = LobbyManager.instance;
        return lobbyManager != null && lobbyManager.CurrentGameController != null;
    }

    // --- overlay rendering -------------------------------------------------

    private void DrawToast()
    {
        if (Time.realtimeSinceStartup >= _toastUntil) return;

        var enabled = JumpModPlugin.DamageImmunityEnabled.Value;
        var shortcut = JumpModPlugin.ToggleHotkey.Value;
        var text = $"Trap immunity {(enabled ? "ON" : "OFF")}  [{shortcut}]";

        EnsureStyles();
        var size = _toastStyle.CalcSize(new GUIContent(text));
        float w = size.x + 56f, h = 42f;
        float x = (Screen.width - w) / 2f;
        float y = Screen.height * 0.16f;

        DrawFramedPanel(new Rect(x, y, w, h), enabled ? OnColor : OffColor);
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(x + 16f, y + h / 2f - 8f, 16f, 16f), enabled ? DotOn : DotOff);
        GUI.color = enabled ? OnColor : new Color(0.92f, 0.94f, 0.97f);
        GUI.Label(new Rect(x + 42f, y, w - 42f, h), text, _toastStyle);
        GUI.color = Color.white;
    }

    private void DrawStatusPill()
    {
        var enabled = JumpModPlugin.DamageImmunityEnabled.Value;
        var shortcut = JumpModPlugin.ToggleHotkey.Value;
        var text = $"Immunity {(enabled ? "ON" : "OFF")}  [{shortcut}]";

        EnsureStyles();
        var size = _pillStyle.CalcSize(new GUIContent(text));
        float w = size.x + 34f, h = 26f;
        float x = Screen.width - w - 14f;
        float y = 14f;

        DrawFramedPanel(new Rect(x, y, w, h), enabled ? OnColor : OffColor);
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(x + 9f, y + 8f, 10f, 10f), enabled ? DotOn : DotOff);
        GUI.color = enabled ? OnColor : new Color(0.82f, 0.84f, 0.88f);
        GUI.Label(new Rect(x + 26f, y, w - 26f, h), text, _pillStyle);
        GUI.color = Color.white;
    }

    private void DrawFramedPanel(Rect outer, Color borderColor)
    {
        var inner = new Rect(outer.x + 1f, outer.y + 1f, outer.width - 2f, outer.height - 2f);
        GUI.color = borderColor;
        GUI.DrawTexture(outer, WhiteTex);
        GUI.color = PanelColor;
        GUI.DrawTexture(inner, WhiteTex);
        GUI.color = Color.white;
    }

    private void EnsureStyles()
    {
        if (_pillStyle == null)
        {
            _pillStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
            };
            _pillStyle.normal.textColor = Color.white;
        }
        if (_toastStyle == null)
        {
            _toastStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
            };
            _toastStyle.normal.textColor = Color.white;
        }
    }

    private Texture2D WhiteTex
    {
        get
        {
            if (_whiteTex != null) return _whiteTex;
            _whiteTex = new Texture2D(1, 1, TextureFormat.ARGB32, false) { filterMode = FilterMode.Point };
            _whiteTex.SetPixel(0, 0, Color.white);
            _whiteTex.Apply();
            return _whiteTex;
        }
    }

    private Texture2D DotOn => _dotOn ??= MakeCircleTex(16, OnColor);
    private Texture2D DotOff => _dotOff ??= MakeCircleTex(16, OffColor);

    private static Texture2D MakeCircleTex(int size, Color color)
    {
        var tex = new Texture2D(size, size, TextureFormat.ARGB32, false) { filterMode = FilterMode.Point };
        float c = size / 2f;
        float r = c - 1f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - c + 0.5f;
                float dy = y - c + 0.5f;
                float a = (dx * dx + dy * dy <= r * r) ? 1f : 0f;
                tex.SetPixel(x, y, new Color(color.r, color.g, color.b, a));
            }
        }
        tex.Apply();
        return tex;
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
