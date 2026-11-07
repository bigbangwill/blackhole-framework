# Changelog

## 0.7.3 - 2026-11-07

- Load scenes from the framework dependency graph additively under the framework scope before starting the first game root. GameFlow navigation waits for framework scene startup and leaves framework dependency scenes loaded when switching roots.

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

- Add Blackhole.Networking with client/server contracts, reliable Unity Transport adapters, peer and connection events, and configurable role-specific installers.
- Pin Unity Transport 6.5.0 for the existing Unity 6000.5 development environment.
- Preserve source networking GUIDs and keep multiplayer game logic and lesson scenes outside the framework import.
- Add transport-only tests for role isolation, reliable payload delivery, scope lifecycle and server port reuse.

### Migration

Install Unity Transport 6.5.0 before importing this version. Networking requires a payload handler for the chosen role and the existing RuntimeLifecycle installer in the transport's owning scope. Messaging supplies the typed event publishers. Use the client installer or server installer once per session; call Connect after client initialization. The framework starting module and existing game configuration are preserved.

## 0.4.0 - 2026-10-31

- Add the reusable CameraController module with smooth panning, bounded orthographic pinch zoom, settings and a drag-surface prefab.
- Add pointer-aware WorldInteraction drag events, release hit flags, camera-plane raycasting and a camera-bound input canvas.
- Add isolated module test-scene scaffolding, dependency scene loading and reliable asset creation after compilation.
- Restore authentication sessions from local storage with a ScriptableObject fallback and lazy header initialization.
- Support encoded GET query parameters while preserving existing queries and URL fragments.
- Keep generated test scenes and interaction examples under Assets/FrameworkTests and outside release archives.

### Migration

Keep configured settings, layer masks, backend URLs, saved sessions, audio and encryption keys when upgrading. Add Blackhole.CameraController to a consuming module's dependencies, activate its camera and enable interaction through ICameraController. Movement settings are injected from the module settings asset and can be overridden at runtime. Custom IInteractionTarget and IInteractionEventSource implementations must implement the new drag callbacks; OnPointerUp now takes a target-hit flag. WorldInteractionEventData includes PointerId and IsHit; legacy constructors and IsPointerOverUI remain available. The WorldInteraction scene retains its EventSystem for standalone use; projects with a shared EventSystem should retain exactly one active instance. Generated isolated tests live under Assets/FrameworkTests/Isolated.

## 0.3.0 - 2026-10-19

- Add standalone Messaging contracts, MessagePipe adapters, module assets and dependency injection registration.
- Pin MessagePipe and its VContainer integration to the imported project revision; keep the existing Unity, UniTask and VContainer versions.
- Add Root Module and Persistent Root scaffolding with generated startup entry points and automatic LifetimeScope registration.
- Complete scene wiring after compilation and protect generated scopes against VContainer template replacement.
- Store full scene paths in generated manifests and remove the redundant starting-scene inspector preview.

### Migration

Install the two pinned MessagePipe Git dependencies listed in Version.json before importing this release. Add Blackhole.Messaging to a module's dependencies when it uses IEventPublisher<T> or IEventSubscriber<T>. Keep configured project settings and the starting module when upgrading. Root Module scaffolding now generates and registers the startup entry point automatically. This release does not include the experimental Networking module, Unity Transport, or game lesson/demo code.

## 0.2.0 - 2026-10-01

- Convert the private repository into a complete Unity 6000.5.0f1 framework development project.
- Move reusable framework assets to `Assets/BlackholeFramework` while preserving their Unity GUIDs.
- Update framework scene paths, configuration paths, editor generators and UI resource URLs.
- Add a development smoke module plus EditMode/PlayMode checks, outside the export folder.
- Pin UniTask 2.5.11, VContainer 1.18.0, URP 17.5.0, Input System 1.19.0 and Unity UI 2.5.0.
- Add metadata/reference validation, configuration normalization and deterministic asset-archive exports.
- Publish `.unitypackage`, checksum and asset-inventory files automatically on version-tag pushes.

### Migration

The default branch is now a Unity project, not a UPM package root. Use release `.unitypackage` files for new installations. Preserve configuration and `.meta` files when migrating an existing embedded copy into `Assets/BlackholeFramework`; remove the old Git package dependency through Package Manager and avoid duplicate installations. The original `v0.1.0` tag remains unchanged.
