#!/usr/bin/env bash
# Subnautica Below Zero Multiplayer - Linux installer / deploy script
# Fork goal: Linux (Proton) support.
#
# What it does:
#   1. Locates the game directory (SUBNAUTICA_DIR or auto-detect Steam paths)
#   2. Builds the mod if project files exist, otherwise deploys prebuilt DLLs
#   3. Installs BepInEx 5 (from lib/BepInEx/ if vendored, else downloads it)
#   4. Deploys plugin DLLs + Data/ assets
#   5. Sets up .botbenson AppData dirs (both ~/.config and ~/ for Mono compat)
#
# Usage:
#   SUBNAUTICA_DIR="/path/to/SubnauticaZero" ./install-linux.sh
#   BEPINEX_ZIP="/path/to/BepInEx_x64_5.4.23.2.zip" ./install-linux.sh
#
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "=========================================================="
echo " Subnautica Below Zero Multiplayer - Linux Installer"
echo "=========================================================="

# ---------------------------------------------------------------- game dir
detect_game_dir() {
    local candidates=(
        "${SUBNAUTICA_DIR:-}"
        "$HOME/.steam/debian-installation/steamapps/common/SubnauticaZero"
        "$HOME/.steam/steam/steamapps/common/SubnauticaZero"
        "$HOME/.local/share/Steam/steamapps/common/SubnauticaZero"
        "$HOME/snap/steam/common/.local/share/Steam/steamapps/common/SubnauticaZero"
    )
    for c in "${candidates[@]}"; do
        if [ -n "$c" ] && [ -d "$c" ]; then
            echo "$c"
            return 0
        fi
    done
    return 1
}

if ! GAME_DIR="$(detect_game_dir)"; then
    echo "Error: Subnautica: Below Zero directory not found."
    echo "Tried default Steam locations, or set SUBNAUTICA_DIR, e.g.:"
    echo '  SUBNAUTICA_DIR="$HOME/.steam/steam/steamapps/common/SubnauticaZero" ./install-linux.sh'
    exit 1
fi
echo "Game directory: $GAME_DIR"

if [ ! -d "$GAME_DIR/SubnauticaZero_Data/Managed" ]; then
    echo "Warning: '$GAME_DIR/SubnauticaZero_Data/Managed' not found."
    echo "Is this the correct game folder? Continuing anyway..."
fi

# ---------------------------------------------------------------- dotnet
DOTNET_CMD=""
if command -v dotnet >/dev/null 2>&1; then
    DOTNET_CMD="dotnet"
elif [ -x "$HOME/.dotnet/dotnet" ]; then
    DOTNET_CMD="$HOME/.dotnet/dotnet"
fi

# ---------------------------------------------------------------- 1. build (if possible)
echo ""
echo "[1/5] Building mod (if project files exist)..."

CSPROJ_COUNT=$(find "$SCRIPT_DIR" -maxdepth 3 -name "*.csproj" | wc -l)
SLN_COUNT=$(find "$SCRIPT_DIR" -maxdepth 2 -name "*.sln" | wc -l)

# Upstream publishes source-only (no .sln/.csproj), so build is optional.
if [ "$SLN_COUNT" -gt 0 ] || [ "$CSPROJ_COUNT" -gt 0 ]; then
    if [ -z "$DOTNET_CMD" ]; then
        echo "Error: .NET SDK not found but project files exist. Install .NET 8 SDK."
        exit 1
    fi
    export PATH="$HOME/.dotnet:$PATH"
    if [ -f "$SCRIPT_DIR/Subnautica.BelowZero.Multiplayer.csproj" ]; then
        BUILD_TARGET="$SCRIPT_DIR/Subnautica.BelowZero.Multiplayer.csproj"
    elif [ "$SLN_COUNT" -gt 0 ]; then
        BUILD_TARGET=$(find "$SCRIPT_DIR" -maxdepth 2 -name "*.sln" | head -n 1)
    else
        # prefer the client plugin if several exist, else first found
        BUILD_TARGET=$(find "$SCRIPT_DIR" -maxdepth 3 -name "*.csproj" -not -path "*/scratch/*" | sort | head -n 1)
    fi
    echo "Building $BUILD_TARGET in Release..."
    "$DOTNET_CMD" build "$BUILD_TARGET" -c Release \
        -p:GameManagedPath="$GAME_DIR/SubnauticaZero_Data/Managed"
    if [ -d "$SCRIPT_DIR/bin/Release/net472" ]; then
        BUILD_DIR="$SCRIPT_DIR/bin/Release/net472"
    else
        BUILD_DIR=$(find "$SCRIPT_DIR" -type d -path "*bin/Release/net*" -not -path "*/scratch/*" | head -n 1 || true)
    fi
else
    echo "No .sln/.csproj found (upstream is source-only)."
    echo "Skipping compile; will deploy prebuilt DLLs from ./build/ or ./bin/ if present."
    BUILD_DIR=""
    for d in "$SCRIPT_DIR/build" "$SCRIPT_DIR/bin/Release/net472" "$SCRIPT_DIR/bin/Release/net48"; do
        if [ -d "$d" ]; then BUILD_DIR="$d"; break; fi
    done
    # also accept a flat drop of dlls next to this script via PREBUILT_DIR
    if [ -z "$BUILD_DIR" ] && [ -n "${PREBUILT_DIR:-}" ] && [ -d "${PREBUILT_DIR:-}" ]; then
        BUILD_DIR="$PREBUILT_DIR"
    fi
fi

if [ -n "${BUILD_DIR:-}" ] && [ -d "$BUILD_DIR" ]; then
    echo "Using assemblies from: $BUILD_DIR"
else
    echo "NOTE: no compiled assemblies found. Place your built *.dll files in ./build/ and re-run,"
    echo "or add proper .csproj files to this fork to enable step [1/4]."
    BUILD_DIR=""
fi

# ---------------------------------------------------------------- 2. BepInEx 5
echo ""
echo "[2/5] Installing BepInEx 5 to game directory..."

BEPINEX_VERSION="5.4.22"
BEPINEX_URL="https://github.com/BepInEx/BepInEx/releases/download/v${BEPINEX_VERSION}/BepInEx_x64_${BEPINEX_VERSION}.0.zip"

install_bepinex_from_dir() {
    # $1 = source dir containing BepInEx (winhttp.dll, BepInEx/ ...)
    echo "Installing BepInEx from $1 ..."
    cp -r "$1"/. "$GAME_DIR/"
}

if [ -f "$SCRIPT_DIR/lib/BepInEx/winhttp.dll" ] || [ -d "$SCRIPT_DIR/lib/BepInEx/BepInEx/core" ]; then
    install_bepinex_from_dir "$SCRIPT_DIR/lib/BepInEx"
elif [ -n "${BEPINEX_ZIP:-}" ] && [ -f "$BEPINEX_ZIP" ]; then
    echo "Extracting BepInEx from $BEPINEX_ZIP ..."
    TMPD="$(mktemp -d)"
    unzip -q -o "$BEPINEX_ZIP" -d "$TMPD"
    cp -r "$TMPD"/. "$GAME_DIR/"
    rm -rf "$TMPD"
elif command -v curl >/dev/null 2>&1 || command -v wget >/dev/null 2>&1; then
    echo "Downloading BepInEx $BEPINEX_VERSION ..."
    TMPD="$(mktemp -d)"
    if command -v curl >/dev/null 2>&1; then
        curl -fL -o "$TMPD/bepinex.zip" "$BEPINEX_URL"
    else
        wget -O "$TMPD/bepinex.zip" "$BEPINEX_URL"
    fi
    unzip -q -o "$TMPD/bepinex.zip" -d "$TMPD/unpacked"
    cp -r "$TMPD/unpacked"/. "$GAME_DIR/"
    rm -rf "$TMPD"
else
    echo "BepInEx not vendored in lib/BepInEx/ and no curl/wget to download it."
    echo "Manually extract BepInEx_x64_${BEPINEX_VERSION}.0.zip into '$GAME_DIR' and re-run."
    echo "Set BEPINEX_ZIP=/path/to/zip to use a local copy."
    exit 1
fi

mkdir -p "$GAME_DIR/BepInEx/plugins/SubnauticaMultiplayer"
PLUGIN_DIR="$GAME_DIR/BepInEx/plugins/SubnauticaMultiplayer"

# ---------------------------------------------------------------- 3. Deploy mod
echo ""
echo "[3/5] Deploying multiplayer mod assemblies..."

if [ -n "$BUILD_DIR" ]; then
    # copy dlls (and pdbs for debugging)
    cp -v "$BUILD_DIR"/*.dll "$PLUGIN_DIR/"
    cp -v "$BUILD_DIR"/*.pdb "$PLUGIN_DIR/" 2>/dev/null || true

    # Remove build-time and dummy stubs to allow Unity/Mono native Reflection.Emit
    rm -f "$PLUGIN_DIR"/AsmResolver*.dll \
          "$PLUGIN_DIR"/BepInEx.AssemblyPublicizer*.dll \
          "$PLUGIN_DIR"/Microsoft.NET.StringTools*.dll \
          "$PLUGIN_DIR"/System.Reflection.Emit*.dll 2>/dev/null || true
else
    echo "Skipped (no assemblies). Plugin dir created at $PLUGIN_DIR"
fi

# Copy Data directory assets (SpawnPoints.bin etc.)
if [ -d "$SCRIPT_DIR/Data" ]; then
    mkdir -p "$PLUGIN_DIR/Data"
    cp -r "$SCRIPT_DIR/Data/." "$PLUGIN_DIR/Data/"
    echo "Copied Data/ assets to plugin directory."
fi

# Remove disabled mods if present
if [ -d "$GAME_DIR/BepInEx/plugins/RadialTabs" ]; then
    echo "Removing disabled RadialTabs..."
    rm -rf "$GAME_DIR/BepInEx/plugins/RadialTabs"
fi

if [ -d "$GAME_DIR/BepInEx/plugins/CurtainsandBlinds" ]; then
    echo "Removing disabled CurtainsandBlinds..."
    rm -rf "$GAME_DIR/BepInEx/plugins/CurtainsandBlinds"
fi

# Deploy bundled mod collection to BepInEx/plugins
if [ -d "$SCRIPT_DIR/lib/plugins" ]; then
    echo "Deploying bundled mod collection to $GAME_DIR/BepInEx/plugins/ ..."
    mkdir -p "$GAME_DIR/BepInEx/plugins"
    cp -rn "$SCRIPT_DIR/lib/plugins/." "$GAME_DIR/BepInEx/plugins/" 2>/dev/null || cp -r "$SCRIPT_DIR/lib/plugins/." "$GAME_DIR/BepInEx/plugins/"
fi

# Deploy mod configurations to BepInEx/config
if [ -d "$SCRIPT_DIR/lib/config" ]; then
    echo "Deploying mod configurations to $GAME_DIR/BepInEx/config/ ..."
    mkdir -p "$GAME_DIR/BepInEx/config"
    cp -rn "$SCRIPT_DIR/lib/config/." "$GAME_DIR/BepInEx/config/" 2>/dev/null || cp -r "$SCRIPT_DIR/lib/config/." "$GAME_DIR/BepInEx/config/"
fi

# Deploy QMods to game folder
if [ -d "$SCRIPT_DIR/lib/QMods" ]; then
    echo "Deploying QMods to $GAME_DIR/QMods/ ..."
    mkdir -p "$GAME_DIR/QMods"
    cp -rn "$SCRIPT_DIR/lib/QMods/." "$GAME_DIR/QMods/" 2>/dev/null || cp -r "$SCRIPT_DIR/lib/QMods/." "$GAME_DIR/QMods/"
fi

# ---------------------------------------------------------------- 4. AppData
echo ""
echo "[4/5] Setting up AppData directories..."

# NOTE: C# Environment.SpecialFolder.ApplicationData resolves to ~/.config
# on Linux/Mono, while older docs mention ~/.botbenson. Create both layouts
# and symlink them together so either lookup hits the same files.
CONFIG_BASE="$HOME/.config/.botbenson/Subnautica Below Zero"
LEGACY_BASE="$HOME/.botbenson/Subnautica Below Zero"

for base in "$CONFIG_BASE" "$LEGACY_BASE"; do
    mkdir -p "$base/Game/Plugins" "$base/Game/Dependencies" \
             "$base/Game/Logs" "$base/Game/Saves" "$base/Game/Core"
    if [ -f "$SCRIPT_DIR/Data/SpawnPoints.bin" ]; then
        cp "$SCRIPT_DIR/Data/SpawnPoints.bin" "$base/Game/Core/" 2>/dev/null || true
    fi
done

# Keep the two trees in sync if they are not already the same location.
if [ ! -L "$LEGACY_BASE" ] && [ -d "$CONFIG_BASE" ] && [ -d "$LEGACY_BASE" ]; then
    if [ -z "$(ls -A "$LEGACY_BASE" 2>/dev/null)" ]; then
        rm -rf "$LEGACY_BASE"
        ln -s "$CONFIG_BASE" "$LEGACY_BASE"
        echo "Symlinked $LEGACY_BASE -> $CONFIG_BASE"
    fi
fi

# Also detect Proton Wine prefix AppData if available
PROTON_APPDATA=$(find "$HOME/.steam" -path "*compatdata/848450/pfx/drive_c/users/*/AppData/Roaming" 2>/dev/null | head -n 1 || true)
if [ -n "$PROTON_APPDATA" ] && [ -d "$PROTON_APPDATA" ]; then
    PROTON_BASE="$PROTON_APPDATA/.botbenson/Subnautica Below Zero"
    mkdir -p "$PROTON_BASE/Game/Plugins" "$PROTON_BASE/Game/Dependencies" \
             "$PROTON_BASE/Game/Logs" "$PROTON_BASE/Game/Saves" "$PROTON_BASE/Game/Core"
    if [ -f "$SCRIPT_DIR/Data/SpawnPoints.bin" ]; then
        cp "$SCRIPT_DIR/Data/SpawnPoints.bin" "$PROTON_BASE/Game/Core/"
    fi
    echo "Configured Proton prefix AppData at: $PROTON_BASE"
fi

# ---------------------------------------------------------------- 5. Custom Music
echo ""
echo "[5/5] Setting up Custom Music in Music/Unknown Worlds/Subnautica..."

MUSIC_DIRS=(
    "$HOME/Music/Unknown Worlds/Subnautica"
)

PROTON_MUSIC=$(find "$HOME/.steam" -path "*compatdata/848450/pfx/drive_c/users/*/Music" 2>/dev/null | head -n 1 || true)
if [ -n "$PROTON_MUSIC" ] && [ -d "$PROTON_MUSIC" ]; then
    MUSIC_DIRS+=("$PROTON_MUSIC/Unknown Worlds/Subnautica")
fi

MUSIC_SRC="${MUSIC_SOURCE_DIR:-}"
if [ -z "$MUSIC_SRC" ] && [ -d "$HOME/Music/Subnautica_Music_Compressed" ]; then
    MUSIC_SRC="$HOME/Music/Subnautica_Music_Compressed"
fi
if [ -z "$MUSIC_SRC" ] && [ -d "$HOME/Music/a.Flac/A..General" ]; then
    MUSIC_SRC="$HOME/Music/a.Flac/A..General"
fi
if [ -z "$MUSIC_SRC" ] && [ -d "$SCRIPT_DIR/Music_Compressed" ]; then
    MUSIC_SRC="$SCRIPT_DIR/Music_Compressed"
fi
if [ -z "$MUSIC_SRC" ] && [ -d "$SCRIPT_DIR/Music" ]; then
    MUSIC_SRC="$SCRIPT_DIR/Music"
fi

if [ -n "$MUSIC_SRC" ] && [ -d "$MUSIC_SRC" ]; then
    echo "Linking custom music from $MUSIC_SRC ..."
    for mdir in "${MUSIC_DIRS[@]}"; do
        mkdir -p "$mdir"
        for artist in Bad-Bunny C.R.O Feid; do
            if [ -d "$MUSIC_SRC/$artist" ]; then
                find "$MUSIC_SRC/$artist" -type f \( -name "*.flac" -o -name "*.mp3" -o -name "*.ogg" -o -name "*.wav" \) | while read -r trackfile; do
                    bname="$(basename "$trackfile")"
                    if [ ! -e "$mdir/$bname" ]; then
                        ln -s "$trackfile" "$mdir/$bname" 2>/dev/null || cp -n "$trackfile" "$mdir/$bname" 2>/dev/null || true
                    fi
                done
                echo "  Linked $artist tracks -> $mdir"
            fi
        done
    done
else
    echo "NOTE: Music source not found. Set MUSIC_SOURCE_DIR to auto-link your music on Linux."
fi

echo ""
echo "=========================================================="
echo " Installation Complete!"
echo "=========================================================="
echo ""
echo "IMPORTANT LINUX STEAM STEP:"
echo "In Steam -> Right click 'Subnautica: Below Zero' -> Properties"
echo "Set Launch Options to:"
echo ""
echo '    WINEDLLOVERRIDES="winhttp=n,b" %command%'
echo ""
echo "This allows Proton/Wine to load BepInEx (winhttp.dll) and the mod."
echo "You can now launch the game from Steam!"
echo ""
echo "First launch generates BepInEx config; if the menu mod does not appear,"
echo "verify BepInEx 5.x (not 6.x) was installed and the override is set."
