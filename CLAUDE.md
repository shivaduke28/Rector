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

### Compilation Error Checking

To check for compilation errors after making changes:
1. Refresh Unity assets: `mcp__unity-natural-mcp__refresh_assets`
2. Get compilation errors: `mcp__unity-natural-mcp__get_compile_logs`

This will show any compile errors or warnings in the Unity project.
