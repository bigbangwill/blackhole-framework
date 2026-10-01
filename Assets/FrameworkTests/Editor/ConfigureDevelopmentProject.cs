using System;
using System.IO;
using System.Linq;
using Blackhole.Development;
using Blackhole.Framework.Composition;
using Blackhole.Framework.Composition.Editor;
using Blackhole.ModuleSystem.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Blackhole.Development.Editor
{
    public static class ConfigureDevelopmentProject
    {
        private const string SmokeRoot = "Assets/FrameworkTests/Smoke";
        public const string SmokeScene = SmokeRoot + "/FrameworkSmokeScene.unity";

        [MenuItem("Blackhole/Development/Configure Smoke Project")]
        public static void Configure()
        {
            AssetDatabase.Refresh();
            SmokeInstaller installer = LoadOrCreate<SmokeInstaller>(SmokeRoot + "/SmokeInstaller.asset");
            ModuleManifestSO manifest = LoadOrCreate<ModuleManifestSO>(SmokeRoot + "/Blackhole.Development.Smoke.asset");
            SerializedObject module = new(manifest);
            module.FindProperty("moduleID").stringValue = "Blackhole.Development.Smoke";
            module.FindProperty("installer").objectReferenceValue = installer;
            module.FindProperty("moduleScene").stringValue = SmokeScene;
            module.FindProperty("isPersistant").boolValue = true;
            SerializedProperty dependencies = module.FindProperty("dependencies");
            dependencies.arraySize = 1;
            dependencies.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<ModuleManifestSO>("Assets/BlackholeFramework/AudioPlayer/Blackhole.AudioPlayer.asset");
            module.ApplyModifiedPropertiesWithoutUndo();

            if (!File.Exists(SmokeScene))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                new GameObject("Framework Smoke Scope").AddComponent<SmokeSceneLifetimeScope>();
                Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
                camera.tag = "MainCamera";
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.04f, 0.05f, 0.09f);
                EditorSceneManager.SaveScene(scene, SmokeScene);
            }

            BlackholeProjectSettingsSO settings = AssetDatabase.LoadAssetAtPath<BlackholeProjectSettingsSO>(FrameworkProjectSetup.SettingsPath);
            SerializedObject project = new(settings);
            project.FindProperty("startingModule").objectReferenceValue = manifest;
            project.ApplyModifiedPropertiesWithoutUndo();
            SerializedObject playerSettings = new(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset").First());
            playerSettings.FindProperty("activeInputHandler").intValue = 2;
            playerSettings.ApplyModifiedPropertiesWithoutUndo();
            PlayerSettings.companyName = "BigBang";
            PlayerSettings.productName = "Blackhole Framework";
            AssetDatabase.SaveAssets();
            FrameworkProjectSetup.RegenerateSettings();
            var errors = FrameworkStartupValidator.Validate();
            if (errors.Count > 0) throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
            EditorSceneManager.OpenScene(FrameworkLifetimeScope.FrameworkScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("Blackhole development project configured and startup validation passed.");
        }

        public static void ConfigureBatch()
        {
            try { Configure(); EditorApplication.Exit(0); }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        private static T LoadOrCreate<T>(string assetPath) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }
    }
}
