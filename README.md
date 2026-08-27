# PoultryLeap

A [BepInEx](https://bepinex.dev/) client plugin for [Ultimate Chicken Horse](https://www.ultimatechickenhorse.com/) that raises your jump velocity and lets you shrug off trap deaths — with a hotkey, an on-screen status pill, and live config reloading.

The plugin is built and tested against the Steam release of the game.

## Features

- **Jump multiplier** — scales jump velocity for every jump type in the game: ground jumps, air/multi-jumps, wall jumps (vertical and, when game modifiers are active, horizontal), jetpack takeoffs, and the upward velocity cap. One Harmony postfix on `Modifiers.get_JumpSpeed` covers them all.
- **Trap damage immunity** — optionally ignore trap deaths while playing. Falling, drowning, lava, suicide, retry, AFK auto-kill, and run-timer deaths still apply, so you can always reset yourself.
- **Hotkey toggle** — press the toggle hotkey (F8 by default, with optional modifiers such as `LeftControl + F8`) mid-match to flip immunity on or off. A short toast confirms the change.
- **On-screen status overlay** — a small pill in the corner of the screen shows whether immunity is currently ON or OFF (and the active hotkey) whenever you are in an active match.
- **Config hot reload** — a `FileSystemWatcher` on the BepInEx config file picks up edits from the ConfigEditor (or any text editor) and applies them without restarting the game.

## Configuration

The plugin stores its settings in `BepInEx/config/uch.jumpmod.cfg`:

| Section           | Key            | Type               | Default        | Description                                                                                                                       |
| ----------------- | -------------- | ------------------ | -------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| `General`         | `Enabled`      | `bool`             | `true`         | Master toggle. Set to `false` to disable the mod entirely.                                                                        |
| `Jump`            | `JumpMultiplier` | `float`          | `1.15`         | Multiplier for jump velocity (ground, air, wall, velocity cap, and jetpack takeoff). `1.00` = vanilla; `1.15` = 15% more jump velocity. |
| `DamageImmunity`  | `Enabled`      | `bool`             | `false`        | Ignore trap deaths. Falling, drowning, lava, suicide, retry, AFK auto-kill, and run timer deaths still apply.                      |
| `DamageImmunity`  | `ToggleHotkey` | `KeyboardShortcut` | `F8`           | Toggle damage immunity while playing. Examples: `F8` or `LeftControl + F8`.                                                       |

Hot-reload behaviour: changes to `DamageImmunity.Enabled` and `DamageImmunity.ToggleHotkey` take effect immediately after the config file is saved. The `Jump.JumpMultiplier` value is read once at startup, so changing it still requires a game restart.

## Installation

1. Install [BepInEx 5.x](https://bepinex.dev/) for Ultimate Chicken Horse (extract it into the game folder and run the game once so BepInEx generates its folders).
2. Copy the compiled `UCHJumpMod.dll` into `BepInEx/plugins/` inside the game directory.
3. Launch the game. On startup the log should show `[UCH Jump Mod] Patched OK!` with the active settings.

The plugin's BepInEx GUID is `uch.jumpmod`, version 1.2.0.

## Building

The plugin targets **.NET Standard 2.1** and references game and BepInEx assemblies straight from your Steam install. Before building, adjust the `HintPath` entries in `JumpMod/UCHJumpMod.csproj` if your Steam library is not at the default location (`C:\Program Files (x86)\Steam\steamapps\common\Ultimate Chicken Horse`), then:

```sh
dotnet build JumpMod/UCHJumpMod.csproj -c Release
```

The project references (all with `Private=false`, so nothing is copied into the output):

- Game assemblies: `UnityEngine`, `UnityEngine.CoreModule`, `UnityEngine.InputLegacyModule`, `UnityEngine.IMGUIModule`, `UnityEngine.TextRenderingModule`, `Assembly-CSharp`, `com.unity.multiplayer-hlapi.Runtime`
- BepInEx: `BepInEx.dll`, `0Harmony.dll`

`GameSource/` contains decompiled reference source for the game's assemblies (e.g. `Assembly-CSharp/Modifiers.cs`, `Character.cs`), used to verify the patched jump and death paths. It is documentation only — it is not compiled or shipped.

## ConfigEditor

`ConfigEditor/` is a standalone Windows Forms tool (`UCHJumpModConfigEditor`, .NET 8, WinForms) for editing `BepInEx/config/uch.jumpmod.cfg` without hunting for the file by hand. It auto-locates the game directory, exposes sliders/checkboxes for the settings above, captures the toggle hotkey from a key press, and can launch the game through Steam.

Build it as a self-contained single-file executable (no .NET runtime install needed on the target machine):

```sh
dotnet publish ConfigEditor/ConfigEditor.csproj -c Release
```

`ConfigEditor.Tests/` is a small console program (`net9.0`) that sanity-checks the project: it verifies the expected source files exist and that the editor and deployed config stay consistent with the plugin (e.g. no stale settings, correct Steam app ID, documented jump paths still present). Run it with the project root and a deployed config path as arguments:

```sh
dotnet run --project ConfigEditor.Tests -- <project-root> <deployed-config-path>
```

## Disclaimer

This mod is intended for **offline / solo play and your own save files**. Using it in online lobbies modifies your client relative to other players and may be considered cheating; if you choose to use it online, you do so entirely at your own risk. The authors are not responsible for any account actions, lost progress, or other consequences.

## License

[MIT](LICENSE) — © 2026 UCH Jump Mod contributors.