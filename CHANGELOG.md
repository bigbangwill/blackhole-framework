# Changelog

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
