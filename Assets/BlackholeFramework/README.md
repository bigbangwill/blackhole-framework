# Blackhole Framework

Reusable runtime modules, UI Toolkit components, networking, interaction, audio, storage and module composition for Unity 6000.5.0f1 or later. This framework is distributed as a .unitypackage and imported into Assets/BlackholeFramework.

## Install dependencies first

Use Unity's Package Manager to install UniTask and VContainer with these Git URLs:

```text
https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.11
https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.18.0
```

Install Universal RP 17.5.0, Unity UI 2.5.0 and Input System 1.19.0. The receiving project must enable the Input System (or Both) in Player settings. Composite Outline needs URP configured as the project's render pipeline. Required built-in Unity modules are listed in Version.json. Newtonsoft.Json 13.0.4 is included with its MIT license; avoid importing another non-overridable Newtonsoft.Json.dll alongside it.

## Import and configure

Download a versioned .unitypackage from https://github.com/bigbangwill/blackhole-framework/releases and import it through Assets > Import Package > Custom Package (menu wording varies by Unity version). The import includes all framework .meta files and preserves asset GUIDs.

Create your game modules using Assets > Create > Blackhole > Modules > Module Scaffold. The game starting module needs a persistent manifest, an existing scene, its installer, assigned dependencies and a RootModuleStartup entry point registered in the root scene's LifetimeScope.

Select Framework/BlackholeProjectSettings.asset and assign the Game Starting Module. Populate AudioPlayer/Settings/SoundLibrary.asset with your own clips, configure server URLs when using authentication/networking, and set a project-specific encryption key before creating encrypted saves. Keep that key when upgrading unless saves are migrated. Distributed session tokens, server URLs and audio clips are intentionally empty.

Run Blackhole > Framework > Refresh Module Configuration after compilation. This discovers module manifests, refreshes the catalog and puts the framework scene first in Build Settings and your starting scene second. Open Framework/BlackholeFrameworkScene.unity and enter Play Mode.

## Ownership and updates

Each module keeps its implementation, manifest, installer and editable settings together. Installers hold asset references and must remain stateless. Shared UI documents, styles, textures, fonts, scenes and the audio mixer are included.

Commit or back up the receiving project before importing a newer release. Review the import list and preserve configured settings, installers, scenes and any customized framework files. Imports are manual; they do not merge edits or remove obsolete files. Keep .meta files stable. Releases normalize the starting module, generated catalog, sound library, authentication session, server URLs and encryption settings to reusable defaults, so blindly importing those files resets local configuration.

See the repository README and CHANGELOG for release instructions and migration notes. FrameworkTests and project render-pipeline settings are development-only and excluded from the export.
