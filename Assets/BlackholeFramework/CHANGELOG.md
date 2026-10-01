# Changelog

## 0.2.0 - 2026-10-01

- Convert the source repository into a standalone Unity development project.
- Relocate framework assets to Assets/BlackholeFramework, preserving Unity metadata.
- Update startup paths, module scenes, editor generators and UI resource references for the Assets location.
- Add a separate development smoke module, local Unity checks and deterministic .unitypackage exports.
- Add tag-triggered GitHub release publishing, integrity checks and exporter tests.

### Migration from 0.1.0

Do not keep the former embedded/UPM installation alongside this version. Move its assets and .meta files into Assets/BlackholeFramework, preserving configured assets, before importing this release. Preserve the encryption key and saved-data settings. Remove the old Git package dependency through Package Manager if present. The original v0.1.0 Git tag remains available.
