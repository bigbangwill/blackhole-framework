# Blackhole Framework

Reusable runtime modules, in-process messaging, UI Toolkit components, interaction, audio, storage and module composition for Unity 6000.5.0f1 or later. This framework is distributed as a .unitypackage and imported into Assets/BlackholeFramework.

## Install dependencies first

Use Unity's Package Manager to install UniTask, VContainer, MessagePipe and MessagePipe.VContainer with these pinned Git URLs:

```text
https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.11
https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.18.0
https://github.com/Cysharp/MessagePipe.git?path=src/MessagePipe.Unity/Assets/Plugins/MessagePipe#3adc01deb1a283cb2faeb36c00178b84884a7fc3
https://github.com/Cysharp/MessagePipe.git?path=src/MessagePipe.Unity/Assets/Plugins/MessagePipe.VContainer#3adc01deb1a283cb2faeb36c00178b84884a7fc3
```

Install Universal RP 17.5.0, Unity UI 2.5.0, Input System 1.19.0 and Unity Transport 6.5.0. The receiving project must enable the Input System (or Both) in Player settings. Composite Outline needs URP configured as the project's render pipeline. Required built-in Unity modules are listed in Version.json. Newtonsoft.Json 13.0.4 is included with its MIT license; avoid importing another non-overridable Newtonsoft.Json.dll alongside it.

## Import and configure

Download a versioned .unitypackage from https://github.com/bigbangwill/blackhole-framework/releases and import it through Assets > Import Package > Custom Package (menu wording varies by Unity version). The import includes all framework .meta files and preserves asset GUIDs.

Create your game modules using Assets > Create > Blackhole > Modules > Module Scaffold. Enable Root Module to generate its scene, manifest, installer, LifetimeScope and RootModuleStartup entry point. The entry point is registered in the scope's Configure method, including when a scope script already exists. The scope is added to the scene automatically after compilation. Enable Persistent Root for the game starting module, and assign any required dependencies in its manifest. Regular game roots can remain non-persistent.

Select Framework/BlackholeProjectSettings.asset and assign the Game Starting Module. Its manifest supplies the starting scene; no separate starting scene assignment is needed. Populate AudioPlayer/Settings/SoundLibrary.asset with your own clips, configure server URLs when using authentication/networking, and set a project-specific encryption key before creating encrypted saves. Keep that key when upgrading unless saves are migrated. Distributed session tokens, server URLs and audio clips are intentionally empty.

Run Blackhole > Framework > Refresh Module Configuration after compilation. This discovers module manifests, refreshes the catalog and puts the framework scene first in Build Settings and your starting scene second. Open Framework/BlackholeFrameworkScene.unity and enter Play Mode.

## Editor tools

Open Window > General > Scene Browser for the Scenes tab. Search matches scene paths; the list shows active, loaded and dirty scenes. Double-click opens a scene on its own. Row buttons load/unload scenes additively, and modified scenes retain Unity's save prompts. Scene changes are disabled during Play Mode. Scenes is also registered in Unity's Add Tab menu when the internal pane registry is available.

Assets > Create > C# Class and Assets > Create > Interface create plain C# files from the bundled templates.

Blackhole > Modules > Inspect Root Modules reads catalog manifests marked Is Root Module, resolves their scene assets and finds compiled IRootModuleStartup implementations. New root scaffolds set this flag; mark existing roots explicitly. The inspector checks class presence, not container registration.

To generate game navigation, scaffold a game-owned GameFlow module with Contract, Application and Composition layers first. In the root-module inspector, set its folder and namespace, then Generate Game Flows. This creates navigation interfaces, a loading-screen use case, an installer and (if missing) an abstract RootModuleEntryOption. The generator adds the required assembly references while preserving existing assembly settings. Add the LoadingScreen module dependency to the game module manifest. Generated files are marked and can be updated; handwritten files must be merged or moved first. Game modules and their generated navigation never become part of the framework release.

## Storage errors

ISaveData.SaveData returns Result<StorageSaveResults> so callers can distinguish invalid keys/data, access denial, long paths and storage failures. Nested relative keys are supported; keys cannot escape the configured save directory. SaveEncryptData preserves underlying storage and encryption errors. Existing saves retain the .sav extension and the IV-prefixed AES format. Preserve existing storage settings and encryption keys when upgrading.

## Isolated module tests

Create Isolated Test Scene is enabled by default in the module scaffolder. It generates a test scene and a testing assembly under Assets/FrameworkTests/Isolated/<ModuleName>, assigns the catalog and manifest after compilation, and adds a camera plus a configured EventSystem to the test root. SceneLoader is added as a dependency. Open the generated scene by itself and enter Play Mode to install the module graph and load dependency scenes. Root-module tests also invoke the production root startup. These files are separate from the production scene and are excluded from framework releases.

## Camera movement and world interaction

Add Blackhole.CameraController to a consuming module's dependencies and resolve Blackhole.CameraController.Contract.ICameraController. Configure the CameraController settings asset, call ActivateCamera(camera), and enable input with SetCanInteract(true). CameraMovementSetting can override the injected movement defaults. The controller provides smooth panning, two-finger orthographic pinch zoom and viewport-aware bounds. Its prefab uses the WorldInteraction interaction layer; preserve or adapt that layer to your project's mask.

WorldInteraction events include PointerId, begin/drag/end callbacks and IsHit on release. IsPointerOverUI remains as a compatibility name for the same target-hit flag. The input canvas binds to Camera.main and raycasts onto the XY plane at Z=0. The persistent framework startup scene owns the shared EventSystem and InputSystemUIInputModule. Module scenes, including WorldInteraction, use it and must not add another EventSystem. Generated isolated test roots provide their own copy so they can run independently. Interaction examples and the tester component live under Assets/FrameworkTests.

Authentication restores a saved session on demand, falling back to the configured token repository. RestRequest.Get accepts an optional queryParams dictionary and safely appends encoded values to an existing query string.

## In-process messaging

Add Blackhole.Messaging to the dependencies of modules that use IEventPublisher<T> or IEventSubscriber<T>. The module installs MessagePipe-backed publishers and subscribers through VContainer. Subscribe returns IDisposable; dispose subscriptions when the owning service or presenter stops. Messaging delivers events inside the current process and does not require Unity Transport or a network connection. MessagingSettings is an empty extension point in this version.

## Ownership and updates

Blackhole.Networking provides Unity Transport clients and servers with configurable role-specific installer assets. Supply the payload handler for the chosen role, install the existing RuntimeLifecycle installer in that transport's owning scope, and initialize the client before calling Connect. See Networking/README.md for scope ownership, messaging dependencies and payload limits. Multiplayer game rules and lesson scenes are not included.

Each module keeps its implementation, manifest, installer and editable settings together. Installers hold asset references and must remain stateless. Shared UI documents, styles, textures, fonts, scenes and the audio mixer are included.

Commit or back up the receiving project before importing a newer release. Review the import list and preserve configured settings, installers, scenes and any customized framework files. Imports are manual; they do not merge edits or remove obsolete files. Keep .meta files stable. Releases normalize the starting module, generated catalog, sound library, authentication session, server URLs and encryption settings to reusable defaults, so blindly importing those files resets local configuration.

See the repository README and CHANGELOG for release instructions and migration notes. FrameworkTests and project render-pipeline settings are development-only and excluded from the export.
