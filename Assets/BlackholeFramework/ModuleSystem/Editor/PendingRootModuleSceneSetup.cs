using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Blackhole.Editor
{
    [InitializeOnLoad]
    internal static class PendingRootModuleSceneSetup
    {
        private const string SessionStateKey = "Blackhole.ModuleScaffolder.PendingRootScenes";
        private static double nextAttempt;

        static PendingRootModuleSceneSetup()
        {
            EditorApplication.update += TryCompletePendingScenes;
        }

        public static void Queue(string scenePath, string scopeTypeName, string scopeScriptPath, string entryPointTypeName)
        {
            PendingScenes pending = ReadPendingScenes();
            pending.Requests.RemoveAll(request => request.ScenePath == scenePath);
            pending.Requests.Add(new SceneRequest { ScenePath = scenePath, ScopeTypeName = scopeTypeName, ScopeScriptPath = scopeScriptPath, EntryPointTypeName = entryPointTypeName });
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
            if (pending.Requests.Count == 0)
                return;

            foreach (SceneRequest request in pending.Requests.ToArray())
            {
                string scriptPath = string.IsNullOrWhiteSpace(request.ScopeScriptPath)
                    ? Path.GetDirectoryName(request.ScenePath)?.Replace('\\', '/') + "/Scripts/Composition/" + request.ScopeTypeName.Substring(request.ScopeTypeName.LastIndexOf('.') + 1) + ".cs"
                    : request.ScopeScriptPath;
                string entryPointTypeName = string.IsNullOrWhiteSpace(request.EntryPointTypeName)
                    ? request.ScopeTypeName.Substring(0, request.ScopeTypeName.Length - "LifetimeScope".Length) + "EntryPoint"
                    : request.EntryPointTypeName;

                try
                {
                    string source = File.ReadAllText(scriptPath);
                    string registeredSource = RootModuleScopeSource.EnsureEntryPointRegistration(source, entryPointTypeName);
                    if (source != registeredSource)
                    {
                        File.WriteAllText(scriptPath, registeredSource);
                        AssetDatabase.ImportAsset(scriptPath, ImportAssetOptions.ForceUpdate);
                        // Keep the request across recompilation; attach the scope only after the repaired script compiles.
                        return;
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogError($"Could not register root entry point in '{scriptPath}': {exception.Message}");
                    pending.Requests.Remove(request);
                    continue;
                }

                Type scopeType = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath)?.GetClass();
                if (scopeType == null || !typeof(LifetimeScope).IsAssignableFrom(scopeType))
                    scopeType = TypeCache.GetTypesDerivedFrom<LifetimeScope>().FirstOrDefault(type => type.FullName == request.ScopeTypeName);
                if (scopeType == null)
                    continue;

                Scene openScene = SceneManager.GetSceneByPath(request.ScenePath);
                if (openScene.IsValid() && openScene.isLoaded && openScene.isDirty)
                    continue;

                try
                {
                    SetupScene(request.ScenePath, scopeType);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"Could not set up root module scene '{request.ScenePath}': {exception.Message}");
                }

                pending.Requests.Remove(request);
            }

            if (pending.Requests.Count == 0)
                SessionState.EraseString(SessionStateKey);
            else
                SessionState.SetString(SessionStateKey, JsonUtility.ToJson(pending));
        }

        internal static void SetupScene(string scenePath, Type scopeType)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
                throw new InvalidOperationException("The module scene does not exist.");

            Scene previousActiveScene = SceneManager.GetActiveScene();
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool wasOpen = scene.IsValid() && scene.isLoaded;
            if (!wasOpen)
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                LifetimeScope[] scopes = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<LifetimeScope>(true)).ToArray();
                if (scopes.Length == 1 && scopes[0].GetType() == scopeType)
                    return;

                if (scopes.Length > 0)
                    throw new InvalidOperationException("The scene already has a different LifetimeScope. Replace it with the generated root LifetimeScope before retrying.");

                SceneManager.SetActiveScene(scene);
                GameObject root = new(scopeType.Name);
                SceneManager.MoveGameObjectToScene(root, scene);
                root.AddComponent(scopeType);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("The module scene could not be saved.");
            }
            finally
            {
                if (!wasOpen)
                    EditorSceneManager.CloseScene(scene, true);

                if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
                    SceneManager.SetActiveScene(previousActiveScene);
            }
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
            public string ScopeTypeName;
            public string ScopeScriptPath;
            public string EntryPointTypeName;
        }
    }
}
