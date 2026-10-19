# Blackhole Framework

A modular Unity framework for composing games from reusable services and scene-based modules. Blackhole combines VContainer dependency injection, UniTask asynchronous workflows, MessagePipe messaging and editor tooling for module creation and startup validation.

This repository is the standalone Unity development project. Games install versioned `.unitypackage` releases into `Assets/BlackholeFramework` and track their imported copy in their own repository.

**Current version:** 0.3.0 · **Development editor:** Unity 6000.5.0f1

## Features

| Area | Included functionality |
| --- | --- |
| Module composition | ScriptableObject manifests, dependency resolution, installers and scoped service registration. |
| Runtime lifecycle | Persistent game roots, scene transitions, asynchronous root startup and cancellation tied to module scope. |
| Editor tooling | Module scaffolding, generated assembly definitions, root scenes, LifetimeScopes, startup entry points and configuration refresh. |
| Messaging | Typed `IEventPublisher<T>` and `IEventSubscriber<T>` contracts backed by MessagePipe. |
| Presentation and input | UI Toolkit components, loading screens, global dialogue, touch input and world interaction. |
| Game services | Audio playback, REST API clients, authentication, local file storage and encrypted saves. |
| Rendering | Composite Outline for projects configured with Universal RP. |
| Distribution | Deterministic framework-only exports, metadata/reference checks, checksums and an asset inventory. |

Version 0.3.0 introduces standalone in-process messaging and automatic root-module scaffolding. Experimental Networking, Unity Transport and game lesson/demo modules are excluded. See the [changelog](CHANGELOG.md) for changes and migration notes.

## Requirements

Use the pinned versions when opening this development project or preparing a receiving game:

| Dependency | Version / revision |
| --- | --- |
| Unity Editor | 6000.5.0f1 |
| UniTask | 2.5.11 |
| VContainer | 1.18.0 |
| MessagePipe and MessagePipe.VContainer | `3adc01deb1a283cb2faeb36c00178b84884a7fc3` |
| Universal RP | 17.5.0 |
| Input System | 1.19.0 |
| Unity UI | 2.5.0 |
| Unity Test Framework (development project) | 1.7.0 |

[Packages/manifest.json](Packages/manifest.json) pins the development dependencies. [Version.json](Assets/BlackholeFramework/Version.json) records the release version and consumer dependencies, including required built-in Unity modules. The [installation guide](Assets/BlackholeFramework/README.md#install-dependencies-first) contains the exact Git URLs for Package Manager.

Enable **Input System Package (New)** or **Both** under Player settings > Active Input Handling. Composite Outline requires URP configured as the project's render pipeline; the development project's render-pipeline settings are excluded from releases. Newtonsoft.Json 13.0.4 is bundled with its MIT license; avoid importing a conflicting second DLL.

## Install in a game

1. Install the dependencies above through Unity's Package Manager.
2. Download a versioned `.unitypackage` from [Releases](https://github.com/bigbangwill/blackhole-framework/releases) and import it using **Assets > Import Package > Custom Package**.
3. Open **Assets > Create > Blackhole > Modules > Module Scaffold**. Choose a game-owned folder such as `Assets/Game/Modules`, enable **Root Module**, and enable **Persistent Root** for the game starting module.
4. Wait for compilation and automatic scene wiring to finish. The scaffold creates the manifest, installer, scene, LifetimeScope and registered `RootModuleStartup` entry point. Assign the module's required dependencies in its manifest.
5. Select `Assets/BlackholeFramework/Framework/BlackholeProjectSettings.asset` and assign **Game Starting Module** to that manifest. The manifest supplies its starting scene.
6. Configure your audio clips, server URLs and project-specific encryption key as needed. Release defaults contain no game audio, saved login session or backend URLs. Preserve your encryption key across upgrades unless saved data is migrated.
7. Run **Blackhole > Framework > Refresh Module Configuration** in Edit Mode after compilation. This refreshes the manifest catalog and puts the framework scene first and the starting module scene second in Build Settings.
8. Open `Assets/BlackholeFramework/Framework/BlackholeFrameworkScene.unity` and enter Play Mode.

See the [framework installation guide](Assets/BlackholeFramework/README.md) for configuration and asset ownership details. Use release packages for installation; avoid keeping the old UPM/Git copy alongside the imported framework.

## Module workflow

A module manifest identifies the module, its installer, optional scene and dependencies. Installers register services and asset references with VContainer and must remain stateless. Modules can use separate Contract, Domain, Application, Infrastructure, Presentation and Composition assemblies; choose the layers needed in the scaffolder.

The framework starts the configured persistent game root. That root can load other game roots beneath it through `IModuleLoader.LoadRootModuleAsync`. Put asynchronous startup work in the generated startup class by overriding `RootModuleStartup.InitializeAsync(CancellationToken)`. The loader waits for startup after loading the module's scenes and activating its root scene. Other game roots can remain non-persistent.

### In-process messaging

Add `Blackhole.Messaging` to the dependencies of modules that publish or subscribe to events, and reference `Blackhole.Messaging.Contract` from assemblies using its interfaces. Inject `IEventPublisher<T>` to call `Publish(message)` or `IEventSubscriber<T>` to call `Subscribe(handler)`.

`Subscribe` returns an `IDisposable`; dispose it when its owning service or presenter stops. Messaging runs inside the current process and does not require a network connection or Unity Transport. `MessagingSettings` is an empty extension point in this version.

## Develop locally

1. Clone or download this repository and open its root with Unity **6000.5.0f1**. Allow Package Manager to resolve dependencies and wait for compilation to finish.
2. Run **Blackhole > Development > Configure Smoke Project** to rebuild the development setup if needed. This assigns the persistent smoke root, refreshes configuration and runs `FrameworkStartupValidator`.
3. Open `Assets/BlackholeFramework/Framework/BlackholeFrameworkScene.unity` and enter Play Mode. The smoke module exercises startup, module loading and audio dependency wiring without game clips or a backend login.
4. Use **Window > General > Test Runner** to run EditMode and PlayMode tests before releasing framework changes.

The dependency installer does nothing automatically when the project opens. For batch-mode bootstrap or repair, its entry point is `Blackhole.Development.Editor.PackageInstaller.Install`.

### Repository layout

| Path | Purpose | Exported |
| --- | --- | --- |
| `Assets/BlackholeFramework` | Reusable code, manifests, installers, settings, scenes, UI and third-party notices. | Yes |
| `Assets/FrameworkTests` | Smoke module, EditMode/PlayMode tests and development verification tooling. | No |
| `Assets/Editor/ProjectBootstrap` | Explicit dependency installation helper. | No |
| `Assets/Settings`, `Assets/Scenes` | Development URP configuration and template scene. | No |
| `Packages`, `ProjectSettings` | Development dependencies and Unity project settings. | No |
| `scripts` | Deterministic release exporter and Python tests. | No |
| `.github/workflows` | Packaging validation and release automation. | No |

Keep reusable changes under `Assets/BlackholeFramework` and preserve existing `.meta` GUIDs. Keep experiments and game-specific examples under `Assets/FrameworkTests`. Generated folders such as `Library`, `Temp`, `Logs`, `UserSettings`, `TestResults` and `dist` are ignored.

### Validation

- **EditMode:** framework configuration and `FrameworkStartupValidator`, configuration preservation, messaging and root-module scaffolding.
- **PlayMode:** persistent root startup, scene loading and active-scene selection.
- **Python:** exporter metadata, reference, configuration and archive checks.

Run the exporter tests from the repository root:

```powershell
python -m unittest discover -s scripts -p "test_*.py" -v
```

`FrameworkStartupValidator` also runs before player builds and rejects invalid startup configuration. A clean imported release has no game starting module; configure one before expecting startup validation to pass.

## Upgrade an existing game

1. Commit or back up the game before importing a newer release and read the [migration notes](CHANGELOG.md).
2. Review the import selection and preserve configured settings, installers, scenes, `.meta` files and any customized framework files.
3. Review the resulting diff and remove obsolete files identified by the changelog. Unity package imports do not merge edits or delete obsolete files.
4. Run **Blackhole > Framework > Refresh Module Configuration**, then compile and test the game's startup and affected flows.

**Release configuration is intentionally reset to reusable defaults.** Replacing those assets blindly can erase the game's starting module, sound library, session, server URLs or encryption key. For 0.3.0, install both pinned MessagePipe packages before importing the update.

## Build and publish a release

1. Update [Version.json](Assets/BlackholeFramework/Version.json) and [CHANGELOG.md](CHANGELOG.md) together. Update the [exported changelog](Assets/BlackholeFramework/CHANGELOG.md) with consumer migration notes.
2. Run Unity EditMode and PlayMode tests, verify `FrameworkStartupValidator`, then run the Python tests above.
3. Export the framework and verify its import into a receiving Unity project:

```powershell
python scripts/export_unitypackage.py --tag v0.3.0 --output dist
```

4. Commit and push the tested source changes to `main`, then tag the release commit:

```powershell
git tag -a v0.3.0 -m "Blackhole Framework 0.3.0"
git push origin v0.3.0
```

The commands illustrate version 0.3.0; substitute the new version before releasing. The tag must equal `v` plus the version in `Version.json`, including any prerelease suffix. Never move or reuse a published tag, and keep generated release files out of Git.

The [validation workflow](.github/workflows/validate.yml) runs exporter tests and packages the current version on pushes to `main` and pull requests. The [release workflow](.github/workflows/release.yml) runs on `v[0-9]*` tag pushes and publishes:

- `BlackholeFramework-<version>.unitypackage`
- `SHA256SUMS.txt`
- `package-inventory.json`, listing exported assets and required dependencies

Prerelease versions such as `0.3.0-preview.1` produce prereleases when Version.json and the tag match. Manual workflow dispatch from a branch creates a downloadable Actions artifact without publishing a release. The workflow uses GitHub's built-in token; no personal token or Unity license secret is required.

**Hosted workflows validate packaging; they do not compile Unity code or run EditMode/PlayMode tests.** Run those checks and local import verification before tagging. Exports preserve Unity asset GUIDs and metadata; preview thumbnails and Unity 6.6 package signatures are not added.

### Exported defaults

The exporter includes only `Assets/BlackholeFramework` and normalizes configuration inside the archive without modifying source settings:

- The starting game module is unassigned, and development manifests are removed from the catalog.
- The sound library is empty; the reusable audio mixer remains included.
- Saved authentication tokens and project backend URLs are blank.
- The encryption key is `configure-a-project-specific-key`.

Unknown serialized references, credential-like values, obsolete package paths, duplicate GUIDs, orphan metadata and missing metadata fail the export. Keep game-owned configuration and secrets out of the reusable folder.

## Documentation

- [Installation, dependencies and configuration](Assets/BlackholeFramework/README.md)
- [Release history and migration notes](CHANGELOG.md)
- [Composite Outline](Assets/BlackholeFramework/CompositeOutline/README.md)
- [UI Toolkit Outline Filter](Assets/BlackholeFramework/UIToolkitComponent/OutlineFilter/README.md)
- [Third-party notices](Assets/BlackholeFramework/Third%20Party%20Notices.md)
- [Development guidelines](AGENTS.md)
