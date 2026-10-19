using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blackhole.ModuleSystem.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blackhole.Editor
{
    public sealed class ModuleScaffolderWindow : EditorWindow
    {
        private const string DefaultModulesPath = "Assets/Game/Modules";
        private const string SharedKernelApplicationAssemblyName = "Blackhole.SharedKernel.Application";
        private const string SharedKernelCompositionAssemblyName = "Blackhole.SharedKernel.Composition";
        private const string SharedKernelContractAssemblyName = "Blackhole.SharedKernel.Contract";
        private const string SettingsContractAssemblyName = "Blackhole.Settings.Contract";
        private const string UniTaskAssemblyName = "UniTask";
        private const string VContainerAssemblyName = "VContainer";
        private const string ModuleSystemCompositionAssemblyName = "Blackhole.ModuleSystem.Composition";

        private readonly Dictionary<ModuleLayer, bool> selectedLayers = new Dictionary<ModuleLayer, bool>
        {
            { ModuleLayer.Contract, true },
            { ModuleLayer.Domain, true },
            { ModuleLayer.Application, true },
            { ModuleLayer.Infrastructure, true },
            { ModuleLayer.Presentation, true },
            { ModuleLayer.Composition, true }
        };

        private string assemblyRoot = "Blackhole.NewModule";
        private string modulesPath = DefaultModulesPath;
        private string moduleFolderName = "NewModule";
        private string sceneName = "NewModule";
        private bool createScene;
        private bool createManifest = true;
        private bool createSettings = true;
        private bool isRootModule;
        private bool persistentRootModule;
        private Vector2 scrollPosition;

        [MenuItem("Assets/Create/Blackhole/Modules/Module Scaffold", false, 80)]
        public static void OpenFromCreateMenu()
        {
            var window = GetWindow<ModuleScaffolderWindow>(true, "Create Module Scaffold", true);
            window.minSize = new Vector2(460f, 540f);
            window.ApplySelectionAsTargetPath();
            window.ShowUtility();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Module", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            assemblyRoot = EditorGUILayout.TextField("Assembly Root", assemblyRoot);
            if (EditorGUI.EndChangeCheck())
            {
                moduleFolderName = SuggestFolderName(assemblyRoot);
                sceneName = moduleFolderName;
            }

            moduleFolderName = EditorGUILayout.TextField("Folder Name", moduleFolderName);
            modulesPath = EditorGUILayout.TextField("Modules Path", modulesPath);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Layers", EditorStyles.boldLabel);
            DrawLayerToggle(ModuleLayer.Contract, "Contract", "Public DTOs, contracts, and module-facing interfaces.");
            DrawLayerToggle(ModuleLayer.Domain, "Domain", "Core entities, value objects, and domain services.");
            DrawLayerToggle(ModuleLayer.Application, "Application", "Use cases, ports, and application services.");
            DrawLayerToggle(ModuleLayer.Infrastructure, "Infrastructure", "Adapters and technical implementations.");
            DrawLayerToggle(ModuleLayer.Presentation, "Presentation", "Controllers, presenters, views, and MonoBehaviours.");
            DrawLayerToggle(ModuleLayer.Composition, "Composition", "Installers and dependency injection wiring.");

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Module Assets", EditorStyles.boldLabel);
            isRootModule = EditorGUILayout.ToggleLeft(new GUIContent("Root Module", "Generate a LifetimeScope and RootModuleStartup entry point and wire them into the module scene after compilation."), isRootModule);
            if (isRootModule)
            {
                createScene = true;
                createManifest = true;
                selectedLayers[ModuleLayer.Composition] = true;
                persistentRootModule = EditorGUILayout.ToggleLeft(new GUIContent("Persistent Root", "Enable this for the game starting module, which stays loaded while other game roots load beneath it."), persistentRootModule);
                EditorGUILayout.HelpBox("Root modules include a scene, manifest, Composition layer, LifetimeScope and startup entry point. Scene wiring finishes after scripts compile.", MessageType.Info);
            }

            using (new EditorGUI.DisabledScope(isRootModule))
                createScene = EditorGUILayout.ToggleLeft(new GUIContent("Create Scene", "Create an empty scene in the module folder and add it to Build Settings."), createScene);
            if (createScene)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    sceneName = EditorGUILayout.TextField("Scene Name", sceneName);
                }
            }

            using (new EditorGUI.DisabledScope(isRootModule))
                createManifest = EditorGUILayout.ToggleLeft(new GUIContent("Create Module Manifest", "Create and register a ModuleManifestSO for this module."), createManifest);

            createSettings = EditorGUILayout.ToggleLeft(
                new GUIContent("Create Settings", "Create a module-specific ScriptableObject class and asset without registering it."),
                createSettings);
            if (createSettings && !selectedLayers[ModuleLayer.Infrastructure])
            {
                EditorGUILayout.HelpBox("Creating settings requires the Infrastructure layer.", MessageType.Info);
            }

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Target", GetModuleAssetPath());
                if (createScene)
                {
                    EditorGUILayout.LabelField("Scene", GetSceneAssetPath());
                }

                if (createManifest)
                {
                    EditorGUILayout.LabelField("Manifest", GetManifestAssetPath());
                }

                if (createSettings)
                {
                    EditorGUILayout.LabelField("Settings Script", GetSettingsScriptAssetPath());
                    EditorGUILayout.LabelField("Settings Asset", GetSettingsAssetPath());
                }

                foreach (var layer in GetSelectedLayers())
                {
                    EditorGUILayout.LabelField("Assembly", $"{assemblyRoot}.{layer}");
                }
            }

            EditorGUILayout.Space(12f);
            using (new EditorGUI.DisabledScope(!CanCreate(out _)))
            {
                if (GUILayout.Button("Create Module", GUILayout.Height(32f)))
                {
                    CreateModule();
                }
            }

            if (!CanCreate(out var validationMessage))
            {
                EditorGUILayout.HelpBox(validationMessage, MessageType.Warning);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawLayerToggle(ModuleLayer layer, string label, string tooltip)
        {
            using (new EditorGUI.DisabledScope(isRootModule && layer == ModuleLayer.Composition))
                selectedLayers[layer] = EditorGUILayout.ToggleLeft(new GUIContent(label, tooltip), selectedLayers[layer]);
        }

        private void ApplySelectionAsTargetPath()
        {
            var selectedPath = Selection.activeObject == null ? null : AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrWhiteSpace(selectedPath))
            {
                modulesPath = DefaultModulesPath;
                return;
            }

            if (File.Exists(selectedPath))
            {
                selectedPath = Path.GetDirectoryName(selectedPath)?.Replace('\\', '/');
            }

            modulesPath = string.IsNullOrWhiteSpace(selectedPath) ? DefaultModulesPath : selectedPath;
        }

        private void CreateModule()
        {
            if (!CanCreate(out var validationMessage))
            {
                EditorUtility.DisplayDialog("Cannot Create Module", validationMessage, "OK");
                return;
            }

            var modulePath = GetModuleAssetPath();
            if (AssetDatabase.IsValidFolder(modulePath) &&
                !EditorUtility.DisplayDialog("Folder Already Exists", $"{modulePath} already exists. Add missing layer folders and asmdefs?", "Continue", "Cancel"))
            {
                return;
            }

            var selected = GetSelectedLayers().ToArray();
            var existingAssemblies = FindAssemblyGuidsByName();
            var layerGuids = selected.ToDictionary(layer => layer, layer => GetOrCreateLayerGuid(layer, existingAssemblies));

            EnsureAssetFolder(modulesPath);
            EnsureAssetFolder(modulePath);
            var scriptsPath = $"{modulePath}/Scripts";
            EnsureAssetFolder(scriptsPath);

            foreach (var layer in selected)
            {
                var layerPath = $"{scriptsPath}/{layer}";
                EnsureAssetFolder(layerPath);
                WriteAsmdef(layerPath, layer, layerGuids, existingAssemblies);
                EnsureStarterFolders(layerPath, layer);
            }

            if (createSettings)
            {
                CreateSettingsScaffold();
            }

            if (createScene)
            {
                CreateOrUpdateScene();
            }

            ModuleManifestSO manifest = null;
            if (createManifest)
            {
                manifest = CreateOrUpdateManifest();
            }

            if (selectedLayers[ModuleLayer.Composition])
                CreateInstallerScaffold();

            if (isRootModule)
                CreateRootModuleScaffold();

            if (manifest != null)
            {
                RegisterManifestInCatalogs(manifest);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Module Created", BuildCompletionMessage(modulePath), "OK");
            Close();
        }

        private void WriteAsmdef(
            string layerPath,
            ModuleLayer layer,
            IReadOnlyDictionary<ModuleLayer, string> layerGuids,
            IReadOnlyDictionary<string, AssemblyInfo> existingAssemblies)
        {
            var assemblyName = $"{assemblyRoot}.{layer}";
            var references = GetReferences(layer, layerGuids, existingAssemblies)
                .Select(reference => $"GUID:{reference}")
                .Concat(ReadExistingReferences($"{layerPath}/{assemblyName}.asmdef"))
                .Distinct()
                .ToArray();

            var asmdefPath = $"{layerPath}/{assemblyName}.asmdef";
            var fullAsmdefPath = ToFullPath(asmdefPath);
            var asmdef = File.Exists(fullAsmdefPath)
                ? JsonUtility.FromJson<AssemblyDefinition>(File.ReadAllText(fullAsmdefPath))
                : new AssemblyDefinition
                {
                    includePlatforms = Array.Empty<string>(),
                    excludePlatforms = Array.Empty<string>(),
                    allowUnsafeCode = false,
                    overrideReferences = false,
                    precompiledReferences = Array.Empty<string>(),
                    autoReferenced = true,
                    defineConstraints = Array.Empty<string>(),
                    versionDefines = Array.Empty<VersionDefine>(),
                    noEngineReferences = false
                };

            asmdef.name = assemblyName;
            if (string.IsNullOrWhiteSpace(asmdef.rootNamespace))
            {
                asmdef.rootNamespace = assemblyName;
            }

            asmdef.references = references;
            File.WriteAllText(fullAsmdefPath, JsonUtility.ToJson(asmdef, true));
            if (!File.Exists(ToFullPath($"{asmdefPath}.meta")))
            {
                File.WriteAllText(ToFullPath($"{asmdefPath}.meta"), CreateAsmdefMeta(layerGuids[layer]));
            }
        }

        private string GetOrCreateLayerGuid(ModuleLayer layer, IReadOnlyDictionary<string, AssemblyInfo> existingAssemblies)
        {
            return existingAssemblies.TryGetValue($"{assemblyRoot}.{layer}", out var assembly)
                ? assembly.Guid
                : GenerateUnityGuid();
        }

        private IEnumerable<string> GetReferences(ModuleLayer layer,
            IReadOnlyDictionary<ModuleLayer, string> layerGuids,
            IReadOnlyDictionary<string, AssemblyInfo> existingAssemblies)
        {
            if (layer == ModuleLayer.Application || layer == ModuleLayer.Infrastructure || layer == ModuleLayer.Presentation || layer == ModuleLayer.Composition)
            {
                foreach (var guid in GetLayerGuid(ModuleLayer.Contract, layerGuids, existingAssemblies))
                {
                    yield return guid;
                }
            }

            if (layer == ModuleLayer.Application || layer == ModuleLayer.Infrastructure || layer == ModuleLayer.Composition)
            {
                foreach (var guid in GetLayerGuid(ModuleLayer.Domain, layerGuids, existingAssemblies))
                {
                    yield return guid;
                }
            }

            if (layer == ModuleLayer.Infrastructure || layer == ModuleLayer.Presentation || layer == ModuleLayer.Composition)
            {
                foreach (var guid in GetLayerGuid(ModuleLayer.Application, layerGuids, existingAssemblies))
                {
                    yield return guid;
                }
            }

            if (layer == ModuleLayer.Composition)
            {
                foreach (var guid in GetLayerGuid(ModuleLayer.Infrastructure, layerGuids, existingAssemblies))
                {
                    yield return guid;
                }

                foreach (var guid in GetLayerGuid(ModuleLayer.Presentation, layerGuids, existingAssemblies))
                {
                    yield return guid;
                }
            }

            if ((layer == ModuleLayer.Application || layer == ModuleLayer.Composition) &&
                existingAssemblies.TryGetValue(SharedKernelApplicationAssemblyName, out var sharedKernelApplication))
            {
                yield return sharedKernelApplication.Guid;
            }

            if (layer == ModuleLayer.Composition &&
                existingAssemblies.TryGetValue(SharedKernelCompositionAssemblyName, out var sharedKernelComposition))
            {
                yield return sharedKernelComposition.Guid;
            }

            if (layer != ModuleLayer.Domain &&
                existingAssemblies.TryGetValue(SharedKernelContractAssemblyName, out var sharedKernelContract))
            {
                yield return sharedKernelContract.Guid;
            }

            if (layer == ModuleLayer.Infrastructure &&
                existingAssemblies.TryGetValue(SettingsContractAssemblyName, out var settingsContract))
            {
                yield return settingsContract.Guid;
            }

            if (layer != ModuleLayer.Domain &&
                existingAssemblies.TryGetValue(UniTaskAssemblyName, out var uniTask))
            {
                yield return uniTask.Guid;
            }

            if ((layer == ModuleLayer.Infrastructure || layer == ModuleLayer.Presentation || layer == ModuleLayer.Composition) &&
                existingAssemblies.TryGetValue(VContainerAssemblyName, out var vContainer))
            {
                yield return vContainer.Guid;
            }

            if (isRootModule && layer == ModuleLayer.Composition && existingAssemblies.TryGetValue(ModuleSystemCompositionAssemblyName, out var moduleSystemComposition))
                yield return moduleSystemComposition.Guid;
        }

        private IEnumerable<string> GetLayerGuid(ModuleLayer layer, IReadOnlyDictionary<ModuleLayer, string> layerGuids, IReadOnlyDictionary<string, AssemblyInfo> existingAssemblies)
        {
            if (layerGuids.TryGetValue(layer, out var guid) || TryGetAssemblyGuid(existingAssemblies, $"{assemblyRoot}.{layer}", out guid))
            {
                yield return guid;
            }
        }

        private static bool TryGetAssemblyGuid(
            IReadOnlyDictionary<string, AssemblyInfo> assemblies,
            string assemblyName,
            out string guid)
        {
            if (assemblies.TryGetValue(assemblyName, out var assembly))
            {
                guid = assembly.Guid;
                return true;
            }

            guid = string.Empty;
            return false;
        }

        private static IEnumerable<string> ReadExistingReferences(string asmdefPath)
        {
            var fullPath = ToFullPath(asmdefPath);
            if (!File.Exists(fullPath))
            {
                return Enumerable.Empty<string>();
            }

            var definition = JsonUtility.FromJson<AssemblyDefinition>(File.ReadAllText(fullPath));
            return definition?.references == null
                ? Enumerable.Empty<string>()
                : definition.references;
        }

        private void CreateOrUpdateScene()
        {
            var scenePath = GetSceneAssetPath();
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                var previousActiveScene = SceneManager.GetActiveScene();
                var moduleScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                EditorSceneManager.SaveScene(moduleScene, scenePath);
                EditorSceneManager.CloseScene(moduleScene, true);

                if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
                {
                    SceneManager.SetActiveScene(previousActiveScene);
                }
            }

            AddSceneToBuildSettings(scenePath);
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            var existingIndex = scenes.FindIndex(scene => scene.path.Equals(scenePath, StringComparison.Ordinal));
            if (existingIndex >= 0)
            {
                if (!scenes[existingIndex].enabled)
                {
                    scenes[existingIndex] = new EditorBuildSettingsScene(scenePath, true);
                    EditorBuildSettings.scenes = scenes.ToArray();
                }

                return;
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private ModuleManifestSO CreateOrUpdateManifest()
        {
            var manifestPath = GetManifestAssetPath();
            var manifest = AssetDatabase.LoadAssetAtPath<ModuleManifestSO>(manifestPath);
            var isNewManifest = manifest == null;

            if (isNewManifest)
            {
                manifest = CreateInstance<ModuleManifestSO>();
                AssetDatabase.CreateAsset(manifest, manifestPath);
            }

            var serializedManifest = new SerializedObject(manifest);
            serializedManifest.FindProperty("moduleID").stringValue = assemblyRoot;
            serializedManifest.FindProperty("moduleScene").stringValue = createScene ? GetSceneAssetPath() : string.Empty;
            if (isNewManifest)
            {
                serializedManifest.FindProperty("isPersistant").boolValue = false;
                serializedManifest.FindProperty("dependencies").arraySize = 0;
            }

            if (isRootModule)
                serializedManifest.FindProperty("isPersistant").boolValue = persistentRootModule;

            serializedManifest.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(manifest);
            return manifest;
        }

        private static void RegisterManifestInCatalogs(ModuleManifestSO manifest)
        {
            foreach (var catalogGuid in AssetDatabase.FindAssets($"t:{nameof(ModuleManifestCatalogSO)}"))
            {
                var catalogPath = AssetDatabase.GUIDToAssetPath(catalogGuid);
                var catalog = AssetDatabase.LoadAssetAtPath<ModuleManifestCatalogSO>(catalogPath);
                if (catalog == null)
                {
                    continue;
                }

                var serializedCatalog = new SerializedObject(catalog);
                var manifests = serializedCatalog.FindProperty("moduleManifests");
                var alreadyRegistered = Enumerable.Range(0, manifests.arraySize)
                    .Any(index => manifests.GetArrayElementAtIndex(index).objectReferenceValue == manifest);
                if (alreadyRegistered)
                {
                    continue;
                }

                manifests.InsertArrayElementAtIndex(manifests.arraySize);
                manifests.GetArrayElementAtIndex(manifests.arraySize - 1).objectReferenceValue = manifest;
                serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(catalog);
            }
        }

        private string BuildCompletionMessage(string modulePath)
        {
            var createdItems = new List<string> { "layer assemblies" };
            if (selectedLayers[ModuleLayer.Composition])
                createdItems.Add("installer script and asset linked to the manifest");
            if (createScene)
            {
                createdItems.Add("scene");
            }

            if (createManifest)
            {
                createdItems.Add("registered manifest");
            }

            if (createSettings)
            {
                createdItems.Add("unregistered settings class and asset");
            }

            if (isRootModule)
                createdItems.Add("root LifetimeScope and startup entry point (scene wiring completes after compilation)");

            return $"Created {assemblyRoot} at {modulePath}.\n\nIncluded: {string.Join(", ", createdItems)}.";
        }

        private void CreateInstallerScaffold()
        {
            string installerName = $"{SuggestFolderName(assemblyRoot)}Installer";
            string installerTypeName = $"{assemblyRoot}.Composition.{installerName}";
            string scriptPath = $"{GetModuleAssetPath()}/Scripts/Composition/{installerName}.cs";
            string assetFolder = $"{GetModuleAssetPath()}/Data";
            EnsureAssetFolder(assetFolder);
            if (!File.Exists(ToFullPath(scriptPath)))
            {
                string script = string.Join(Environment.NewLine, new[]
                {
                    "using Blackhole.SharedKernel.Composition;",
                    "using UnityEngine;",
                    "using VContainer;",
                    string.Empty,
                    $"namespace {assemblyRoot}.Composition",
                    "{",
                    $"    [CreateAssetMenu(fileName = \"{installerName}\", menuName = \"Blackhole/Modules/Installers/{installerName}\")]",
                    $"    public sealed class {installerName} : ModuleInstallerSO",
                    "    {",
                    "        public override void InstallModule(IContainerBuilder builder)",
                    "        {",
                    "        }",
                    "    }",
                    "}",
                    string.Empty
                });
                File.WriteAllText(ToFullPath(scriptPath), script);
            }

            PendingModuleInstallerAssetCreator.Queue(installerTypeName, $"{assetFolder}/{installerName}.asset", createManifest ? GetManifestAssetPath() : null);
        }

        private void CreateRootModuleScaffold()
        {
            string moduleName = SuggestFolderName(assemblyRoot);
            string scopeName = $"{moduleName}LifetimeScope";
            string entryPointName = $"{moduleName}EntryPoint";
            string compositionPath = $"{GetModuleAssetPath()}/Scripts/Composition";
            string entryPointPath = $"{compositionPath}/{entryPointName}.cs";
            if (!File.Exists(ToFullPath(entryPointPath)))
            {
                string script = string.Join(Environment.NewLine, new[]
                {
                    "using Blackhole.ModuleSystem.Composition;",
                    string.Empty,
                    $"namespace {assemblyRoot}.Composition",
                    "{",
                    $"    public sealed class {entryPointName} : RootModuleStartup",
                    "    {",
                    $"        public {entryPointName}(RootModuleStartupRegistry registry) : base(registry)",
                    "        {",
                    "        }",
                    "    }",
                    "}",
                    string.Empty
                });
                File.WriteAllText(ToFullPath(entryPointPath), script);
            }

            string scopePath = $"{compositionPath}/{scopeName}.cs";
            if (!File.Exists(ToFullPath(scopePath)))
            {
                string script = string.Join(Environment.NewLine, new[]
                {
                    "using VContainer;",
                    "using VContainer.Unity;",
                    string.Empty,
                    $"namespace {assemblyRoot}.Composition",
                    "{",
                    $"    public sealed class {scopeName} : LifetimeScope",
                    "    {",
                    "        protected override void Configure(IContainerBuilder builder)",
                    "        {",
                    $"            builder.RegisterEntryPoint<{entryPointName}>(Lifetime.Singleton);",
                    "        }",
                    "    }",
                    "}",
                    string.Empty
                });
                File.WriteAllText(ToFullPath(scopePath), script);
            }
            else
            {
                string existingSource = File.ReadAllText(ToFullPath(scopePath));
                string updatedSource = RootModuleScopeSource.EnsureEntryPointRegistration(existingSource, $"{assemblyRoot}.Composition.{entryPointName}");
                if (updatedSource != existingSource)
                    File.WriteAllText(ToFullPath(scopePath), updatedSource);
            }

            // Supply the meta before import so VContainer does not replace this script with its empty template.
            string scopeMetaPath = ToFullPath($"{scopePath}.meta");
            if (!File.Exists(scopeMetaPath))
                File.WriteAllText(scopeMetaPath, "fileFormatVersion: 2\n" + $"guid: {GenerateUnityGuid()}\n");

            PendingRootModuleSceneSetup.Queue(GetSceneAssetPath(), $"{assemblyRoot}.Composition.{scopeName}", scopePath, $"{assemblyRoot}.Composition.{entryPointName}");
        }

        private void CreateSettingsScaffold()
        {
            var scriptFolder = $"{GetModuleAssetPath()}/Scripts/{ModuleLayer.Infrastructure}/ScriptableObjects";
            var dataFolder = $"{GetModuleAssetPath()}/Data";
            EnsureAssetFolder(scriptFolder);
            EnsureAssetFolder(dataFolder);

            var scriptPath = GetSettingsScriptAssetPath();
            if (!File.Exists(ToFullPath(scriptPath)))
            {
                File.WriteAllText(ToFullPath(scriptPath), BuildSettingsScript());
            }

            if (AssetDatabase.LoadMainAssetAtPath(GetSettingsAssetPath()) == null)
            {
                PendingModuleSettingsAssetCreator.Queue(GetSettingsTypeName(), GetSettingsAssetPath());
            }
        }

        private string BuildSettingsScript()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "using UnityEngine;",
                string.Empty,
                $"namespace {GetSettingsNamespace()}",
                "{",
                $"    [CreateAssetMenu(fileName = \"{GetSettingsAssetName()}\", menuName = \"Blackhole/Modules/{GetSettingsBaseName()}/Settings\")]",
                $"    public sealed class {GetSettingsClassName()} : ScriptableObject",
                "    {",
                "    }",
                "}",
                string.Empty
            });
        }

        private static void EnsureStarterFolders(string layerPath, ModuleLayer layer)
        {
            switch (layer)
            {
                case ModuleLayer.Contract:
                    EnsureAssetFolder($"{layerPath}/Dto");
                    EnsureAssetFolder($"{layerPath}/Interface");
                    break;
                case ModuleLayer.Domain:
                    EnsureAssetFolder($"{layerPath}/Entity");
                    EnsureAssetFolder($"{layerPath}/ValueObject");
                    break;
                case ModuleLayer.Application:
                    EnsureAssetFolder($"{layerPath}/Ports");
                    EnsureAssetFolder($"{layerPath}/UseCases");
                    break;
                case ModuleLayer.Infrastructure:
                    EnsureAssetFolder($"{layerPath}/Adapters");
                    break;
                case ModuleLayer.Presentation:
                    EnsureAssetFolder($"{layerPath}/Controller");
                    EnsureAssetFolder($"{layerPath}/Presenter");
                    break;
                case ModuleLayer.Composition:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layer), layer, null);
            }
        }

        private static Dictionary<string, AssemblyInfo> FindAssemblyGuidsByName()
        {
            var result = new Dictionary<string, AssemblyInfo>(StringComparer.Ordinal);
            foreach (var guid in AssetDatabase.FindAssets("t:AssemblyDefinitionAsset"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                var text = File.ReadAllText(ToFullPath(path));
                var definition = JsonUtility.FromJson<AssemblyDefinitionNameOnly>(text);
                if (!string.IsNullOrWhiteSpace(definition?.name) && !result.ContainsKey(definition.name))
                {
                    result.Add(definition.name, new AssemblyInfo(guid, path));
                }
            }

            return result;
        }

        private bool CanCreate(out string validationMessage)
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                validationMessage = "Create modules in Edit Mode after compilation and import finish.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(assemblyRoot))
            {
                validationMessage = "Assembly root is required.";
                return false;
            }

            if (assemblyRoot.Any(char.IsWhiteSpace) || assemblyRoot.Split('.').Any(part => string.IsNullOrWhiteSpace(part)))
            {
                validationMessage = "Assembly root must be a dot-separated name without spaces.";
                return false;
            }

            if (assemblyRoot.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                validationMessage = "Assembly root cannot contain invalid file name characters.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(moduleFolderName) || moduleFolderName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                validationMessage = "Folder name is required and cannot contain invalid path characters.";
                return false;
            }

            if (!IsValidAssetFolderPath(modulesPath))
            {
                validationMessage = "Modules path must be inside the Assets folder.";
                return false;
            }

            if (!GetSelectedLayers().Any())
            {
                validationMessage = "Select at least one layer.";
                return false;
            }

            if (createScene &&
                (string.IsNullOrWhiteSpace(sceneName) || sceneName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
            {
                validationMessage = "Scene name is required and cannot contain invalid path characters.";
                return false;
            }

            if (createSettings && !selectedLayers[ModuleLayer.Infrastructure])
            {
                validationMessage = "Create Settings requires the Infrastructure layer.";
                return false;
            }

            if (isRootModule && (!createScene || !createManifest || !selectedLayers[ModuleLayer.Composition]))
            {
                validationMessage = "Root modules require a scene, manifest and Composition layer.";
                return false;
            }

            validationMessage = string.Empty;
            return true;
        }

        private static bool IsValidAssetFolderPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return false;
            }

            var normalized = assetPath.Replace('\\', '/').TrimEnd('/');
            if (Path.IsPathRooted(normalized) ||
                !(normalized.Equals("Assets", StringComparison.Ordinal) || normalized.StartsWith("Assets/", StringComparison.Ordinal)))
            {
                return false;
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            return normalized.Split('/').All(part =>
                !string.IsNullOrWhiteSpace(part) &&
                part != "." &&
                part != ".." &&
                part.IndexOfAny(invalidChars) < 0);
        }

        private string GetModuleAssetPath()
        {
            return $"{modulesPath.TrimEnd('/', '\\')}/{moduleFolderName}".Replace('\\', '/');
        }

        private string GetSceneAssetPath()
        {
            return $"{GetModuleAssetPath()}/{sceneName}.unity";
        }

        private string GetManifestAssetPath()
        {
            return $"{GetModuleAssetPath()}/{assemblyRoot}.asset";
        }

        private string GetSettingsScriptAssetPath()
        {
            return $"{GetModuleAssetPath()}/Scripts/{ModuleLayer.Infrastructure}/ScriptableObjects/{GetSettingsClassName()}.cs";
        }

        private string GetSettingsAssetPath()
        {
            return $"{GetModuleAssetPath()}/Data/{GetSettingsAssetName()}.asset";
        }

        private string GetSettingsNamespace()
        {
            return $"{assemblyRoot}.{ModuleLayer.Infrastructure}";
        }

        private string GetSettingsClassName()
        {
            return $"{GetSettingsBaseName()}SettingsSO";
        }

        private string GetSettingsAssetName()
        {
            return $"{GetSettingsBaseName()}Settings";
        }

        private string GetSettingsTypeName()
        {
            return $"{GetSettingsNamespace()}.{GetSettingsClassName()}";
        }

        private string GetSettingsBaseName()
        {
            return SuggestFolderName(assemblyRoot);
        }

        private IEnumerable<ModuleLayer> GetSelectedLayers()
        {
            return selectedLayers.Where(pair => pair.Value).Select(pair => pair.Key);
        }

        private static string SuggestFolderName(string root)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                return string.Empty;
            }

            var parts = root.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 0 ? root : parts[parts.Length - 1];
        }

        private static void EnsureAssetFolder(string assetPath)
        {
            assetPath = assetPath.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            var parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            var folder = Path.GetFileName(assetPath);
            if (string.IsNullOrWhiteSpace(parent) || string.IsNullOrWhiteSpace(folder))
            {
                throw new InvalidOperationException($"Cannot create asset folder at {assetPath}.");
            }

            EnsureAssetFolder(parent);
            AssetDatabase.CreateFolder(parent, folder);
        }

        private static string GenerateUnityGuid()
        {
            return Guid.NewGuid().ToString("N");
        }

        private static string CreateAsmdefMeta(string guid)
        {
            return "fileFormatVersion: 2\n" +
                   $"guid: {guid}\n" +
                   "AssemblyDefinitionImporter:\n" +
                   "  externalObjects: {}\n" +
                   "  userData: \n" +
                   "  assetBundleName: \n" +
                   "  assetBundleVariant: \n";
        }

        private static string ToFullPath(string assetPath)
        {
            var normalizedAssetPath = assetPath.Replace('\\', '/');
            if (normalizedAssetPath.StartsWith("Packages/", StringComparison.Ordinal))
            {
                var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(normalizedAssetPath);
                if (packageInfo != null && !string.IsNullOrWhiteSpace(packageInfo.resolvedPath))
                {
                    var packagePrefix = $"Packages/{packageInfo.name}";
                    var relativePath = normalizedAssetPath.Substring(packagePrefix.Length).TrimStart('/');
                    return Path.Combine(packageInfo.resolvedPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
                }
            }

            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                throw new InvalidOperationException("Could not resolve Unity project root.");
            }

            return Path.Combine(projectRoot, normalizedAssetPath.Replace('/', Path.DirectorySeparatorChar));
        }

        private enum ModuleLayer
        {
            Contract,
            Domain,
            Application,
            Infrastructure,
            Presentation,
            Composition
        }

        [Serializable]
        private sealed class AssemblyDefinitionNameOnly
        {
            public string name = string.Empty;
        }

        private sealed class AssemblyInfo
        {
            public AssemblyInfo(string guid, string assetPath)
            {
                Guid = guid;
                AssetPath = assetPath;
            }

            public string Guid { get; }
            public string AssetPath { get; }
        }

        [Serializable]
        private sealed class AssemblyDefinition
        {
            public string name;
            public string rootNamespace;
            public string[] references;
            public string[] includePlatforms;
            public string[] excludePlatforms;
            public bool allowUnsafeCode;
            public bool overrideReferences;
            public string[] precompiledReferences;
            public bool autoReferenced;
            public string[] defineConstraints;
            public VersionDefine[] versionDefines;
            public bool noEngineReferences;
        }

        [Serializable]
        private sealed class VersionDefine
        {
            public string name = string.Empty;
            public string expression = string.Empty;
            public string define = string.Empty;
        }
    }

    [Serializable]
    internal sealed class PendingModuleSettingsAssetRequest
    {
        public string TypeName;
        public string AssetPath;
    }

    [InitializeOnLoad]
    internal static class PendingModuleSettingsAssetCreator
    {
        private const string SessionStateKey = "Blackhole.ModuleScaffolder.PendingSettingsAsset";

        static PendingModuleSettingsAssetCreator()
        {
            EditorApplication.delayCall += TryCreatePendingAsset;
        }

        public static void Queue(string typeName, string assetPath)
        {
            var request = new PendingModuleSettingsAssetRequest
            {
                TypeName = typeName,
                AssetPath = assetPath
            };

            SessionState.SetString(SessionStateKey, JsonUtility.ToJson(request));
            EditorApplication.delayCall += TryCreatePendingAsset;
        }

        private static void TryCreatePendingAsset()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            string requestJson = SessionState.GetString(SessionStateKey, string.Empty);
            if (string.IsNullOrWhiteSpace(requestJson))
                return;

            var request = JsonUtility.FromJson<PendingModuleSettingsAssetRequest>(requestJson);
            if (request == null || string.IsNullOrWhiteSpace(request.TypeName) || string.IsNullOrWhiteSpace(request.AssetPath))
            {
                SessionState.EraseString(SessionStateKey);
                Debug.LogError("Could not create the pending module settings asset because its request was invalid.");
                return;
            }

            if (AssetDatabase.LoadMainAssetAtPath(request.AssetPath) != null)
            {
                SessionState.EraseString(SessionStateKey);
                return;
            }

            Type settingsType = TypeCache.GetTypesDerivedFrom<ScriptableObject>().FirstOrDefault(type => type.FullName == request.TypeName);
            if (settingsType == null)
            {
                SessionState.EraseString(SessionStateKey);
                Debug.LogError($"Could not create settings asset '{request.AssetPath}' because type '{request.TypeName}' was not found after compilation.");
                return;
            }

            ScriptableObject settingsAsset = ScriptableObject.CreateInstance(settingsType);
            AssetDatabase.CreateAsset(settingsAsset, request.AssetPath);
            AssetDatabase.SaveAssets();
            SessionState.EraseString(SessionStateKey);
            Debug.Log($"Created unregistered module settings asset at '{request.AssetPath}'.");
        }
    }

    [Serializable]
    internal sealed class PendingModuleInstallerAssetRequest
    {
        public string TypeName;
        public string AssetPath;
        public string ManifestPath;
    }

    [InitializeOnLoad]
    internal static class PendingModuleInstallerAssetCreator
    {
        private const string SessionStateKey = "Blackhole.ModuleScaffolder.PendingInstallerAsset";

        static PendingModuleInstallerAssetCreator()
        {
            EditorApplication.delayCall += TryCreatePendingAsset;
        }

        public static void Queue(string typeName, string assetPath, string manifestPath)
        {
            var request = new PendingModuleInstallerAssetRequest { TypeName = typeName, AssetPath = assetPath, ManifestPath = manifestPath };
            SessionState.SetString(SessionStateKey, JsonUtility.ToJson(request));
            EditorApplication.delayCall += TryCreatePendingAsset;
        }

        private static void TryCreatePendingAsset()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryCreatePendingAsset;
                return;
            }

            string requestJson = SessionState.GetString(SessionStateKey, string.Empty);
            if (string.IsNullOrWhiteSpace(requestJson))
                return;

            var request = JsonUtility.FromJson<PendingModuleInstallerAssetRequest>(requestJson);
            Type installerType = TypeCache.GetTypesDerivedFrom<Blackhole.SharedKernel.Composition.ModuleInstallerSO>().FirstOrDefault(type => type.FullName == request.TypeName);
            if (installerType == null)
            {
                SessionState.EraseString(SessionStateKey);
                Debug.LogError($"Could not create installer '{request.AssetPath}': type '{request.TypeName}' was not found after compilation.");
                return;
            }

            var installer = AssetDatabase.LoadAssetAtPath<Blackhole.SharedKernel.Composition.ModuleInstallerSO>(request.AssetPath);
            if (installer == null)
            {
                installer = (Blackhole.SharedKernel.Composition.ModuleInstallerSO)ScriptableObject.CreateInstance(installerType);
                AssetDatabase.CreateAsset(installer, request.AssetPath);
            }

            if (!string.IsNullOrWhiteSpace(request.ManifestPath))
            {
                ModuleManifestSO manifest = AssetDatabase.LoadAssetAtPath<ModuleManifestSO>(request.ManifestPath);
                if (manifest != null && manifest.Installer == null)
                {
                    SerializedObject serialized = new(manifest);
                    serialized.FindProperty("installer").objectReferenceValue = installer;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(manifest);
                }
            }

            AssetDatabase.SaveAssetIfDirty(installer);
            if (!string.IsNullOrWhiteSpace(request.ManifestPath))
            {
                ModuleManifestSO manifest = AssetDatabase.LoadAssetAtPath<ModuleManifestSO>(request.ManifestPath);
                if (manifest != null)
                    AssetDatabase.SaveAssetIfDirty(manifest);
            }
            SessionState.EraseString(SessionStateKey);
        }
    }
}
