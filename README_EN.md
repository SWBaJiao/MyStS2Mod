# FriendlyFire-StS2 — Slay the Spire 2 Friendly Fire Mod

**🌐 Language:** English | [日本語](README_JA.md) | [한국어](README_KO.md) | [中文](README.md)

> Hold `Alt` to "friendly" slash your teammates with attack cards.

![Slay the Spire 2](https://img.shields.io/badge/Slay%20the%20Spire%202-Mod-red?style=flat-square)
![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blue?style=flat-square)
![Harmony 2.4.2](https://img.shields.io/badge/Harmony-2.4.2-green?style=flat-square)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)
![AI Assisted](https://img.shields.io/badge/AI%20Assisted-Claude-blueviolet?style=flat-square)

---

## Features

| Feature | Description |
|---------|-------------|
| **Single-target Friendly Fire** | Hold `Alt` to target teammates with `AnyEnemy` attack cards |
| **AOE Expansion** | Hold `Alt` to make `AllEnemies` AOE cards hit **other players' characters** (excludes self and own summons) |
| **Full Effect Application** | Card debuffs (Vulnerable, Weak, etc.) apply to teammates normally |
| **JSON Whitelist** | Fine-grained control over which cards allow friendly fire via config |
| **Dangerous Card Protection** | Auto-blocks cards that access `Monster` property to prevent crashes |
| **On-screen Indicator** | Red "Friendly Fire ON" banner appears when holding the toggle key |
| **Multiplayer Safe** | TargetId signal mechanism ensures all clients stay in sync |

---

## Installation Guide

> **Important: Back up your save files before installing any mod!**
>
> Save location:
> - **Windows:** `%APPDATA%\..\Roaming\SlayTheSpire2\`
> - **macOS:** `~/Library/Application Support/SlayTheSpire2/`

### Step 1: Locate the Game Directory

| Platform | Path |
|----------|------|
| **Windows** | `C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\` |
| **macOS** | `~/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/Resources/` |

> **Tip:** In Steam, right-click the game → Manage → Browse Local Files.

### Step 2: Create the mods Folder

Create a folder named `mods` in the game root directory (skip if it already exists).

### Step 3: Install BaseLib (Dependency)

This mod requires [Alchyr/BaseLib-StS2](https://github.com/Alchyr/BaseLib-StS2). **Install it first.**

1. Download the latest release from [BaseLib-StS2 Releases](https://github.com/Alchyr/BaseLib-StS2/releases)
2. Extract the `BaseLib` folder into `mods/`

### Step 4: Install FriendlyFire

1. Download the latest `FriendlyFire.zip` from [Releases](../../releases)
2. Extract the `FriendlyFire` folder into `mods/`

```
mods/
  +-- BaseLib/                      <-- Dependency (Step 3)
  +-- FriendlyFire/                 <-- This mod
        +-- FriendlyFire.dll
        +-- FriendlyFire.pck
        +-- mod_manifest.json
        +-- friendly_fire_config.cfg
```

### Step 5: Launch the Game

1. Launch Slay the Spire 2
2. Go to Main Menu → **Mod Manager**
3. Enable both **BaseLib** and **Friendly Fire**
4. Start a co-op battle

### How to Use

| Action | Effect |
|--------|--------|
| Play attack card **without Alt** | Normal behavior, same as vanilla |
| Play single-target card **with Alt held** | Can select teammates as targets; red indicator appears |
| Play AOE card **with Alt held** | AOE hits all enemies + other players' characters (not self or own summons) |

> **Multiplayer Note:** All players must install the **same version** of the mod with **identical** whitelist configs. The host should distribute the config file.

### Uninstall

1. Delete the `mods/FriendlyFire/` folder
2. Restart the game — saves are not affected

---

## Configuration

Edit `friendly_fire_config.cfg` to customize mod behavior. Restart the game after changes.

```jsonc
{
  // Hold this key to enable friendly fire. Options: Alt, Shift, Ctrl, Tab, Space, F1~F4
  "toggle_key": "Alt",

  // Single-target attack whitelist (card class names)
  // Empty [] = all single-target attack cards allowed
  "single_target_whitelist": [],

  // Enable AOE friendly fire expansion
  "aoe_enabled": true,

  // AOE attack whitelist, same rules as above
  "aoe_whitelist": [],

  // Dangerous cards blacklist (cards that crash when accessing Target.Monster)
  "dangerous_cards_blacklist": []
}
```

---

## FAQ

**Q: Will friendly fire damage myself?**
> No. AOE excludes the attacker and all their summons/pets.

**Q: Does it work in single player?**
> Single-target has no teammate to target. AOE has no ally to hit. This mod is designed for **co-op multiplayer**.

**Q: Will all card effects apply?**
> Yes. Damage, debuffs (Vulnerable, Weak, Poison, etc.) all apply normally. The only exception is cards accessing `Monster` properties — those use safe alternative logic.

**Q: Will multiplayer desync?**
> No. The mod uses TargetId signals to ensure all clients compute identical target lists. All players must have the same mod version and config.

---

## AI Development Note

This project was developed with extensive AI assistance (Claude). See the [Chinese README](README.md) for detailed technical architecture and AI development tips.

---

## License

[MIT License](LICENSE) — Free to use, modify, and distribute.
