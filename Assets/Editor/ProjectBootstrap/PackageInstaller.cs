using System;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Blackhole.Development.Editor
{
    public static class PackageInstaller
    {
        private static readonly string[] PackagesToAdd =
        {
            "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.11",
            "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.18.0",
            "com.unity.render-pipelines.universal@17.5.0",
            "com.unity.inputsystem@1.19.0",
            "com.unity.ugui@2.5.0",
            "com.unity.test-framework@1.7.0"
        };
        private static readonly string[] PackagesToRemove =
        {
            "com.unity.2d.animation", "com.unity.2d.aseprite", "com.unity.2d.psdimporter", "com.unity.2d.spriteshape",
            "com.unity.2d.tilemap", "com.unity.2d.tilemap.extras", "com.unity.2d.tooling", "com.unity.visualscripting",
            "com.unity.timeline", "com.unity.collab-proxy", "com.unity.multiplayer.center", "com.unity.ide.rider"
        };
        private static AddAndRemoveRequest request;
        private static double deadline;

        public static void Install()
        {
            request = Client.AddAndRemove(PackagesToAdd, PackagesToRemove);
            deadline = EditorApplication.timeSinceStartup + 600;
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (!request.IsCompleted && EditorApplication.timeSinceStartup < deadline) return;
            EditorApplication.update -= Poll;
            if (!request.IsCompleted || request.Status != StatusCode.Success)
            {
                Debug.LogError($"Blackhole dependency installation failed: {request.Error?.message ?? "timed out"}");
                EditorApplication.Exit(1);
                return;
            }
            Debug.Log($"Blackhole dependencies resolved: {string.Join(", ", request.Result.Select(package => package.name))}");
            EditorApplication.Exit(0);
        }
    }
}
