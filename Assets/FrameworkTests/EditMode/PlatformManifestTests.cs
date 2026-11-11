using Blackhole.Development;
using Blackhole.ModuleSystem.Composition;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Unity.Multiplayer;
using Unity.Multiplayer.Editor;

namespace Blackhole.Development.Tests
{
    public sealed class PlatformManifestTests
    {
        [TestCase(MultiplayerRoleFlags.Client)]
        [TestCase(MultiplayerRoleFlags.Server)]
        [TestCase(MultiplayerRoleFlags.ClientAndServer)]
        public void EditorRetainsBothInstallersAndResolverUsesTheInstanceRole(MultiplayerRoleFlags role)
        {
            var previousRole = EditorMultiplayerRolesManager.ActiveMultiplayerRoleMask;
            var manifest = ScriptableObject.CreateInstance<PlatformModuleManifestSO>();
            var client = ScriptableObject.CreateInstance<SmokeInstaller>();
            var server = ScriptableObject.CreateInstance<SmokeInstaller>();
            var catalog = ScriptableObject.CreateInstance<ModuleManifestCatalogSO>();
            try
            {
                EditorMultiplayerRolesManager.ActiveMultiplayerRoleMask = role;
                var serialized = new SerializedObject(manifest);
                serialized.FindProperty("moduleID").stringValue = "PlatformTest";
                Assert.That(serialized.FindProperty("_clientInstaller"), Is.Not.Null);
                Assert.That(serialized.FindProperty("_serverInstaller"), Is.Not.Null);
                serialized.FindProperty("_clientInstaller").objectReferenceValue = client;
                serialized.FindProperty("_serverInstaller").objectReferenceValue = server;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                serialized = new SerializedObject(catalog);
                serialized.FindProperty("moduleManifests").arraySize = 1;
                serialized.FindProperty("moduleManifests").GetArrayElementAtIndex(0).objectReferenceValue = manifest;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(new ModuleResolver(catalog).GetResolverDto("PlatformTest").Installers[0], Is.SameAs(role == MultiplayerRoleFlags.Server ? server : client));
            }
            finally
            {
                EditorMultiplayerRolesManager.ActiveMultiplayerRoleMask = previousRole;
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(manifest);
                Object.DestroyImmediate(client);
                Object.DestroyImmediate(server);
            }
        }
    }
}
