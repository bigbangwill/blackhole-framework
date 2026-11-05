# Adan-Game framework import audit — 2026-11-06

The source repository was inspected without checkout or modification. The working tree was on GIS (9810d98). All local and remote-tracking branch tips were inspected for framework changes and editor tools; source history was also searched for editor scripts.

## Imported

| Change | Source | Destination / adaptation |
| --- | --- | --- |
| Scenes browser and Add Tab registration | origin/CountryRoot, origin/LandRoot; Scene tab view (86f198a) | Framework/Editor/Scenes; framework namespace; preserve built-in panes and use fully qualified pane names. |
| C# class/interface creation menus and templates | origin/CountryRoot, origin/LandRoot | Framework/Editor/ScriptCreationMenu.cs and ScriptTemplates; original GUIDs retained. |
| Root-module inspector and GameFlow generator | origin/GameFlow, origin/LandRoot | Framework/Editor/Modules; configurable game-owned folder and namespace, assembly-reference wiring, handwritten-file protection. No game runtime module was copied. |
| Root-module metadata and scaffolder flag | origin/GameFlow, origin/LandRoot | Add IsRootModule without removing virtual manifest properties or protected fields. Preserve isolated tests under FrameworkTests. |
| Concrete entry-options DI registration | origin/GameFlow, origin/LandRoot | RuntimeModuleLoader; register both generic/base and concrete types so base registration is retained. |
| Nested save paths and typed storage errors | GIS | ISaveData, SaveDataService, StorageData and StorageSaveResults; keep existing save settings and .sav paths. |
| Encrypted-save error propagation and corrupt-input handling | GIS | SaveEncryptData and EncryptionData; retain IV-prefixed AES format and all configured keys. |
| Authentication save-failure detail | GIS | Add typed storage error to the existing save-failure diagnostic; retain benign first-launch/missing-session behavior. |
| REST base URL/endpoint joining | GIS | RestRequest; normalize the slash separator without changing existing query encoding or fragments. |

## Reviewed and retained from this framework

- Current CameraController, pointer/button identity, captured drag targets, camera-plane raycasting, compatibility constructors and IsHit alias. Older GIS/TestPR interaction code would remove those fixes.
- Shared framework EventSystem ownership, validation and standalone-test EventSystems. The source validator and isolated-scene setup are older.
- Current client/server networking assemblies and PlatformModuleManifestSO. The source checkout has no replacement for these changes.
- Tests and example scenes remain under Assets/FrameworkTests rather than source module Editor/Test folders.
- BOM/whitespace-only interface differences, older README/changelog/version files and third-party notice/nuspec differences were not imported as behavior changes. Version and changelogs are updated together for this import.

## Excluded game data and scratch assets

Source framework/startup scenes, project settings, starting module, module catalog, authentication configuration/session, REST endpoint settings and WorldInteraction settings were not copied. The source WorldInteraction/Test scene, InteractiableEventTESTER and empty NewUXMLTemplate were scratch/example assets. GIS, GameFlow, LandRoot, CountryRoot, generated game-root test scopes and other game-owned modules remain in the source game.

The GameFlow generator is reusable editor code; it writes future navigation code only into a user-selected game module. It does not ship any Adan module IDs, generated game navigation, scenes, endpoints or credentials.

## Validation

- 52 EditMode tests passed, including networking, editor tools, nested storage paths, storage errors, encrypted save round trips, corrupted ciphertext and REST URL separators.
- 10 PlayMode tests passed, including base/concrete entry-options injection, framework startup, camera interaction and networking lifecycle.
- A temporary game module was scaffolded under FrameworkTests. The adapted generator produced compiling interfaces, use case and installer with a custom namespace, added GUID assembly references while preserving define constraints, and produced identical output on a second run. The temporary module was removed through Unity's asset APIs.
- FrameworkStartupValidator passed. Imported editor tool GUIDs were checked against their source branch metadata.
- All nine unitypackage exporter tests passed, and a v0.7.0 archive was generated under the ignored TestResults directory. No tag or release was published.
