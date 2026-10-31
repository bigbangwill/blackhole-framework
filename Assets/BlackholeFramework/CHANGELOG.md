# Changelog

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
