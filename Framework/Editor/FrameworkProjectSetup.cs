using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blackhole.ModuleSystem.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blackhole.Framework.Composition.Editor
{
    /// <summary>Refreshes the catalog and build scenes without copying manifests, installers or settings.</summary>
    public static class FrameworkProjectSetup
    {
        public const string PackageRoot = "Packages/com.bigbang.blackhole-framework";
        public const string SettingsPath = PackageRoot + "/Framework/BlackholeProjectSettings.asset";
        public const string CatalogPath = PackageRoot + "/ModuleSystem/ModuleManifestCatalog.asset";

        [MenuItem("Blackhole/Framework/Refresh Module Configuration")]
        public static void RefreshFromMenu()
        {
            BlackholeProjectSettingsSO settings = RegenerateSettings();
            Selection.activeObject = settings;
            Debug.Log("Blackhole module configuration refreshed. No module assets were copied.", settings);
        }

        /// <summary>Rediscovers modules and refreshes wiring while preserving configured settings.</summary>
        public static BlackholeProjectSettingsSO RegenerateSettings()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Refresh module configuration in Edit Mode after compilation and import finish.");

            BlackholeProjectSettingsSO settings = AssetDatabase.LoadAssetAtPath<BlackholeProjectSettingsSO>(SettingsPath);
            ModuleManifestCatalogSO catalog = AssetDatabase.LoadAssetAtPath<ModuleManifestCatalogSO>(CatalogPath);
            if (settings == null || catalog == null)
                throw new InvalidOperationException("The framework package must contain its project settings and module catalog assets.");

            RefreshCatalog(catalog);
            SerializedObject serialized = new(settings);
            serialized.FindProperty("moduleCatalog").objectReferenceValue = catalog;
            serialized.FindProperty("frameworkScene").stringValue = FrameworkLifetimeScope.FrameworkScenePath;
            Save(serialized);
            RefreshFrameworkScene(settings);
            RefreshBuildScenes(settings, catalog);
            return settings;
        }

        public static void RefreshCatalog(ModuleManifestCatalogSO catalog)
        {
            ModuleManifestSO[] modules = AssetDatabase.FindAssets($"t:{nameof(ModuleManifestSO)}").Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ModuleManifestSO>).Where(module => module != null)
                .OrderBy(module => module.ModuleID, StringComparer.Ordinal).ToArray();
            string duplicate = modules.GroupBy(module => module.ModuleID, StringComparer.Ordinal).FirstOrDefault(group => group.Count() > 1)?.Key;
            if (duplicate != null) throw new InvalidOperationException($"Duplicate module ID '{duplicate}'. Keep one manifest per module.");

            SerializedObject serialized = new(catalog);
            SerializedProperty list = serialized.FindProperty("moduleManifests");
            list.arraySize = modules.Length;
            for (int i = 0; i < modules.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = modules[i];
            Save(serialized);
        }

        private static void RefreshBuildScenes(BlackholeProjectSettingsSO settings, ModuleManifestCatalogSO catalog)
        {
            string[] paths = AssetDatabase.FindAssets("t:Scene").Select(AssetDatabase.GUIDToAssetPath).ToArray();
            string ScenePath(ModuleManifestSO module)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(module.ModuleScene) != null) return module.ModuleScene;
                string[] matches = paths.Where(path => Path.GetFileNameWithoutExtension(path) == module.ModuleScene).ToArray();
                if (matches.Length != 1) throw new InvalidOperationException($"Module '{module.ModuleID}' must reference one existing, unambiguous scene.");
                return matches[0];
            }

            List<string> first = new() { FrameworkLifetimeScope.FrameworkScenePath };
            if (settings.StartingModule != null && !string.IsNullOrWhiteSpace(settings.StartingModule.ModuleScene))
                first.Add(ScenePath(settings.StartingModule));
            string[] modules = catalog.ModuleManifests.Where(module => !string.IsNullOrWhiteSpace(module.ModuleScene)).Select(ScenePath).ToArray();
            EditorBuildSettingsScene[] scenes = first.Select(path => new EditorBuildSettingsScene(path, true))
                .Concat(EditorBuildSettings.scenes.Where(scene => AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path) != null && !first.Contains(scene.path)))
                .Concat(modules.Where(path => !first.Contains(path)).Select(path => new EditorBuildSettingsScene(path, true)))
                .GroupBy(scene => scene.path).Select(group => group.First()).ToArray();
            if (!EditorBuildSettings.scenes.Select(scene => (scene.path, scene.enabled)).SequenceEqual(scenes.Select(scene => (scene.path, scene.enabled))))
                EditorBuildSettings.scenes = scenes;
        }

        private static void RefreshFrameworkScene(BlackholeProjectSettingsSO settings)
        {
            Scene scene = SceneManager.GetSceneByPath(settings.FrameworkScene);
            bool wasOpen = scene.IsValid() && scene.isLoaded;
            if (wasOpen && scene.isDirty) return;
            if (!wasOpen) scene = EditorSceneManager.OpenScene(settings.FrameworkScene, OpenSceneMode.Additive);
            try
            {
                FrameworkLifetimeScope scope = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<FrameworkLifetimeScope>(true)).Single();
                SerializedObject serialized = new(scope);
                serialized.FindProperty("projectSettings").objectReferenceValue = settings;
                if (!serialized.ApplyModifiedPropertiesWithoutUndo()) return;
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            finally
            {
                if (!wasOpen) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void Save(SerializedObject serialized)
        {
            if (!serialized.ApplyModifiedPropertiesWithoutUndo()) return;
            EditorUtility.SetDirty(serialized.targetObject);
            AssetDatabase.SaveAssetIfDirty(serialized.targetObject);
        }
    }
}
