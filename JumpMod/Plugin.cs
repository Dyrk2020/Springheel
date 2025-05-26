using System;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace UCHJumpMod;

[BepInPlugin("uch.jumpmod", "UCH Jump Mod", "1.2.0")]
public class JumpModPlugin : BaseUnityPlugin
{
    internal static ConfigEntry<float> JumpMultiplier;
    internal static ConfigEntry<bool> ModEnabled;
    internal static JumpModPlugin Instance;

    private void Awake()
    {
        Instance = this;
        ModEnabled = Config.Bind("General", "Enabled", true,
            "Master toggle. Set false to disable the mod.");
        JumpMultiplier = Config.Bind("Jump", "JumpMultiplier", 1.15f,
            "Multiplier for jump velocity (ground, air, wall, velocity cap, and jetpack takeoff). 1.00 = vanilla; 1.15 = 15% more jump velocity.");
        Logger.LogInfo($"[UCH Jump Mod] Loaded. JumpMultiplier={JumpMultiplier.Value}, Enabled={ModEnabled.Value}");
    }
}
