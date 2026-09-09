# Walkthrough: Player Suit & Hair Customization & 3rd-Person Follow Camera

## 1. Upgraded 3rd-Person Follow Camera (`F5`)
The third-person camera has been adjusted to lower both its height and look target right behind Robin's body:

### Camera Framing & Height:
- **Base Camera Height**: Lowered from `1.2m` down to `0.15m - 0.35m` above the player root (a full ~1 meter lower!), placing the camera directly level behind Robin's back and torso.
- **Look-At Target**: Lowered from `1.0m` down to `0.0m` (Robin's body center), so the camera aims directly at her suit and body rather than pointing upward into the sky.
- **Dynamic Height Control**:
  - Tap or hold **`Up Arrow`** or **`Down Arrow`** while in third person (`F5`) to raise or lower the camera height in real time.
  - Type `/camheight <offset>` or `/height <offset>` in chat console (range: `-1.5m` to `+1.5m`) to set an exact height offset.
- **Distance Range**:
  - Minimum: `1.0m` (close-up).
  - Maximum: `3.5m` (strictly capped at original maximum distance).
  - Default: `2.8m`.

### Controls Summary (while in 3rd Person `F5`):
| Control | Action | Details |
|:---|:---|:---|
| **`Up Arrow` / `Down Arrow`** | Raise / lower camera height | Dynamically adjusts vertical camera elevation relative to Robin's body. |
| **`Alt + Mouse Scroll Wheel`** | Zoom in / out | Holding `Alt` (or `Ctrl`) while scrolling zooms smoothly without switching quickslot tools. |
| **`[` and `]`** | Zoom in / out | Tap or hold `[` to zoom in / bring camera closer; `]` to zoom out / move camera farther. |
| **`PageUp` / `PageDown`** | Zoom in / out | Dedicated keyboard keys to bring camera closer or farther. |
| **`Keypad +` / `Keypad -`** | Zoom in / out | Numpad keys to bring camera closer or farther. |
| **`/camheight <offset>`** | Set camera height | Console command (e.g. `/camheight 0`, `/camheight -0.3`, `/camheight 0.4`). |
| **`/zoom <distance>`** | Set camera distance | Console command (e.g. `/zoom 1.5` or `/camdist 2.8`). |

---

## 2. Suit & Hair Customization Controls:
| Action | Control / Command | Notes |
|:---|:---|:---|
| **Toggle 3rd Person** | **`F5`** | Shows Robin's head, hair, suit, and flippers with full shadow casting. |
| **Cycle Suit Color** | **`F6`** or `/suitcolor <name\|num>` or `/color <name\|num>` | Cycles through 10 suit colors; Player 2 defaults to Ocean Cyan. |
| **Cycle Hair Color** | **`F7`** or `/haircolor <name\|num>` or `/hair <name\|num>` | Cycles through 10 hair colors; fully opaque, solid Cutout shading. |
| **List Available Colors** | `/haircolor list` or `/suitcolor list` | Displays all names and numbers in chat. |

---

## 3. Verification & Deployment:
- **Build**: Built `Subnautica.BelowZero.Multiplayer.csproj` in Release mode (`0 Errores`).
- **Deploy**: Deployed updated `Subnautica.Client.dll` to both `D:\SteamLibrary\steamapps\common\SubnauticaZero\BepInEx\plugins\Subnautica.BelowZero.Multiplayer` and `SubnauticaMultiplayer`.
