using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Blackhole.Development.Editor
{
    /// <summary>Runs against a separate clean project, never the development source.</summary>
    public static class ReleaseImportVerification
    {
        public static void ImportBatch()
        {
            try
            {
                string package = Environment.GetEnvironmentVariable("BLACKHOLE_IMPORT_PACKAGE");
                if (string.IsNullOrWhiteSpace(package) || !File.Exists(package)) throw new InvalidOperationException("Set BLACKHOLE_IMPORT_PACKAGE to the release archive path.");
                AssetDatabase.importPackageCompleted += name =>
                {
                    Debug.Log("Blackhole release archive imported into the clean verification project.");
                    EditorApplication.delayCall += () => EditorApplication.Exit(0);
                };
                AssetDatabase.importPackageFailed += (name, error) => { Debug.LogError(error); EditorApplication.Exit(1); };
                AssetDatabase.importPackageCancelled += name => { Debug.LogError("Release import was cancelled."); EditorApplication.Exit(1); };
                AssetDatabase.ImportPackage(package, false);
            }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        public static void VerifyBatch()
        {
            try
            {
                const string root = "Assets/BlackholeFramework";
                SerializedObject settings = new(AssetDatabase.LoadMainAssetAtPath(root + "/Framework/BlackholeProjectSettings.asset"));
                if (settings.FindProperty("startingModule").objectReferenceValue != null) throw new InvalidOperationException("Release contains a development starting module.");
                SerializedObject library = new(AssetDatabase.LoadMainAssetAtPath(root + "/AudioPlayer/Settings/SoundLibrary.asset"));
                if (library.FindProperty("entries").arraySize != 0) throw new InvalidOperationException("Release contains development audio entries.");
                SerializedObject session = new(AssetDatabase.LoadMainAssetAtPath(root + "/Authentication/Settings/SessionTokenSO.asset"));
                if (!string.IsNullOrEmpty(session.FindProperty("AccessToken").stringValue) || !string.IsNullOrEmpty(session.FindProperty("RefreshToken").stringValue))
                    throw new InvalidOperationException("Release contains saved authentication credentials.");
                string[] scenes =
                {
                    root + "/Framework/BlackholeFrameworkScene.unity", root + "/LoadingScreen/LoadingScene.unity",
                    root + "/GlobalDialogue/NotificationScene.unity", root + "/WorldInteraction/WorldInteractionScene.unity"
                };
                foreach (string scenePath in scenes)
                {
                    var scene = EditorSceneManager.OpenPreviewScene(scenePath);
                    try
                    {
                        foreach (GameObject gameObject in scene.GetRootGameObjects())
                            foreach (Transform child in gameObject.GetComponentsInChildren<Transform>(true))
                                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) != 0)
                                    throw new InvalidOperationException($"Missing script in imported scene: {scenePath}");
                    }
                    finally { EditorSceneManager.ClosePreviewScene(scene); }
                }
                if (Directory.Exists("Assets/FrameworkTests")) throw new InvalidOperationException("Development tests leaked into the archive.");
                Debug.Log("Blackhole clean release import verified: compilation, default configuration and scene scripts passed.");
                EditorApplication.Exit(0);
            }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }
    }
}
