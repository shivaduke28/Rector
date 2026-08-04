# CLAUDE.md

## Project Overview

Rector is a Unity-based audio-reactive visual effects application with a node-based graph system for creating interactive audiovisual experiences.
Built with Unity 6000.3.21f1 (6.3 LTS) and URP 17.3.0.

The main codebase is located at `/Assets/Rector/Scripts/`.

## Architecture

### Core Systems

1. **Node Graph System** - Visual programming with nodes, slots, and edges
   - Codebase: `/Assets/Rector/Scripts/UI/Graphs/`

2. **Audio Processing** - Real-time audio input analysis
   - Main entry: `AudioInputStream` in `/Assets/Rector/Scripts/Audio/`
   - Beat detection, threshold analysis, frequency spectrum processing
   - Uses LASP (Low-latency Audio Signal Processing)

3. **VFX System** - Audio-driven visual effects
   - Managed by `VfxManager` in `/Assets/Rector/Scripts/Vfx/`
   - Uses Unity's Visual Effect Graph

4. **Camera System** - Dynamic camera behaviors
   - `CameraManager` handles camera behavior switching
   - Uses Cinemachine for camera control
   - Camera behaviors implemented in `/Assets/Rector/Scripts/Cameras/`

5. **UI System** - Custom HUD and node editor
   - HudModel/HudView pattern in `/Assets/Rector/Scripts/UI/`
   - Graph page UI for node editing

### Dependency Management

- **RectorInstaller** (`/Assets/Rector/Scripts/RectorInstaller.cs`) - Main composition root
- Uses custom dependency injection pattern
- All major systems are initialized here

### Key Technologies

- **UniTask** - Asynchronous operations
- **R3** - Reactive Extensions for Unity
- **LASP** - Audio processing
- **Cinemachine** - Camera management
- **Visual Effect Graph** - Particle effects

### Project Structure

```
/Assets/Rector/
├── Scripts/          # All C# source code
├── Shaders/          # HLSL shaders and subgraphs
├── Settings/         # URP settings, post-process volumes
├── StaticResources/  # Materials, models, textures
└── Prefabs/          # Reusable game objects
```

### Development Rules
- After work, format with `dotnet format Rector.csproj`
  - Use `--include` to specify files as needed
- Add `sealed` to classes if possible
- Use C# modern syntax:
  - Prefer get-only auto-properties over readonly fields with properties
  - Use auto-properties wherever possible to reduce boilerplate code
- When a file exceeds 300 lines, consider splitting related classes into separate files
- Unity-specific constraints:
  - Cannot use `dotnet build` command
  - Check compilation errors with `unity command recompile` — see the Unity CLI
    section below

### Async/Await Guidelines
- **Never use `async void`** - Use `UniTaskVoid` instead
  - Exception: Unity event handlers that must be `async void`
- **Always append `Async` suffix** to async methods
  - Example: `ProcessDataAsync()`, `HandleInputAsync()`
- **Always pass `CancellationToken`** to async methods
  - Pass it as the last parameter
  - Use it in all async operations (e.g., `UniTask.Delay()`, `UniTask.NextFrame()`)
- **Use `.Forget()` for fire-and-forget operations**
  - Example: `HandleAsync(cancellationToken).Forget()`
- **Properly manage CancellationTokenSource**
  - Create in the scope that controls the lifetime
  - Dispose when no longer needed
  - Cancel before disposing

## Unity CLI

The Unity CLI (`brew install --cask unity-cli`) drives the editor from the
terminal. `com.unity.pipeline` exposes a local HTTP API that the CLI talks to,
so a running editor can be queried and controlled without touching its window.

The editor version is resolved from `ProjectVersion.txt`, so no command needs
it spelled out. `make` lists the wrapped targets; the rest is used directly.

Every command takes `--json` for structured output. Exit codes: 0 success,
1 error, 6 test failure.

### Checking your work

**`unity command recompile` works while the editor is unfocused or minimized.**
Bringing Unity to the foreground is not required to compile-check a change.

```bash
unity command recompile              # force a script recompile
unity command recompile_status       # idle | triggered | compiling | completed | up_to_date
unity command console --level error --tail 20
unity command console --tail 50      # all levels, with stack traces
make test                            # EditMode tests -> test-results.xml
```

`console` reads the Player's output too, not just the editor's, and takes a
cursor via `--since` to follow along.

### Recommended workflow

1. While coding, check IDE diagnostics (`mcp__ide__getDiagnostics`) — it also
   catches YAML/JSON syntax errors that Unity never sees
2. `unity command recompile`, then poll `unity command recompile_status`
3. `unity command console --level error` — fix what it reports
4. `dotnet format Rector.csproj --include <changed files>`

### Inspecting and driving the project

`unity command` with no argument lists all 140 built-in commands. Destructive
ones require `--confirm true` and accept `--dry_run true`.

```bash
unity status                         # connected editors: port, project, state, PID
unity command search --query "t:VisualEffectAsset" --limit 20
unity command list_open_scenes
unity command get_performance_stats
unity command editor_play            # also editor_pause / editor_stop
unity command package_add --identifier <name@version> --confirm true --wait true
unity command package_remove --name <name> --confirm true --wait true
```

`unity command eval` runs C# inside the live editor through Roslyn — no project
recompile, no domain reload. Use it for one-off inspection; promote anything
repeated to a `[CliCommand]` method so it lives in source control and can be
reviewed once instead of per-call.

```bash
unity command eval 'return UnityEditor.AssetDatabase.FindAssets("t:VisualEffectAsset").Length;'
```

### Driving a build

`unity command --runtime <player exec name>` connects to a running Player the
same way it connects to the editor, so a development build of Rector can be
inspected and hot-reloaded while it is actually running against live audio.
The runtime API is localhost-only and off by default — development and QA
builds only, never a shipping build.

### Caveats

- `com.unity.pipeline` is experimental (`0.4.0-exp.1`); its command surface may
  change between versions
- Modal dialogs in the editor block the API — anything waiting on one hangs
- Editing an asset on disk while the editor holds it in memory gets overwritten
  on the editor's next save. Change those through the CLI or the Inspector
