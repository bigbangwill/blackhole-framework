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

Install Universal RP 17.5.0, Unity UI 2.5.0 and Input System 1.19.0. The receiving project must enable the Input System (or Both) in Player settings. Composite Outline needs URP configured as the project's render pipeline. Required built-in Unity modules are listed in Version.json. Newtonsoft.Json 13.0.4 is included with its MIT license; avoid importing another non-overridable Newtonsoft.Json.dll alongside it.

## Import and configure

Download a versioned .unitypackage from https://github.com/bigbangwill/blackhole-framework/releases and import it through Assets > Import Package > Custom Package (menu wording varies by Unity version). The import includes all framework .meta files and preserves asset GUIDs.

Create your game modules using Assets > Create > Blackhole > Modules > Module Scaffold. Enable Root Module to generate its scene, manifest, installer, LifetimeScope and RootModuleStartup entry point. The entry point is registered in the scope's Configure method, including when a scope script already exists. The scope is added to the scene automatically after compilation. Enable Persistent Root for the game starting module, and assign any required dependencies in its manifest. Regular game roots can remain non-persistent.

Select Framework/BlackholeProjectSettings.asset and assign the Game Starting Module. Its manifest supplies the starting scene; no separate starting scene assignment is needed. Populate AudioPlayer/Settings/SoundLibrary.asset with your own clips, configure server URLs when using authentication/networking, and set a project-specific encryption key before creating encrypted saves. Keep that key when upgrading unless saves are migrated. Distributed session tokens, server URLs and audio clips are intentionally empty.

Run Blackhole > Framework > Refresh Module Configuration after compilation. This discovers module manifests, refreshes the catalog and puts the framework scene first in Build Settings and your starting scene second. Open Framework/BlackholeFrameworkScene.unity and enter Play Mode.

## In-process messaging

Add Blackhole.Messaging to the dependencies of modules that use IEventPublisher<T> or IEventSubscriber<T>. The module installs MessagePipe-backed publishers and subscribers through VContainer. Subscribe returns IDisposable; dispose subscriptions when the owning service or presenter stops. Messaging delivers events inside the current process and does not require Unity Transport or a network connection. MessagingSettings is an empty extension point in this version.

## Ownership and updates

Each module keeps its implementation, manifest, installer and editable settings together. Installers hold asset references and must remain stateless. Shared UI documents, styles, textures, fonts, scenes and the audio mixer are included.

Commit or back up the receiving project before importing a newer release. Review the import list and preserve configured settings, installers, scenes and any customized framework files. Imports are manual; they do not merge edits or remove obsolete files. Keep .meta files stable. Releases normalize the starting module, generated catalog, sound library, authentication session, server URLs and encryption settings to reusable defaults, so blindly importing those files resets local configuration.

See the repository README and CHANGELOG for release instructions and migration notes. FrameworkTests and project render-pipeline settings are development-only and excluded from the export.
