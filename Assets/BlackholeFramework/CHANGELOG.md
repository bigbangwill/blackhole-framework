# Changelog

## 0.8.0 - 2026-11-11

- Expose pointer buttons and route WorldInteraction hits by renderer priority, with pass-through targets, deselection and click suppression after drags.
- Attach the drag surface to the active camera and restore its original parent on deactivation. Camera dragging uses left mouse and touch input.
- Add spinner-backed async loading transitions and use them in generated GameFlow navigation.
- Add audio pause and resume, preserve playback on repeated requests for the same music clip and synchronize slider changes with mute settings.
- Generate GameFlow into `.g.cs` files with a separate Unity installer declaration. Existing generated scripts retain their GUIDs and both navigation interfaces.

### Migration

Select **Receive Alongside Target** on custom camera drag surfaces so they receive input behind selectable objects. Enable **Pass Through** on other targets that should let input reach lower-priority targets. Use `PauseAll` and `ResumeAll` to retain audio playback position. Regenerate GameFlow from **Blackhole > Modules > Inspect Root Modules** to update the generated files.

## 0.7.3 - 2026-11-07

- Load scenes from the framework dependency graph additively under the framework scope before starting the first game root. GameFlow navigation waits for framework scene startup and leaves framework dependency scenes loaded when switching roots.

## 0.7.2 - 2026-11-06

- Select platform manifest installers from each Editor instance's Multiplayer Role so Multiplayer Play Mode can run a server and clients without player builds. Client and Server together uses the client installer; dedicated server players retain UNITY_SERVER selection.
- Include Dedicated Server 2.0.2 in framework dependencies for Multiplayer Roles support.

## 0.7.1 - 2026-11-06

- Exclude Settings editor tools from player compilation.
- Compile only the selected platform manifest installer field into each player, keeping both authoring references available in the Editor.
- Add full-build isolation verification tooling under FrameworkTests; verification probes require an explicit build-only define and are excluded from releases.

## 0.7.0 - 2026-11-06

- Import the Scenes browser with search, scene status, single/additive opening, unloading and Add Tab registration from Adan-Game's CountryRoot/LandRoot branches.
- Add class/interface creation templates and a root-module inspector with configurable GameFlow source generation into game-owned modules.
- Mark newly scaffolded root manifests and preserve both base and concrete entry-options registrations across root-module navigation.
- Port nested save paths, typed storage errors, encryption failure propagation and slash-safe REST URL joining from Adan-Game GIS.
- Keep current client/server networking assembly separation and role-specific installers; preserve current camera, interaction and shared EventSystem behavior.

### Migration

ISaveData.SaveData now returns Result<StorageSaveResults>. Update custom ISaveData implementations and callers that explicitly declared the old Result return type. Existing file names, encrypted-save layout, endpoint settings and encryption keys are preserved. Set Is Root Module on existing root manifests before using the inspector; new root scaffolds set it automatically. Generate GameFlow code into an existing game module with Contract, Application and Composition layers, using its matching namespace. Handwritten outputs are not overwritten. Use Window > General > Scene Browser to open Scenes; Add Tab registration falls back to this menu when Unity's internal pane registry is unavailable.

## 0.6.0 - 2026-11-01

- Port reusable CameraController improvements from Adan-Game GIS: cursor-centered mouse-wheel zoom, gradual zoom from initial framing outside configured limits, edit-time boundary previews and surface tracking while input is disabled.
- Supply a dedicated Camera/Zoom input action on the framework drag-surface prefab without changing existing movement settings or explicit camera activation.
- Preserve the Configure injection API and support existing custom prefabs without wheel input actions.
- Add camera regression checks and retain development tooling outside release archives.
- Move the shared EventSystem and its Input System bindings to the persistent framework startup scene; keep WorldInteraction free of global input ownership.
- Validate EventSystem ownership and preserve root UI input across module unload/reload; generated isolated tests get a configured EventSystem in their own test root.

### Migration

Keep existing camera settings when upgrading. The supplied drag-surface prefab includes CameraController.inputactions; assign an asset with a Camera/Zoom axis action to custom prefabs to enable wheel zoom. Continue calling ActivateCamera and SetCanInteract explicitly. Cameras initially outside the configured zoom range move gradually toward that range; viewport bounds still take priority. The framework startup scene now supplies the shared EventSystem. Remove duplicate EventSystems from game/module scenes; custom startup scenes must retain one configured EventSystem with InputSystemUIInputModule. Standalone test roots need their own EventSystem.


## 0.5.0 - 2026-10-31

- Add Blackhole.Networking contracts, Unity Transport client/server adapters, connection events and role-specific installer assets.
- Pin Unity Transport 6.5.0 and add transport-only lifecycle and socket tests outside release archives.
- Preserve networking GUIDs without importing the multiplayer game or lesson modules.

### Migration

Install Unity Transport 6.5.0. Register a payload handler for the chosen role and install RuntimeLifecycle in the scope owning that transport. Install one networking role per session and connect clients after initialization. Preserve existing starting modules, settings and game configuration.

## 0.4.0 - 2026-10-31

- Add CameraController with smooth panning, bounded pinch zoom, settings and a drag-surface prefab.
- Add pointer-aware WorldInteraction drag events, release hit flags, camera-plane raycasting and a camera-bound input canvas.
- Add isolated test-scene scaffolding and reliable asset creation after compilation.
- Add local/repository session restoration and lazy authentication header initialization.
- Add encoded GET query parameters with existing query and fragment preservation.
- Place examples and generated tests outside the exported framework.

### Migration

Preserve configured settings, layers, backend URLs, sessions, audio and encryption keys. Add Blackhole.CameraController as a dependency, then activate its camera and enable interaction through ICameraController. Its settings asset supplies movement defaults; callers can override them. Custom interaction implementations need the new drag callbacks and pointer-up target-hit flag. Event payloads now include PointerId and IsHit while retaining the legacy constructor and IsPointerOverUI property. Keep exactly one active EventSystem when composing scenes. Isolated tests are generated under Assets/FrameworkTests/Isolated and are excluded from releases.

## 0.3.0 - 2026-10-19

- Add standalone Messaging contracts, MessagePipe adapters, module assets and dependency injection registration.
- Pin MessagePipe and its VContainer integration to the imported project revision; keep the existing Unity, UniTask and VContainer versions.
- Add Root Module and Persistent Root scaffolding with generated startup entry points and automatic LifetimeScope registration.
- Complete scene wiring after compilation and protect generated scopes against VContainer template replacement.
- Store full scene paths in generated manifests and remove the redundant starting-scene inspector preview.

### Migration

Install the two pinned MessagePipe Git dependencies listed in Version.json before importing this release. Add Blackhole.Messaging to a module's dependencies when it uses IEventPublisher<T> or IEventSubscriber<T>. Keep configured project settings and the starting module when upgrading. Root Module scaffolding now generates and registers the startup entry point automatically. This release does not include the experimental Networking module, Unity Transport, or game lesson/demo code.

## 0.2.0 - 2026-10-01

- Convert the source repository into a standalone Unity development project.
- Relocate framework assets to Assets/BlackholeFramework, preserving Unity metadata.
- Update startup paths, module scenes, editor generators and UI resource references for the Assets location.
- Add a separate development smoke module, local Unity checks and deterministic .unitypackage exports.
- Add tag-triggered GitHub release publishing, integrity checks and exporter tests.

### Migration from 0.1.0

Do not keep the former embedded/UPM installation alongside this version. Move its assets and .meta files into Assets/BlackholeFramework, preserving configured assets, before importing this release. Preserve the encryption key and saved-data settings. Remove the old Git package dependency through Package Manager if present. The original v0.1.0 Git tag remains available.
