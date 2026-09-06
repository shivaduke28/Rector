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

`unity command` with no argument lists every command, `rector_*` included.
Destructive ones require `--confirm true` and accept `--dry_run true`.

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

`unity command --runtime Rector` connects to a running Player, so a build
processing live audio can be inspected and driven exactly like the editor.
Placement of `--runtime` does not matter — commander.js claims it wherever it
appears — but put it before the command name for readability. When no matching
Player is found the call fails; it never silently answers from the editor.

Watch the shell instead: **zsh does not word-split unquoted parameters**, so
`FLAGS="--runtime Rector"; unity command $FLAGS rector_state` passes one
argument `--runtime Rector`, which is not recognised and the call goes to
the editor. Write the flags out, or use an array.

No scene component is involved (the `RuntimePipelineManager` GameObject that
`Base.unity` used to carry is gone since package 0.6.0). The runtime server is
configured in `ProjectSettings/Packages/com.unity.pipeline/RuntimePipelineConfig.json`
(Project Settings → Pipeline → Runtime, or `set_runtime_pipeline_settings
--settings '{"enableInBuilds": true}' --confirm true`). At build time the
package bakes that JSON into a transient asset under
`Assets/Settings/Pipeline/Resources/` and deletes it afterwards — those paths
are gitignored; a leftover from an interrupted build is safe to delete. At boot
`RuntimePipelineBootstrap` creates the driver, which starts the server when
`autoStart && enableInBuilds`. The listener binds `http://+:<port>/` — every
interface — and rejects non-loopback callers with 403 in the request handler,
ahead of the bearer-token check. The token lives in the runtime descriptor,
which on macOS is written **inside the .app bundle** (`Application.dataPath/..`),
not beside it.

The server itself is not gated on Development Build; only hot reload (the
PlayerConnection receiver and its overlay) sits behind `DEVELOPMENT_BUILD`, and
the Roslyn compilers behind `#if UNITY_EDITOR || (UNITY_STANDALONE && DEBUG)`.
The `eval` and hot-reload *commands* stay registered in a release build and
return "Platform Not Supported". Rector is IL2CPP, so they cannot work in any
build regardless. A release build opens the port, and `quit` / `set_timescale`
/ `simulate_key` come with it.

**The build processor does consult `EditorUserBuildSettings.development`.**
With `enableInBuilds` on and Development Build off it logs a SECURITY RISK
warning, and in a GUI editor (`!Application.isBatchMode`) it also opens a modal
dialog — Continue / Cancel / Disable Pipeline — which blocks the API. So a
release build driven through `eval` from the running editor hangs there, and
"Disable Pipeline" silently flips `enableInBuilds` back to `false` in the JSON.
Build as a Development Build, or build headless, when driving from the CLI.

**`enableInBuilds` is deliberately left on** — every build, release included,
runs the server. Rector is a personal instrument rather than distributed
software, and being able to drive any build is worth more here than closing a
port that only processes running as the same user can reach. Reviewers have
flagged this; it is a decision, not an oversight. Revisit it if Rector is ever
handed to someone else, and note the exposure is the whole runtime command
surface, not just `rector_*`.

### Caveats

- `com.unity.pipeline` is experimental (`0.6.0-exp.1`); its command surface may
  change between versions
- The CLI and the package move together. After `brew upgrade unity-cli`, an
  editor still on an older package rejects every command with "too old to
  parse command lines". Run `unity pipeline upgrade`; it edits
  `Packages/manifest.json` and `packages-lock.json`, so commit that separately
- `capture_game_view` / `capture_scene_view` only write `--save_path` inside
  the project root; a scratchpad path is rejected
- Modal dialogs in the editor block the API — anything waiting on one hangs
- Editing an asset on disk while the editor holds it in memory gets overwritten
  on the editor's next save. Change those through the CLI or the Inspector
