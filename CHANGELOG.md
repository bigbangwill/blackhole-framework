# Changelog

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
