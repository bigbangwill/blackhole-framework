using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blackhole.ModuleSystem.Composition;
using Blackhole.Framework.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Blackhole.Editor
{
    [InitializeOnLoad]
    internal static class PendingIsolatedModuleSceneSetup
    {
        private const string SessionStateKey = "Blackhole.ModuleScaffolder.PendingIsolatedScenes";
        private static double nextAttempt;

        static PendingIsolatedModuleSceneSetup()
        {
            EditorApplication.update += TryCompletePendingScenes;
        }

        internal static void Queue(string scenePath, string scriptPath, string manifestPath, string catalogPath, string generatedSource)
        {
            PendingScenes pending = ReadPendingScenes();
            pending.Requests.RemoveAll(request => request.ScenePath == scenePath);
            pending.Requests.Add(new SceneRequest
            {
                ScenePath = scenePath, ScriptPath = scriptPath, ManifestPath = manifestPath, CatalogPath = catalogPath, GeneratedSource = generatedSource
            });
            SessionState.SetString(SessionStateKey, JsonUtility.ToJson(pending));
        }

        private static PendingScenes ReadPendingScenes()
        {
            string json = SessionState.GetString(SessionStateKey, string.Empty);
            return string.IsNullOrWhiteSpace(json) ? new PendingScenes() : JsonUtility.FromJson<PendingScenes>(json);
        }

        private static void TryCompletePendingScenes()
        {
            if (EditorApplication.timeSinceStartup < nextAttempt || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            nextAttempt = EditorApplication.timeSinceStartup + 1;
            PendingScenes pending = ReadPendingScenes();
            foreach (SceneRequest request in pending.Requests.ToArray())
            {
                try
                {
                    string source = File.ReadAllText(request.ScriptPath);
                    if (!string.IsNullOrEmpty(request.GeneratedSource) && IsolatedModuleScopeSource.IsEmptyLifetimeScopeTemplate(source))
                    {
                        File.WriteAllText(request.ScriptPath, request.GeneratedSource);
                        AssetDatabase.ImportAsset(request.ScriptPath, ImportAssetOptions.ForceUpdate);
                        return;
                    }

                    Type scopeType = AssetDatabase.LoadAssetAtPath<MonoScript>(request.ScriptPath)?.GetClass();
                    ModuleManifestSO manifest = AssetDatabase.LoadAssetAtPath<ModuleManifestSO>(request.ManifestPath);
                    ModuleManifestCatalogSO catalog = AssetDatabase.LoadAssetAtPath<ModuleManifestCatalogSO>(request.CatalogPath);
                    if (scopeType == null || !typeof(LifetimeScope).IsAssignableFrom(scopeType) || manifest == null || manifest.Installer == null || catalog == null)
                        continue;

                    Scene openScene = SceneManager.GetSceneByPath(request.ScenePath);
                    if (openScene.IsValid() && openScene.isLoaded && openScene.isDirty)
                        continue;

                    SetupScene(request.ScenePath, scopeType, manifest, catalog);
                    pending.Requests.Remove(request);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"Could not set up isolated module scene '{request.ScenePath}': {exception.Message}");
                    pending.Requests.Remove(request);
                }
            }

            if (pending.Requests.Count == 0)
                SessionState.EraseString(SessionStateKey);
            else
                SessionState.SetString(SessionStateKey, JsonUtility.ToJson(pending));
        }

        internal static void SetupScene(string scenePath, Type scopeType, ModuleManifestSO manifest, ModuleManifestCatalogSO catalog)
        {
            PendingRootModuleSceneSetup.SetupScene(scenePath, scopeType, scope =>
            {
                SerializedObject serialized = new(scope);
                serialized.FindProperty("_catalog").objectReferenceValue = catalog;
                serialized.FindProperty("_module").objectReferenceValue = manifest;
                serialized.FindProperty("autoRun").boolValue = true;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                Scene scene = scope.gameObject.scene;
                if (!scene.GetRootGameObjects().Any(root => root.GetComponentInChildren<EventSystem>(true) != null))
                {
                    Scene framework = EditorSceneManager.OpenPreviewScene(FrameworkLifetimeScope.FrameworkScenePath);
                    try
                    {
                        EventSystem template = framework.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<EventSystem>(true)).Single();
                        GameObject eventSystem = UnityEngine.Object.Instantiate(template.gameObject);
                        eventSystem.name = "Event System";
                        SceneManager.MoveGameObjectToScene(eventSystem, scene);
                    }
                    finally
                    {
                        EditorSceneManager.ClosePreviewScene(framework);
                    }
                }
                if (scene.GetRootGameObjects().Any(root => root.GetComponentInChildren<Camera>(true) != null))
                    return;

                GameObject cameraObject = new("Main Camera");
                SceneManager.MoveGameObjectToScene(cameraObject, scene);
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0, 0, -10);
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 5;
                cameraObject.AddComponent<AudioListener>();
            });
        }

        [Serializable]
        private sealed class PendingScenes
        {
            public List<SceneRequest> Requests = new();
        }

        [Serializable]
        private sealed class SceneRequest
        {
            public string ScenePath;
            public string ScriptPath;
            public string ManifestPath;
            public string CatalogPath;
            public string GeneratedSource;
        }
    }
}
