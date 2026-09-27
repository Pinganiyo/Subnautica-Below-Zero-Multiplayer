@echo off
setlocal enabledelayedexpansion

echo ==========================================================
echo  Subnautica Below Zero Multiplayer - Windows Installer
echo ==========================================================
echo.
echo Options: set SUBNAUTICA_DIR to override the game path,
echo          set PREBUILT_DIR to deploy DLLs from a custom folder.
echo BepInEx 5.x (5.4.23.2 x64) must be extracted into lib\BepInEx\.
echo.

REM ---- dotnet on PATH (user-local install fallback) ----
if exist "%LocalAppData%\Microsoft\dotnet\dotnet.exe" (
    set "PATH=%LocalAppData%\Microsoft\dotnet;%PATH%"
)
where dotnet >nul 2>&1
if errorlevel 1 (
    echo Error: 'dotnet' not found. Install the .NET 8 SDK and re-run.
    echo https://dotnet.microsoft.com/download
    exit /b 1
)

REM ---- game directory (SUBNAUTICA_DIR wins, else common Steam locations) ----
REM NOTE: single-line IFs on purpose - "Program Files (x86)" inside a
REM parenthesised block can break batch parsing because of the ")" in the name.
set "GAME_DIR=%SUBNAUTICA_DIR%"
if "%GAME_DIR%"=="" if exist "D:\SteamLibrary\steamapps\common\SubnauticaZero" set "GAME_DIR=D:\SteamLibrary\steamapps\common\SubnauticaZero"
if "%GAME_DIR%"=="" if exist "C:\Program Files (x86)\Steam\steamapps\common\SubnauticaZero" set "GAME_DIR=C:\Program Files (x86)\Steam\steamapps\common\SubnauticaZero"
if "%GAME_DIR%"=="" if exist "E:\SteamLibrary\steamapps\common\SubnauticaZero" set "GAME_DIR=E:\SteamLibrary\steamapps\common\SubnauticaZero"
if "%GAME_DIR%"=="" if exist "%ProgramFiles(x86)%\Steam\steamapps\common\SubnauticaZero" set "GAME_DIR=%ProgramFiles(x86)%\Steam\steamapps\common\SubnauticaZero"

if not exist "%GAME_DIR%" (
    if not "%GAME_DIR%"=="" echo Not found: "%GAME_DIR%"
    set "GAME_DIR="
    set /p "GAME_DIR=Please enter the path to your SubnauticaZero game folder: "
    REM Strip surrounding quotes if the user entered them
    set "GAME_DIR=!GAME_DIR:"=!"
)
if not exist "%GAME_DIR%" (
    echo Error: Directory "%GAME_DIR%" does not exist.
    exit /b 1
)
echo Game directory: "%GAME_DIR%"

echo.
echo [1/4] Building mod in Release mode...

REM Upstream is source-only (no .sln/.csproj): build only if project files exist,
REM otherwise deploy prebuilt DLLs from .\build\ or %PREBUILT_DIR%.
set "BUILD_TARGET="
for %%F in ("%~dp0*.sln") do if exist "%%F" if not defined BUILD_TARGET set "BUILD_TARGET=%%F"
if not defined BUILD_TARGET for %%F in ("%~dp0*.csproj") do if exist "%%F" if not defined BUILD_TARGET set "BUILD_TARGET=%%F"

if not defined BUILD_TARGET (
    echo No .sln/.csproj found - skipping compile.
    echo Put prebuilt *.dll files in "%~dp0build\" or set PREBUILT_DIR.
) else (
    echo Building "!BUILD_TARGET!" ...
    dotnet build "!BUILD_TARGET!" -c Release -p:GameManagedPath="%GAME_DIR%\SubnauticaZero_Data\Managed"
    if !ERRORLEVEL! neq 0 (
        echo Build failed!
        exit /b !ERRORLEVEL!
    )
)

REM Locate the assemblies to deploy (auto-detect, don't hardcode net472).
set "BUILD_DIR="
if exist "%~dp0build\*.dll" set "BUILD_DIR=%~dp0build"
if not defined BUILD_DIR if exist "%~dp0bin\Release\net472\*.dll" set "BUILD_DIR=%~dp0bin\Release\net472"
if not defined BUILD_DIR if exist "%~dp0bin\Release\net48\*.dll" set "BUILD_DIR=%~dp0bin\Release\net48"
if not defined BUILD_DIR if not "%PREBUILT_DIR%"=="" set "BUILD_DIR=%PREBUILT_DIR%"

echo.
echo [2/4] Installing BepInEx 5 to game folder...
if not exist "%~dp0lib\BepInEx\winhttp.dll" (
    echo Error: BepInEx 5 not found in "%~dp0lib\BepInEx\".
    echo Download BepInEx_x64_5.4.23.2.zip from
    echo   https://github.com/BepInEx/BepInEx/releases
    echo extract it into "%~dp0lib\BepInEx\" and re-run.
    exit /b 1
)
xcopy /E /I /Y "%~dp0lib\BepInEx\*" "%GAME_DIR%\"
if errorlevel 1 (
    echo BepInEx copy failed!
    exit /b 1
)

echo.
echo [3/4] Deploying multiplayer mod assemblies...
set "PLUGIN_DIR=%GAME_DIR%\BepInEx\plugins\SubnauticaMultiplayer"
if exist "%PLUGIN_DIR%" if not exist "%PLUGIN_DIR%\*" del /F /Q "%PLUGIN_DIR%"
if not exist "%PLUGIN_DIR%" mkdir "%PLUGIN_DIR%"

if not defined BUILD_DIR (
    echo WARNING: no compiled assemblies found - plugin folder created, nothing deployed.
    echo Build with project files present, or drop the DLLs into "%~dp0build\".
) else (
    echo Deploying from "!BUILD_DIR!" ...
    copy /Y "!BUILD_DIR!\*.dll" "%PLUGIN_DIR%\"
    if exist "!BUILD_DIR!\*.pdb" copy /Y "!BUILD_DIR!\*.pdb" "%PLUGIN_DIR%\" >nul
    REM Remove build-time and dummy stubs to allow Unity/Mono native Reflection.Emit
    del /Q /F "%PLUGIN_DIR%\AsmResolver*.dll" >nul 2>&1
    del /Q /F "%PLUGIN_DIR%\BepInEx.AssemblyPublicizer*.dll" >nul 2>&1
    del /Q /F "%PLUGIN_DIR%\Microsoft.NET.StringTools*.dll" >nul 2>&1
    del /Q /F "%PLUGIN_DIR%\System.Reflection.Emit*.dll" >nul 2>&1
)

if exist "%~dp0Data\SpawnPoints.bin" (
    if not exist "%PLUGIN_DIR%\Data" mkdir "%PLUGIN_DIR%\Data"
    xcopy /E /I /Y "%~dp0Data\*" "%PLUGIN_DIR%\Data\"
)

if exist "%GAME_DIR%\BepInEx\plugins\RadialTabs" (
    echo Removing disabled RadialTabs...
    rmdir /S /Q "%GAME_DIR%\BepInEx\plugins\RadialTabs" >nul 2>&1
)

if exist "%GAME_DIR%\BepInEx\plugins\CurtainsandBlinds" (
    echo Removing disabled CurtainsandBlinds...
    rmdir /S /Q "%GAME_DIR%\BepInEx\plugins\CurtainsandBlinds" >nul 2>&1
)

if exist "%~dp0lib\plugins" (
    echo.
    echo Deploying bundled mod collection to BepInEx\plugins...
    xcopy /E /I /Y "%~dp0lib\plugins\*" "%GAME_DIR%\BepInEx\plugins\"
)

if exist "%~dp0lib\QMods" (
    echo.
    echo Deploying QMods to game folder...
    if not exist "%GAME_DIR%\QMods" mkdir "%GAME_DIR%\QMods"
    xcopy /E /I /Y "%~dp0lib\QMods\*" "%GAME_DIR%\QMods\"
)

echo.
echo [4/4] Setting up AppData directories...
set "BOTBENSON=%APPDATA%\.botbenson\Subnautica Below Zero\Game"
for %%D in (Plugins Dependencies Logs Saves) do (
    if not exist "!BOTBENSON!\%%D" mkdir "!BOTBENSON!\%%D"
)

echo.
echo ==========================================================
echo  Installation Complete!
echo ==========================================================
echo You can now launch Subnautica: Below Zero from Steam or your desktop.
pause
