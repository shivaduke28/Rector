# CLAUDE.md

## Project Overview

Rector is a Unity-based audio-reactive visual effects application with a node-based graph system for creating interactive audiovisual experiences.
Built with Unity 6000.0.42f1 and URP 17.0.4.

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
  - Check compilation errors through Unity's asset refresh

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

### Error Checking and Diagnostics

#### IDE Diagnostics vs Unity Compilation
Two complementary error checking methods are available:

1. **IDE Diagnostics** (`mcp__ide__getDiagnostics`)
   - Real-time feedback while coding
   - Detects syntax errors, code quality issues, potential bugs
   - Catches YAML/JSON syntax errors in config files
   - Use for: Daily coding, before committing changes

2. **Unity Compilation** (`mcp__unity-natural-mcp__get_compile_logs`)
   - Final compilation check by Unity
   - Detects Unity-specific issues (MonoBehaviour, serialization, etc.)
   - Required before running in Unity Editor
   - Use for: After major changes, before testing in Unity

#### Recommended Workflow
1. Check IDE diagnostics while coding (especially for non-C# files like YAML)
2. After code changes, refresh Unity assets: `mcp__unity-natural-mcp__refresh_assets`
3. Check Unity compilation: `mcp__unity-natural-mcp__get_compile_logs`
4. Fix any errors before proceeding

#### Example Usage
```bash
# Check IDE diagnostics for a specific file
mcp__ide__getDiagnostics --uri "file:///path/to/file.cs"

# Check Unity compilation after changes
mcp__unity-natural-mcp__refresh_assets
mcp__unity-natural-mcp__get_compile_logs
```
