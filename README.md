# Blackhole Framework

Reusable Unity modules shared by Blackhole projects.

## Installation in another Unity project

Use Unity **6000.5.0f1 or later**. The package declares Unity UI 2.5.0, Input System 1.19.0, and Universal RP 17.5.0. Composite Outline requires a URP render pipeline configured in the project.

### 1. Install the external dependencies

In Unity's Package Manager, choose **Add package from Git URL** for each dependency before importing Blackhole:

```text
https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.11
https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.18.0
```

Unity does not support Git URLs as dependencies inside a package's package.json. These dependencies must be installed in the consuming project; see [Unity's Git dependency documentation](https://docs.unity3d.com/Manual/upm-git.html). Newtonsoft.Json 13.0.4 is bundled with its MIT license; avoid importing another non-overridable Newtonsoft.Json.dll alongside it.

### 2. Import Blackhole

For normal development, clone the package as an **editable embedded package** into your new project's Packages folder before opening Unity:

```powershell
git clone --branch v0.1.0 https://github.com/bigbangwill/blackhole-framework.git "<YourUnityProject>/Packages/com.bigbang.blackhole-framework"
```

The repository is private. Authenticate Git with a GitHub account that has access, for example with `gh auth login` and `gh auth setup-git`.

You can also use **Add package from Git URL**:

```text
https://github.com/bigbangwill/blackhole-framework.git#v0.1.0
```

Git-installed packages live in Unity's cache and are read-only. This version stores editable project settings, a generated module catalog and installer settings inside the package. **Embed it before configuring or running the framework**: copy its package folder from Library/PackageCache to Packages/com.bigbang.blackhole-framework, excluding any .git folder. The embedded package takes precedence over the Git dependency. See [Unity's embedded-package documentation](https://docs.unity3d.com/Manual/upm-embed.html).

### 3. Configure your game

1. Enable the Input System in **Project Settings > Player > Active Input Handling** (Input System Package or Both).
2. Create a game module with **Assets > Create > Blackhole > Modules > Module Scaffold**, or supply your own manifest, installer and scene. Set the startup module to persistent and assign its required module dependencies.
3. Select **Packages/com.bigbang.blackhole-framework/Framework/BlackholeProjectSettings.asset** and assign your game manifest to **Game Starting Module**.
4. Set your authentication/server URLs if those modules are used. The distributed session token is empty and runtime login must supply it. Set a project-specific encryption key before saving encrypted data; do not change it after creating saves unless you migrate them.
5. Populate **AudioPlayer/Settings/SoundLibrary.asset** with your own clips. It is intentionally empty; the framework mixer and mixer groups are included.
6. Run **Blackhole > Framework > Refresh Module Configuration** after compilation finishes. This discovers your modules and updates Build Settings with the framework scene first and your starting module scene second.
7. Open the framework startup scene and enter Play Mode.

The repository contains only the reusable package and its documentation. The original game's modules, startup selection, audio clips, saved login tokens, backend URLs and encryption key are excluded. Preserve your project configuration when updating an embedded copy.

## Asset ownership

Each framework module has exactly one `ModuleManifestSO`, one `ModuleInstallerSO` asset and its settings inside `Packages/com.bigbang.blackhole-framework`. Manifests reference installers directly; installers reference their settings and required resources directly. There is no manually maintained installer registry, generated configuration copy or runtime resource-name lookup for module installation.

Global dialogue UI documents, styles, textures and runtime theme, loading screen UI, the shared global style and fonts, and the audio mixer are inside the package alongside their scenes and settings. These defaults travel with the framework when it is imported into another project. The mixer is at `AudioPlayer/Settings/AudioPlayerMixer.mixer`; game-specific audio clips belong under the consuming project's Assets folder, outside Resources folders. Game-owned module manifests, installers and data remain in their game modules. Framework startup uses the package's `Framework/BlackholeFrameworkScene.unity`, project settings and module catalog.

Installers must remain stateless: use the supplied container builder and do not store containers, active sessions or created service instances on the installer asset.

`FrameworkInstaller` is a runtime module installer: it registers `FrameworkEntryPoint`, which loads the selected game starting module. It does not create or copy ScriptableObject assets. The former editor `FrameworkSetupAutoInstaller` and asset-copying setup are no longer used. Keep the runtime installer while startup registration is owned by the framework module manifest.

## Refreshing configuration

The editor refreshes the module catalog and build-scene list after asset import or script reload. New manifests are discovered automatically from Assets and packages; no hardcoded list of module classes needs updating.

Use **Blackhole > Framework > Refresh Module Configuration**, or call:

```csharp
Blackhole.Framework.Composition.Editor.FrameworkProjectSetup.RegenerateSettings();
```

This only rediscovers manifests and refreshes catalog/build wiring. It does not clone assets, reset settings, recreate deleted configuration or overwrite installer resource assignments. Run it in Edit Mode after compilation/import finish. Batch-mode callers must invoke it explicitly.

Framework project settings select the game's persistent starting module. The framework scene is build index zero; the starting module's scene is second. Module dependency references must all be assigned: missing dependencies fail with an actionable error instead of being silently skipped.

## Reusing the framework

This project uses an editable embedded package. Its settings and installers can therefore store direct references to game resources under Assets. The dialogue, loading screen, shared UI and mixer defaults need no separate asset import. Import/configure game resources (including audio clips) and select a starting module when using the framework in another project.

These configured package assets are project changes: preserve them when updating the framework. Registry/Git cache packages are normally read-only; use an embedded editable package when configuring assets in place. Do not publish project credentials as shared package defaults.

Unity UI and Input System are declared package dependencies. UniTask and VContainer must be available through the consuming project's registries or Git dependencies.
