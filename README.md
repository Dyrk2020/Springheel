# Springheel

A [BepInEx](https://bepinex.dev/) plugin for [Ultimate Chicken Horse](https://www.ultimatechickenhorse.com/) that raises your jump velocity and shrugs off trap deaths — built and tested against the Steam release.

- **Jump multiplier** — a Harmony postfix on `Modifiers.get_JumpSpeed` scales jump velocity for every jump type: ground jumps, air/multi-jumps, wall jumps, jetpack takeoffs, and the upward velocity cap.
- **Trap immunity with F8 toggle** — optionally ignore trap deaths (configurable hotkey, e.g. `LeftControl + F8`); falling, drowning, lava, suicide, retry, AFK auto-kill, and run-timer deaths still apply, and saved config edits hot-reload.
- **On-screen status pill** — a small corner overlay shows whether immunity is ON or OFF (and the active hotkey) during an active match.

Intended for offline/solo play and your own save files — online use may be considered cheating, at your own risk. [MIT](LICENSE) © 2026 UCH Jump Mod contributors.
