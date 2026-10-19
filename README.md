# Blackhole Framework development project

This private repository is the source of truth for the Blackhole Framework. Open it as a complete Unity **6000.5.0f1** project, develop and test changes here, then distribute versioned `.unitypackage` releases to games.

## Layout

| Folder | Purpose |
| --- | --- |
| `Assets/BlackholeFramework` | Reusable framework code, module manifests, installers, settings, scenes, UI and third-party notices. This folder is exported. |
| `Assets/FrameworkTests` | Development smoke module and Unity EditMode/PlayMode tests. Excluded from exports. |
| `Assets/Editor/ProjectBootstrap` | Explicit dependency installation helper. Excluded from exports. |
| `Assets/Settings`, `Assets/Scenes` | Development project's URP configuration and template scene. Excluded from exports. |
| `Packages`, `ProjectSettings` | Pinned dependencies and reproducible Unity project settings. |
| `scripts` | License-free, deterministic release exporter and its tests. |

Generated folders (`Library`, `Temp`, `Logs`, `UserSettings`, `dist`) are ignored. There is no nested Git repository in the framework folder. The original package-only `v0.1.0` tag remains available; `v0.2.0` introduces this project structure.

## Develop locally

1. Clone this repository and open it with Unity 6000.5.0f1. Allow Package Manager to resolve the pinned Git/registry dependencies.
2. Open `Assets/BlackholeFramework/Framework/BlackholeFrameworkScene.unity`. Enter Play Mode; the development smoke module should report successful startup. It exercises the module loader and audio-module dependency wiring without game audio or a backend login.
3. Use **Window > General > Test Runner** to run EditMode and PlayMode tests. If development configuration needs rebuilding, run **Blackhole > Development > Configure Smoke Project**.
4. Make reusable changes under `Assets/BlackholeFramework`. Put experiments, test modules and game-specific examples under `Assets/FrameworkTests`.

The dependency installer is explicit; it does nothing on opening the project. It can be invoked in batch mode with `Blackhole.Development.Editor.PackageInstaller.Install` when bootstrapping or repairing dependency resolution.

## Current framework update

Version 0.3.0 adds standalone in-process Messaging and automatic root-module scaffolding. MessagePipe and MessagePipe.VContainer are pinned to a specific Git revision in Packages/manifest.json, Version.json and the dependency installer. Unity 6000.5.0f1 and the existing UniTask/VContainer versions remain unchanged. Experimental Networking, Unity Transport and game lesson/demo modules are excluded.

## Release on a tag

1. Update `Assets/BlackholeFramework/Version.json` and `CHANGELOG.md` to the same new semantic version. The framework folder's changelog should describe game migration steps.
2. Run Unity EditMode/PlayMode tests, then the release checks:

```powershell
python -m unittest discover -s scripts -p "test_*.py" -v
python scripts/export_unitypackage.py --tag v0.3.0 --output dist
```

3. Commit and push the tested changes to `main`, then tag that commit:

```powershell
git tag -a v0.3.0 -m "Blackhole Framework 0.3.0"
git push origin v0.3.0
```

Substitute the actual new version on subsequent releases; do not move or reuse published tags.

The **Release Blackhole Framework** GitHub Action runs on `v*` tag pushes. It verifies that the tag matches Version.json, runs exporter tests, checks metadata and references, exports the framework, and publishes a GitHub release with:

- `BlackholeFramework-<version>.unitypackage`
- `SHA256SUMS.txt`
- `package-inventory.json` listing exported assets and required dependencies

Prerelease tags such as `v0.3.0-preview.1` create prereleases. A manual workflow dispatch creates a downloadable Actions artifact without publishing a release. The action uses GitHub's built-in token with repository-content permission; no personal token or Unity license secret is required.

**The hosted workflows validate packaging; they do not run Unity compilation or PlayMode tests.** Run those locally before tagging. The exporter implements Unity's asset-archive layout with original asset GUIDs and metadata; preview thumbnails and Unity 6.6 package signatures are not added. Local Unity import validation is part of release verification.

## Clean release defaults

The exporter includes only `Assets/BlackholeFramework`. It preserves source files and `.meta` GUIDs while normalizing the archive's project configuration:

- Starting game module is unassigned; development manifests are removed from the generated catalog.
- Sound library is empty; the reusable audio mixer remains included.
- Saved authentication session and project backend URLs are blank.
- Encryption settings use the documented placeholder; consumers must select their own key.

Source settings are never modified by exporting. Unknown serialized asset references, credential-like values, old package paths, duplicate GUIDs, orphan metadata and missing metadata fail the export. Do not put unrelated project configuration or secrets in the reusable folder.

## Install in another game

Download the `.unitypackage` from this repository's [Releases](https://github.com/bigbangwill/blackhole-framework/releases), install its external dependencies, and import it. Follow [the framework installation guide](Assets/BlackholeFramework/README.md) to configure the starting module, audio, server and storage settings.

Each game tracks the imported copy in its own repository. Upgrades are manual imports: commit the game first, preserve its configured settings/installers/scenes in the import selection, review the resulting diff, remove obsolete files noted in the changelog, and test. Do not install the old UPM copy alongside this version.

## References

- [Unity asset packages](https://docs.unity3d.com/Manual/AssetPackagesCreate.html)
- [Unity asset metadata](https://docs.unity3d.com/Manual/AssetMetadata.html)
- [GitHub workflow syntax](https://docs.github.com/en/actions/reference/workflows-and-actions/workflow-syntax)
- [Framework third-party notices](Assets/BlackholeFramework/Third%20Party%20Notices.md)
