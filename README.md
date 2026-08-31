# Springheel

A [BepInEx](https://bepinex.dev/) client plugin for [Ultimate Chicken Horse](https://www.ultimatechickenhorse.com/) that raises your jump velocity and lets you shrug off trap deaths — with an F8 toggle, an on-screen status pill, and live config reloading. Built and tested against the Steam release.

## Features

- **Jump multiplier** — one Harmony postfix on `Modifiers.get_JumpSpeed` scales jump velocity for every jump type: ground jumps, air/multi-jumps, wall jumps (vertical and horizontal), jetpack takeoffs, and the upward velocity cap.
- **Trap damage immunity** — optionally ignore trap deaths. Falling, drowning, lava, suicide, retry, AFK auto-kill, and run-timer deaths still apply, so you can always reset yourself.
- **Hotkey toggle** — press F8 (configurable, e.g. `LeftControl + F8`) mid-match to flip immunity; a toast confirms the change.
- **On-screen status pill** — a small overlay in the corner shows whether immunity is ON or OFF (and the active hotkey) during an active match.
- **Config hot reload** — a `FileSystemWatcher` applies saved config edits without restarting the game.

## Configuration

Settings live in `BepInEx/config/uch.jumpmod.cfg`:

| Section          | Key              | Type               | Default | Description                                                        |
| ---------------- | ---------------- | ------------------ | ------- | ------------------------------------------------------------------ |
| `General`        | `Enabled`        | `bool`             | `true`  | Master toggle for the mod.                                         |
| `Jump`           | `JumpMultiplier` | `float`            | `1.15`  | Jump velocity multiplier; `1.00` = vanilla. Read at startup — restart to change. |
| `DamageImmunity` | `Enabled`        | `bool`             | `false` | Ignore trap deaths.                                                |
| `DamageImmunity` | `ToggleHotkey`   | `KeyboardShortcut` | `F8`    | Toggle immunity mid-match, e.g. `F8` or `LeftControl + F8`.        |

`DamageImmunity.Enabled` and `DamageImmunity.ToggleHotkey` hot-reload on save; `Jump.JumpMultiplier` does not.

## Installation

1. Install [BepInEx 5.x](https://bepinex.dev/) into the game folder and run the game once to generate BepInEx's folders.
2. Copy the compiled `UCHJumpMod.dll` into `BepInEx/plugins/`.
3. Launch the game; the log should show `[UCH Jump Mod] Patched OK!`.

Plugin GUID: `uch.jumpmod`, version 1.2.0.

## Building

Targets .NET Standard 2.1 and references game/BepInEx assemblies from your Steam install. Adjust the `HintPath` entries in `JumpMod/UCHJumpMod.csproj` if Steam is not at the default location, then:

```sh
dotnet build JumpMod/UCHJumpMod.csproj -c Release
```

`GameSource/` holds decompiled game reference source (e.g. `Assembly-CSharp/Modifiers.cs`, `Character.cs`) for verifying the patched jump and death paths. It is documentation only — not compiled or shipped.

## ConfigEditor

`ConfigEditor/` is a standalone WinForms tool (`.NET 8`) for editing the config without hunting for the file: it auto-locates the game, exposes sliders/checkboxes, captures the hotkey from a key press, and can launch the game through Steam.

```sh
dotnet publish ConfigEditor/ConfigEditor.csproj -c Release
```

`ConfigEditor.Tests/` (net9.0 console) sanity-checks the project — run with the project root and a deployed config path as arguments:

```sh
dotnet run --project ConfigEditor.Tests -- <project-root> <deployed-config-path>
```

## Disclaimer

Intended for **offline / solo play and your own save files**. Using it in online lobbies may be considered cheating; if you do, that is entirely at your own risk.

## License

[MIT](LICENSE) — © 2026 UCH Jump Mod contributors.
