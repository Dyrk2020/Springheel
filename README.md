# Springheel

A [BepInEx](https://bepinex.dev/) client plugin for [Ultimate Chicken Horse](https://www.ultimatechickenhorse.com/) that raises your jump velocity and lets you shrug off trap deaths — with a hotkey, an on-screen status pill, and live config reloading. Plugin GUID: `uch.jumpmod`, version 1.2.0; built and tested against the Steam release.

## Features

- **Jump multiplier** — scales jump velocity for every jump type in the game: ground jumps, air/multi-jumps, wall jumps (vertical and, with game modifiers active, horizontal), jetpack takeoffs, and the upward velocity cap. One Harmony postfix on `Modifiers.get_JumpSpeed` covers them all
- **Trap damage immunity** — optionally ignore trap deaths. Falling, drowning, lava, suicide, retry, AFK auto-kill, and run-timer deaths still apply, so you can always reset yourself
- **Hotkey toggle** — flip immunity on/off mid-match (F8 by default, modifiers supported); a toast confirms the change
- **On-screen status pill** — corner overlay showing ON/OFF (and the active hotkey) during a match
- **Live config reload** — `FileSystemWatcher` on the BepInEx config picks up edits without restarting the game

## Config (`BepInEx/config/uch.jumpmod.cfg`)

| Key | Default | Meaning |
|---|---|---|
| `General.Enabled` | `true` | master toggle |
| `Jump.JumpMultiplier` | `1.15` | multiplier for jump velocity (1.00 = vanilla) |
| `DamageImmunity.Enabled` | `false` | ignore trap deaths |
| `General.ToggleHotkey` | `F8` | immunity toggle key (with modifiers) |

## Install

1. Install [BepInEx 5](https://bepinex.dev/) for Unity (x64)
2. Drop `JumpMod.dll` into `BepInEx/plugins/`
3. Launch the game; toggles are on the F8 hotkey and in the config file

The repo also contains `ConfigEditor/` (WinForms config editor) and decompiled `GameSource/` kept for reference while developing the Harmony patches.
