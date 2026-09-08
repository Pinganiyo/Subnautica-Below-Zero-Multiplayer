# Linux support (fork notes)

> Upstream: https://github.com/ismail0234/Subnautica-Below-Zero-Multiplayer
> Goal of this fork: run the Below Zero multiplayer mod on Linux (Proton).

## ⚠️ License warning

Upstream `LICENCE.md` is **All Rights Reserved**:

> No part of this repository may be reproduced, distributed, or transmitted
> in any form ... without the prior written permission of the author(s).

And `README.md` states the source is reference-only, no PRs accepted, and may
not be the latest version. **Do not publish/redistribute a fork or binaries
without written permission from the author.** The steps below assume private /
personal use until permission is granted.

## Quick start (Linux)

```bash
# 1. point at your install (or let the script auto-detect Steam paths)
export SUBNAUTICA_DIR="$HOME/.steam/steam/steamapps/common/SubnauticaZero"

# 2. run installer
./install-linux.sh
```

Then in Steam:

- Right click `Subnautica: Below Zero` → Properties → Launch Options:
- Set: `WINEDLLOVERRIDES="winhttp=n,b" %command%`

This makes Proton load BepInEx's `winhttp.dll`.

### BepInEx

- Required: **BepInEx 5.x** (tested `5.4.23.2` x64). BepInEx 6.x will not work.
- The script uses, in order:
  1. `lib/BepInEx/` (vendored — create this dir and unzip BepInEx there), or
  2. `$BEPINEX_ZIP` env var pointing at a local zip, or
  3. download from GitHub releases via curl/wget.

### Build vs. prebuilt DLLs

Upstream publishes **source-only, no `.sln`/`.csproj`**. So
`install-linux.sh` handles two modes:

- If `*.sln` / `*.csproj` exist (i.e. you reconstructed them in this fork),
  it runs `dotnet build -c Release -p:GameManagedPath=...`.
- Otherwise it deploys prebuilt `*.dll` from `./build/` (or `PREBUILT_DIR=...`).

Drop your compiled `Subnautica.*.dll` files into `./build/` if you build on
Windows / in another tree.

`Data/SpawnPoints.bin` is copied to
`BepInEx/plugins/SubnauticaMultiplayer/Data/` automatically.

## Known Linux blockers (from code inspection)

1. **`FirewallApi.cs` uses `netsh.exe`** (`SpecialFolder.System` + `netsh.exe`)
   — Windows-only firewall helper. Must be stubbed/disabled on Linux (guard
   with `OperatingSystem.IsWindows()` / `PlatformID` check), otherwise server
   hosting throws.
2. **Discord RPC native pipe** (`DiscordRPCNativeNamedPipe.dll`,
   `NamedPipeUnity` / `NamedPipeClientStream`) — Windows native DLL + named
   pipes. Works under Proton sometimes, but should fail gracefully (try/catch
   + disable Rich Presence on `DllNotFoundException`). See
   `Subnautica.Client/Modules/DiscordRichPresence.cs`.
3. **AppData path**: `Paths.AppData` = `SpecialFolder.ApplicationData` →
   `~/.config` on Linux/Mono, but docs/scripts historically used
   `~/.botbenson`. The installer creates **both**
   `~/.config/.botbenson/...` and `~/.botbenson/...` (symlinked) so either
   lookup works.
4. **`System.Reflection.Emit*.dll` stubs**: some NuGet graphs drop dummy
   facades that break Unity/Mono's native Emit. The installer deletes them
   from the plugin dir (kept from your original script).
5. **Launcher `.exe` paths** (`GetLauncherTempFile`, `GetLauncherNewVersionFile`,
   `GetNowLauncherFile`) assume Windows executables — irrelevant for BepInEx
   plugin flow, but the auto-updater/launcher app won't run on Linux without
   Proton or a rewrite.

## Creating the actual GitHub fork

`gh` CLI is not required. Manual flow:

```bash
# 1. On github.com, open upstream and click Fork → create under your account.
# 2. Then rewire this clone:
cd /path/to/BZMultiplayer
git remote rename origin upstream
git remote add origin https://github.com/<YOUR_USER>/Subnautica-Below-Zero-Multiplayer.git
git push -u origin main  # or master, check `git branch --show-current`
```

Keep `upstream` remote to pull future source drops:

```bash
git fetch upstream
git merge upstream/main
```
