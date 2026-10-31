using System;
using System.Linq;
using System.Reflection;
using Blackhole.ModuleSystem.Composition;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Blackhole.Development.Tests
{
    public sealed class IsolatedModuleScaffoldingTests
    {
        [Test]
        public void IsolatedSceneSetupIsIdempotentAndAssignsCatalogManifestAndCamera()
        {
            const string path = "Assets/FrameworkTests/IsolatedSetupVerification.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, path);
            var catalog = AssetDatabase.LoadAssetAtPath<ModuleManifestCatalogSO>("Assets/BlackholeFramework/ModuleSystem/ModuleManifestCatalog.asset");
            var manifest = AssetDatabase.LoadAssetAtPath<ModuleManifestSO>("Assets/BlackholeFramework/WorldInteraction/Blackhole.WorldInteraction.asset");
            Assert.That(catalog, Is.Not.Null);
            Assert.That(manifest, Is.Not.Null);
            try
            {
                Type helper = AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType("Blackhole.Editor.PendingIsolatedModuleSceneSetup")).First(type => type != null);
                MethodInfo setup = helper.GetMethod("SetupScene", BindingFlags.Static | BindingFlags.NonPublic);
                setup.Invoke(null, new object[] { path, typeof(IsolatedScopeFixture), manifest, catalog });
                setup.Invoke(null, new object[] { path, typeof(IsolatedScopeFixture), manifest, catalog });
                try
                {
                    var scope = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<IsolatedScopeFixture>()).Single();
                    Assert.That(scope.Catalog, Is.EqualTo(catalog));
                    Assert.That(scope.Module, Is.EqualTo(manifest));
                    Camera camera = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Camera>()).Single();
                    Assert.That(camera.orthographic, Is.True);
                    Assert.That(camera.GetComponent<AudioListener>(), Is.Not.Null);
                }
                finally { EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); }
            }
            finally { AssetDatabase.DeleteAsset(path); }
        }

        [Test]
        public void FrameworkModuleTestPathsStayOutsideReleaseAssets()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType("Blackhole.Editor.ModuleScaffolderWindow")).First(value => value != null);
            var window = ScriptableObject.CreateInstance(type);
            try
            {
                type.GetField("assemblyRoot", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(window, "Blackhole.CameraController");
                string path = (string)type.GetMethod("GetIsolatedTestSceneAssetPath", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(window, null);
                Assert.That(path, Does.StartWith("Assets/FrameworkTests/Isolated/"));
                Assert.That(path, Does.EndWith("CameraControllerTest.unity"));
            }
            finally { UnityEngine.Object.DestroyImmediate(window); }
        }
    }
}
