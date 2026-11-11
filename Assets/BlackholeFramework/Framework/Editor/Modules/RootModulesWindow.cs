using Blackhole.ModuleSystem.Composition;
using Blackhole.ModuleSystem.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Blackhole.Framework.Composition.Editor
{
    public sealed class RootModulesWindow : EditorWindow
    {
        [SerializeField] private ModuleManifestCatalogSO catalog;
        [SerializeField] private string outputFolder = "Assets/Game/Modules/GameFlow";
        [SerializeField] private string outputNamespace = "Blackhole.GameFlow";
        private readonly List<ModuleInfo> modules = new List<ModuleInfo>();
        private Vector2 scrollPosition;
        private bool hasScanned;

        [MenuItem("Blackhole/Modules/Inspect Root Modules")]
        public static void Open()
        {
            GetWindow<RootModulesWindow>("Root Modules").Show();
        }

        private void OnEnable()
        {
            if (catalog == null)
                catalog = AssetDatabase.LoadAssetAtPath<ModuleManifestCatalogSO>(
                    "Assets/BlackholeFramework/ModuleSystem/ModuleManifestCatalog.asset");
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            catalog = (ModuleManifestCatalogSO)EditorGUILayout.ObjectField(
                "Module Catalog", catalog, typeof(ModuleManifestCatalogSO), false);
            if (EditorGUI.EndChangeCheck())
            {
                modules.Clear();
                hasScanned = false;
            }

            outputFolder = EditorGUILayout.TextField("GameFlow Module Folder", outputFolder);
            outputNamespace = EditorGUILayout.TextField("GameFlow Namespace", outputNamespace);
            EditorGUILayout.HelpBox("Generate into an existing scaffolded game module. Handwritten files are preserved; generated files are updated.", MessageType.Info);

            using (new EditorGUI.DisabledScope(catalog == null || EditorApplication.isCompiling ||
                                               EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (GUILayout.Button("Read Root Modules")) ReadModules();
                if (GUILayout.Button("Generate Game Flows")) GenerateGameFlows();
            }

            if (!hasScanned) return;

            EditorGUILayout.HelpBox(
                "Only catalog modules marked Is Root Module are listed. A root entry point is a compiled class implementing IRootModuleStartup in the module folder. This checks whether the class exists, not whether it is registered.",
                MessageType.Info);
            if (modules.Count == 0)
                EditorGUILayout.LabelField("No modules marked Is Root Module found in this catalog.");

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (ModuleInfo module in modules)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(module.Manifest.ModuleID, EditorStyles.boldLabel);
                    EditorGUILayout.ObjectField("Manifest", module.Manifest, typeof(ModuleManifestSO), false);
                    EditorGUILayout.LabelField("Scene", string.IsNullOrWhiteSpace(module.Manifest.ModuleScene) ? "Not assigned" : module.Manifest.ModuleScene);
                    EditorGUILayout.ObjectField("Scene Asset", module.Scene, typeof(SceneAsset), false);
                    if (module.Scene == null)
                        EditorGUILayout.LabelField("Scene Status", "Missing or ambiguous scene reference");
                    EditorGUILayout.LabelField("Root Entry Point", module.EntryPoints.Length == 0 ? "Not found" : "Found");
                    foreach (Type entryPoint in module.EntryPoints)
                        EditorGUILayout.LabelField(entryPoint.FullName, EditorStyles.wordWrappedLabel);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void ReadModules()
        {
            modules.Clear();
            string[] scenePaths = AssetDatabase.FindAssets("t:Scene")
                .Select(AssetDatabase.GUIDToAssetPath).ToArray();

            foreach (ModuleManifestSO manifest in catalog.ModuleManifests
                         .Where(module => module != null && module.IsRootModule)
                         .Distinct().OrderBy(module => module.ModuleID, StringComparer.Ordinal))
            {
                string sceneReference = (manifest.ModuleScene ?? string.Empty).Replace('\\', '/');
                SceneAsset scene = string.IsNullOrWhiteSpace(sceneReference) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(sceneReference);
                if (scene == null)
                {
                    string[] matches = scenePaths.Where(path => Path.GetFileNameWithoutExtension(path) == sceneReference).ToArray();
                    if (matches.Length == 1) scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(matches[0]);
                }

                string manifestPath = AssetDatabase.GetAssetPath(manifest);
                string folder = string.IsNullOrEmpty(manifestPath) ? null : Path.GetDirectoryName(manifestPath)?.Replace('\\', '/');
                Type[] entryPoints = string.IsNullOrEmpty(folder) ? Array.Empty<Type>() : AssetDatabase
                    .FindAssets("t:MonoScript", new[] { folder })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
                    .Where(script => script != null)
                    .Select(script => script.GetClass())
                    .Where(type => type != null && type.IsClass && !type.IsAbstract && !type.ContainsGenericParameters &&
                                   typeof(IRootModuleStartup).IsAssignableFrom(type))
                    .Distinct().OrderBy(type => type.FullName, StringComparer.Ordinal).ToArray();

                modules.Add(new ModuleInfo { Manifest = manifest, Scene = scene, EntryPoints = entryPoints });
            }
            hasScanned = true;
        }

        private void GenerateGameFlows()
        {
            try
            {
                // Read again so generation includes catalog changes made since the last scan.
                ReadModules();
                string[] moduleIds = GameFlowSourceGenerator.ValidateModuleIds(
                    modules.Select(module => module.Manifest.ModuleID));

                string fullFolder = Path.GetFullPath(outputFolder ?? string.Empty).Replace('\\', '/').TrimEnd('/');
                string assets = Path.GetFullPath("Assets").Replace('\\', '/') + "/";
                string framework = Path.GetFullPath(FrameworkProjectSetup.PackageRoot).Replace('\\', '/') + "/";
                if (!fullFolder.StartsWith(assets, StringComparison.OrdinalIgnoreCase) || (fullFolder + "/").StartsWith(framework, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Choose a game module folder under Assets, outside BlackholeFramework.");
                string folder = "Assets/" + fullFolder.Substring(assets.Length);
                GameFlowSourceGenerator.ValidateNamespace(outputNamespace);
                foreach (string layer in new[] { "Contract", "Application", "Composition" })
                {
                    if (!Directory.Exists(folder + "/Scripts/" + layer) || Directory.GetFiles(folder + "/Scripts/" + layer, "*.asmdef").Length != 1)
                        throw new InvalidOperationException("Scaffold the GameFlow module's Contract, Application and Composition layers first.");
                }
                string scriptsFolder = folder + "/Scripts/";
                string installerName = outputNamespace.Substring(outputNamespace.LastIndexOf('.') + 1) + "Installer";
                string installerPath = scriptsFolder + "Composition/" + installerName + ".cs";
                string optionsPath = scriptsFolder + "Contract/Dto/RootModuleEntryOption.cs";

                var outputs = new Dictionary<string, string>();
                var moves = new Dictionary<string, string>();
                AddGeneratedSource(outputs, moves, scriptsFolder + "Contract/Interface/IGameFlow", GameFlowSourceGenerator.BuildGameFlow(moduleIds, outputNamespace));
                AddGeneratedSource(outputs, moves, scriptsFolder + "Application/UseCases/ILoadScene", GameFlowSourceGenerator.BuildLoadScene(moduleIds, outputNamespace));
                AddGeneratedSource(outputs, moves, scriptsFolder + "Application/UseCases/LoadGameUsecase", GameFlowSourceGenerator.BuildUsecase(moduleIds, outputNamespace));
                AddInstallerDeclaration(outputs, installerPath, installerName);
                string registrationPath = scriptsFolder + "Composition/" + installerName + ".g.cs";
                RequireGeneratedSource(registrationPath);
                outputs[registrationPath] = GameFlowSourceGenerator.BuildInstallerRegistration(outputNamespace, installerName);
                if (!File.Exists(optionsPath))
                    outputs[optionsPath] = $"namespace {outputNamespace}.Contract\n{{\n    public abstract class RootModuleEntryOption\n    {{\n    }}\n}}\n";

                AddAssemblyReferences(outputs, scriptsFolder, "Contract", new[] { "Blackhole.SharedKernel.Contract", "UniTask" });
                AddAssemblyReferences(outputs, scriptsFolder, "Application", new[] { outputNamespace + ".Contract", "Blackhole.SharedKernel.Contract", "Blackhole.ModuleSystem.Contract", "Blackhole.LoadingScreen.Contract", "UniTask" });
                AddAssemblyReferences(outputs, scriptsFolder, "Composition", new[] { outputNamespace + ".Application", "Blackhole.SharedKernel.Composition", "VContainer" });

                bool changed = WriteSources(outputs, moves);
                Debug.Log($"GameFlow updated for {moduleIds.Length} root modules. " +
                          (changed ? "Navigation and installer registration generated." : "All files are already current."));
            }
            catch (Exception exception)
            {
                Debug.LogError($"Could not generate GameFlows: {exception.Message}");
            }
        }

        private static void AddGeneratedSource(Dictionary<string, string> outputs, Dictionary<string, string> moves, string basePath, string source)
        {
            string legacyPath = basePath + ".cs";
            string generatedPath = basePath + ".g.cs";
            RequireGeneratedSource(generatedPath);
            if (File.Exists(legacyPath))
            {
                RequireGeneratedSource(legacyPath);
                if (File.Exists(generatedPath))
                    throw new InvalidOperationException($"Both '{legacyPath}' and '{generatedPath}' exist. Keep one before generating.");
                moves.Add(legacyPath, generatedPath);
            }
            outputs[generatedPath] = source;
        }

        private void AddInstallerDeclaration(Dictionary<string, string> outputs, string path, string installerName)
        {
            string declaration = GameFlowSourceGenerator.BuildInstallerDeclaration(outputNamespace, installerName);
            if (File.Exists(path))
            {
                string existing = File.ReadAllText(path);
                string scaffold = declaration.Replace("using UnityEngine;\n", "using UnityEngine;\nusing VContainer;\n")
                    .Replace("sealed partial class", "sealed class")
                    .Replace("    {\n    }", "    {\n        public override void InstallModule(IContainerBuilder builder)\n        {\n        }\n    }");
                if (!IsGeneratedSource(existing) && RemoveWhitespace(existing) != RemoveWhitespace(scaffold))
                {
                    if (Regex.IsMatch(existing, $@"\bpartial\s+class\s+{Regex.Escape(installerName)}\b") &&
                        !Regex.IsMatch(existing, @"\boverride\s+void\s+InstallModule\s*\("))
                        return;

                    throw new InvalidOperationException($"'{path}' contains a handwritten installer. Use a partial declaration and move its registration code before generating.");
                }
            }

            outputs[path] = declaration;
        }

        private static void RequireGeneratedSource(string path)
        {
            if (File.Exists(path) && !IsGeneratedSource(File.ReadAllText(path)))
                throw new InvalidOperationException($"'{path}' contains handwritten code. Move or merge it before generating.");
        }

        private static bool IsGeneratedSource(string source) => source.StartsWith("// <auto-generated />", StringComparison.Ordinal);

        private static bool WriteSources(Dictionary<string, string> outputs, Dictionary<string, string> moves)
        {
            var changes = outputs.Where(output => !File.Exists(output.Key) || File.ReadAllText(output.Key) != output.Value).ToArray();
            if (changes.Length == 0)
                return false;

            var sources = moves.ToDictionary(move => move.Value, move => move.Key);
            var originals = new Dictionary<string, byte[]>();
            foreach (var change in changes)
            {
                string path = sources.TryGetValue(change.Key, out var original) ? original : change.Key;
                originals.Add(change.Key, File.Exists(path) ? File.ReadAllBytes(path) : null);
            }

            var moved = new Stack<KeyValuePair<string, string>>();
            var written = new Stack<string>();
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var move in moves)
                {
                    string error = AssetDatabase.MoveAsset(move.Key, move.Value);
                    if (!string.IsNullOrEmpty(error))
                        throw new IOException(error);
                    moved.Push(move);
                }

                var encoding = new UTF8Encoding(false);
                foreach (var change in changes)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(change.Key));
                    written.Push(change.Key);
                    File.WriteAllText(change.Key, change.Value, encoding);
                }
            }
            catch
            {
                foreach (string path in written)
                {
                    if (originals[path] == null)
                        File.Delete(path);
                    else
                        File.WriteAllBytes(path, originals[path]);
                }
                foreach (var move in moved)
                    AssetDatabase.MoveAsset(move.Value, move.Key);
                throw;
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
            return true;
        }

        private static string RemoveWhitespace(string source) => new string(source.Where(character => !char.IsWhiteSpace(character)).ToArray());

        private void AddAssemblyReferences(Dictionary<string, string> outputs, string scriptsFolder, string layer, string[] required)
        {
            string path = Directory.GetFiles(scriptsFolder + layer, "*.asmdef").Single();
            JObject definition = JObject.Parse(File.ReadAllText(path));
            if ((string)definition["name"] != outputNamespace + "." + layer)
                throw new InvalidOperationException($"The {layer} assembly must be named '{outputNamespace}.{layer}'.");
            JArray references = definition["references"] as JArray ?? new JArray();
            foreach (string name in required)
            {
                string referencedPath = UnityEditor.Compilation.CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(name);
                if (string.IsNullOrEmpty(referencedPath)) throw new InvalidOperationException($"Required assembly '{name}' was not found.");
                string guid = "GUID:" + AssetDatabase.AssetPathToGUID(referencedPath);
                if (!references.Any(reference => (string)reference == name || (string)reference == guid)) references.Add(guid);
            }
            definition["references"] = references;
            outputs[path] = definition.ToString() + "\n";
        }

        private sealed class ModuleInfo
        {
            internal ModuleManifestSO Manifest;
            internal SceneAsset Scene;
            internal Type[] EntryPoints;
        }
    }
}
