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

- Convert the source repository into a standalone Unity development project.
- Relocate framework assets to Assets/BlackholeFramework, preserving Unity metadata.
- Update startup paths, module scenes, editor generators and UI resource references for the Assets location.
- Add a separate development smoke module, local Unity checks and deterministic .unitypackage exports.
- Add tag-triggered GitHub release publishing, integrity checks and exporter tests.

### Migration from 0.1.0

Do not keep the former embedded/UPM installation alongside this version. Move its assets and .meta files into Assets/BlackholeFramework, preserving configured assets, before importing this release. Preserve the encryption key and saved-data settings. Remove the old Git package dependency through Package Manager if present. The original v0.1.0 Git tag remains available.
