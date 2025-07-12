using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
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

/// <summary>
/// Harmony patches. Single patch point: Modifiers.get_JumpSpeed postfix.
/// Covers all jump types — all read through this one property:
///   - ground jump   (Character.cs:3496)
///   - air/multijump (Character.cs:3466)
///   - walljump vert  (Modifiers.WallJumpVerticalPush → JumpSpeed * size)
///   - walljump horiz when game modifiers are active (Modifiers.WallJumpHorizontalPush)
///   - jetpack jump impulse (Character.cs:3469: instance2.JumpSpeed * JetpackJumpSpeedModifier)
///   - velocity cap    (Character.cs:3808: Mathf.Max(VMax.y, JumpSpeed * 1.05 * size))
/// </summary>
internal static class Patches
{
    [HarmonyPatch(typeof(Modifiers), "get_JumpSpeed")]
    [HarmonyPostfix]
    private static void JumpSpeed_Postfix(ref float __result)
    {
        if (!JumpModPlugin.ModEnabled.Value) return;
        __result *= JumpModPlugin.JumpMultiplier.Value;
    }
}
