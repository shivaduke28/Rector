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

### Testing Rules

**Rectorの境界は「プロセス外の何を、どう掴んでいるか」で決まる。**
Presentation / Domain のような一般的な層分けはこのプロジェクトには当てはまらない。
ほとんどが Presentation であり、グラフの見え方に本質があるからだ。
テストを書くかどうかは、クラスが外の世界とどう繋がっているかで決める。

| | 掴み方 | 方針 | 例 |
|---|---|---|---|
| **A** | 外部を掴まない | EditModeテストを書く | `ISettingRow` 実装, `NodeNavigator`, `GraphSaveData`, `NodeTemplateId`, `MidiPortName`, `LayerOrderAssigner` |
| **B** | 外部を掴むが、宛先をコンストラクタで受け取れる（ディレクトリ、ポート、prefsキー） | 偽の宛先を渡してEditModeテストを書く | `GraphSlotRepository`（`new GraphSlotRepository(directory)`） |
| **C1** | Unityのランタイムが要る（UXMLツリー、プレイループ、レイアウト解決、ループバック通信） | PlayModeテスト、またはCLI/実機で確認する | `GraphPage`, `GraphSerializer`, `LayeredGraph`, `*View`, `OscModel` |
| **C2** | 本物のデバイスが要る | テストを書かない。実機で確認する | `MidiInputDeviceManager`, `AudioInputStream`, `CameraManager` |

- **Cに落ちたら、まず設計を疑う。** `GraphSerializer` がCなのは `GraphPage` を丸ごと
  受け取っているからで、「Unityだからテストできない」のではなく「握りすぎ」のサイン。
  Bへ動かせないか（宛先や協調相手をコンストラクタで受け取れないか）を先に考える。
- **「テストが書けない」で止めない。** EditModeで書けないものはPlayModeテストで
  書けることが多い。書かないと決めたなら、なぜ書かないのかをコメントかissueに残す。

テストの書き方:

- テストは**振る舞い**に対して書く。振る舞いとはクラスの責務を外から見たときの
  期待値であって、メソッド名ではない。
- **テスト名の主語はクラス・役割にする。メソッド名を主語にしない。**
  良い: `Selector_CommitsOnlyOnSubmitInsideMenu`（Selectorが主語、述部が振る舞いの文）
  悪い: `PickInDirection_Output_ReturnsFirstOutput`（メソッドが主語）
- **テストを書くためだけにクラスや関数を切り出さない。** 切り出すなら、その境界が
  A か B として説明できること。説明できないなら元の場所に置いたままにする。
- 挙動を変えないリファクタで落ちるテストは書き直すか消す。**契約でない文字列を
  アサーションに焼き込まない**（ログ文言、表示フォーマット、PlayerPrefsのキー）。
- 同じ境界の仲間には揃ってテストがある状態を保つ（`ISettingRow` の実装なら全部）。

### Comment Rules

- 書くのは**コードを読んでもわからないこと**。サードパーティの仕様・罠、
  一見不要に見える処理が必要な理由、選ばなかった選択肢とその理由。
- **自分が直接依存していない他クラスの内部について書かない。** そのクラスが
  変わってもコンパイラは何も言わないので確実に腐る。同じ理由を2箇所に書かない。
- コードを読めばわかることは書かない。セクションラベル（`// logger` の直下に
  `RectorLogger...`）や `// 初期化` の類は消す。
- TODO/FIXME を残すならissue番号を添える。添えられないなら消す。
- 模範例:
  - `Osc/OscModel.cs` — ABBAデッドロックの回避理由、受信スレッドの制約
  - `Editor/BuildSceneSetupRestorer.cs` — パッケージのバグと、回避をここに置いた理由
  - `UI/Graphs/Serialization/GraphSaveData.cs` — JsonUtility が空のキーを無視する罠
  - `UI/GraphPages/InputGuideView.cs` — フォントにグリフが無いので別のコードポイントを使う

### Structural Conventions

守られていて効いているもの。崩さないこと。

- **名前空間 = フォルダ。例外なし。**
- **`UI/` の外から `UI/` に依存しない。** `Audio`/`Midi`/`Osc`/`Cameras`/`Vfx` は
  上を見ない。共有したい語彙（`NodeCategory` など）はルート名前空間 `Rector` に置く。
- **入力ソースの型**（Audio/Midi/Osc で3回反復されている）:
  `XxxInputDeviceManager` がデバイスハンドルと `PlayerPrefs` を持ち、
  `XxxModel` が `IInitializable, IDisposable` でR3ストリームを公開し、
  UIは Model しか見ない。次の入力ソースもこの型に従う。
- **`IInitializable` + `RectorInstaller.Register<T>`** で寿命を揃える。
  ただし `LayeredGraph` には実行時に足したノード用の別のディスパッチャがある
  （同じインターフェースだが別のライフサイクル）。
- ノードは `public const string NodeName` + `static GetCategory()` を持ち、
  `NodeTemplateRegisterer` の登録行が導出できる形にする。

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

`unity command --runtime RectorApp` connects to a running Player, so a build
processing live audio can be inspected and driven exactly like the editor.
Placement of `--runtime` does not matter — commander.js claims it wherever it
appears — but put it before the command name for readability. When no matching
Player is found the call fails; it never silently answers from the editor.

Watch the shell instead: **zsh does not word-split unquoted parameters**, so
`FLAGS="--runtime RectorApp"; unity command $FLAGS rector_state` passes one
argument `--runtime RectorApp`, which is not recognised and the call goes to
the editor. Write the flags out, or use an array.

`Base.unity` carries a `RuntimePipelineManager` with `enableInBuilds` on, which
starts the server in a Player. The listener binds `http://+:<port>/` — every
interface — and rejects non-loopback callers with 403 in the request handler,
ahead of the bearer-token check. The token lives in the runtime descriptor,
which on macOS is written **inside the .app bundle** (`Application.dataPath/..`),
not beside it.

Only the Roslyn compilers sit behind
`#if UNITY_EDITOR || (UNITY_STANDALONE && DEBUG)` — `EvalCodeCompiler`,
`HotReloadCompiler`, `RoslynCompilationService`. The `eval` and hot-reload
*commands* stay registered in a release build and return "Platform Not
Supported". Rector is IL2CPP, so they cannot work in any build regardless.
Nothing gates the server itself: `RuntimePipelineManager.Start()` checks only
`autoStart && enableInBuilds`, and the build processor never consults
`EditorUserBuildSettings.development`. A release build from this scene opens
the port, and `quit` / `set_timescale` / `simulate_key` come with it.

**`enableInBuilds` is deliberately left on** — every build, release included,
runs the server. Rector is a personal instrument rather than distributed
software, and being able to drive any build is worth more here than closing a
port that only processes running as the same user can reach. Reviewers have
flagged this; it is a decision, not an oversight. Revisit it if Rector is ever
handed to someone else, and note the exposure is the whole runtime command
surface, not just `rector_*`.

### Caveats

- `com.unity.pipeline` is experimental (`0.4.0-exp.1`); its command surface may
  change between versions
- Modal dialogs in the editor block the API — anything waiting on one hangs
- Editing an asset on disk while the editor holds it in memory gets overwritten
  on the editor's next save. Change those through the CLI or the Inspector
