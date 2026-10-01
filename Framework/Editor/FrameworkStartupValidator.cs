using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blackhole.ModuleSystem.Composition;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Blackhole.Framework.Composition.Editor
{
    public sealed class FrameworkStartupValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            IReadOnlyList<string> errors = Validate();
            if (errors.Count > 0)
                throw new BuildFailedException(string.Join(Environment.NewLine, errors));
        }

        public static IReadOnlyList<string> Validate()
        {
            List<string> errors = new();
            BlackholeProjectSettingsSO settings = FindProjectSettings(errors);
            ValidateBuildScenes(settings, errors);
            ValidateFrameworkScene(settings, errors);
            ValidateModuleCatalog(errors);
            return errors;
        }

        private static BlackholeProjectSettingsSO FindProjectSettings(List<string> errors)
        {
            string[] settingsGuids = AssetDatabase.FindAssets($"t:{nameof(BlackholeProjectSettingsSO)}");
            if (settingsGuids.Length != 1)
            {
                errors.Add($"Expected exactly one {nameof(BlackholeProjectSettingsSO)} asset, but found {settingsGuids.Length}.");
                return null;
            }

            string settingsPath = AssetDatabase.GUIDToAssetPath(settingsGuids[0]);
            BlackholeProjectSettingsSO settings = AssetDatabase.LoadAssetAtPath<BlackholeProjectSettingsSO>(settingsPath);
            if (settings == null)
                return null;

            if (settings.ModuleCatalog == null)
                errors.Add("Framework project settings must reference a module catalog.");

            if (string.IsNullOrWhiteSpace(settings.FrameworkScene) || AssetDatabase.LoadAssetAtPath<SceneAsset>(settings.FrameworkScene) == null)
                errors.Add("Framework project settings must reference the framework startup scene. Run Blackhole/Framework/Refresh Module Configuration.");

            if (settings.StartingModule == null || string.IsNullOrWhiteSpace(settings.StartingModule.ModuleID))
            {
                errors.Add("Framework project settings must reference a game starting module.");
                return settings;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(settings.StartingModule.ModuleScene) == null)
                errors.Add($"Starting module '{settings.StartingModule.ModuleID}' must reference an existing scene.");

            if (!settings.StartingModule.IsPersistant)
                errors.Add($"Starting module '{settings.StartingModule.ModuleID}' must be persistent so game roots can load beneath it.");

            if (settings.StartingModule.ModuleID == FrameworkLifetimeScope.FrameworkModuleId)
                errors.Add("The game starting module must differ from the framework root module.");

            if (settings.ModuleCatalog != null)
            {
                ValidateCatalogModule(settings.ModuleCatalog, FrameworkLifetimeScope.FrameworkModuleId, settings.FrameworkScene, errors);
                ValidateCatalogModule(settings.ModuleCatalog, settings.StartingModule.ModuleID, settings.FrameworkScene, errors);
                if (TryGetCatalogModule(settings.ModuleCatalog, settings.StartingModule.ModuleID) is ModuleManifestSO catalogModule && catalogModule != settings.StartingModule)
                    errors.Add("The configured starting module must be the manifest registered in the module catalog.");
            }

            return settings;
        }

        private static void ValidateBuildScenes(BlackholeProjectSettingsSO settings, List<string> errors)
        {
            EditorBuildSettingsScene[] enabledScenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).ToArray();
            if (settings != null && (enabledScenes.Length == 0 || enabledScenes[0].path != settings.FrameworkScene))
                errors.Add($"'{settings.FrameworkScene}' must be the first enabled scene in Build Settings.");

            if (settings != null && settings.StartingModule != null && (enabledScenes.Length < 2 || enabledScenes[1].path != settings.StartingModule.ModuleScene))
                errors.Add($"Starting module scene '{settings.StartingModule.ModuleScene}' must be the second enabled scene in Build Settings.");

            string[] duplicateNames = enabledScenes.GroupBy(scene => Path.GetFileNameWithoutExtension(scene.path), StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();
            if (duplicateNames.Length > 0)
                errors.Add($"Enabled build scenes must have unique names. Duplicates: {string.Join(", ", duplicateNames)}.");
        }

        private static void ValidateFrameworkScene(BlackholeProjectSettingsSO settings, List<string> errors)
        {
            if (settings == null || AssetDatabase.LoadAssetAtPath<SceneAsset>(settings.FrameworkScene) == null)
                return;

            var scene = EditorSceneManager.OpenPreviewScene(settings.FrameworkScene);
            try
            {
                FrameworkLifetimeScope[] scopes = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<FrameworkLifetimeScope>(true)).ToArray();
                if (scopes.Length != 1)
                {
                    errors.Add("The framework scene must contain exactly one FrameworkLifetimeScope.");
                    return;
                }

                SerializedObject serializedScope = new(scopes[0]);
                if (serializedScope.FindProperty("projectSettings").objectReferenceValue != settings)
                    errors.Add("The FrameworkLifetimeScope must directly reference the project's BlackholeProjectSettingsSO asset.");
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        private static void ValidateModuleCatalog(List<string> errors)
        {
            string[] catalogGuids = AssetDatabase.FindAssets($"t:{nameof(ModuleManifestCatalogSO)}");
            if (catalogGuids.Length != 1)
            {
                errors.Add($"Expected exactly one {nameof(ModuleManifestCatalogSO)} asset, but found {catalogGuids.Length}.");
                return;
            }

            ModuleManifestCatalogSO catalog = AssetDatabase.LoadAssetAtPath<ModuleManifestCatalogSO>(AssetDatabase.GUIDToAssetPath(catalogGuids[0]));
            HashSet<string> moduleIds = new(StringComparer.Ordinal);
            foreach (ModuleManifestSO manifest in catalog.ModuleManifests)
            {
                if (manifest == null)
                {
                    errors.Add("The module catalog contains a missing manifest reference.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(manifest.ModuleID))
                    errors.Add($"Module manifest '{manifest.name}' has an empty module ID.");
                else if (!moduleIds.Add(manifest.ModuleID))
                    errors.Add($"Multiple module manifests have ID '{manifest.ModuleID}'.");

                if (manifest.Installer == null)
                    errors.Add($"Module '{manifest.ModuleID}' has no installer assigned in its manifest.");

                if (manifest.Dependencies != null && manifest.Dependencies.Any(dependency => dependency == null))
                    errors.Add($"Module '{manifest.ModuleID}' has a missing dependency reference. Reassign it in the manifest.");
            }
        }

        private static void ValidateCatalogModule(ModuleManifestCatalogSO catalog, string moduleId, string frameworkScene, List<string> errors)
        {
            try
            {
                ModuleManifestSO manifest = catalog.GetModule(moduleId);
                if (moduleId == FrameworkLifetimeScope.FrameworkModuleId && manifest.ModuleScene != frameworkScene)
                    errors.Add($"Framework root manifest must reference '{frameworkScene}'.");
            }
            catch (InvalidOperationException exception)
            {
                errors.Add(exception.Message);
            }
        }

        private static ModuleManifestSO TryGetCatalogModule(ModuleManifestCatalogSO catalog, string moduleId)
        {
            try
            {
                return catalog.GetModule(moduleId);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
