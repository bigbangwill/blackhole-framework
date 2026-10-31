using System.Collections;
using Blackhole.Framework.Composition;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

namespace Blackhole.Development.Tests
{
    public sealed class FrameworkStartupTests
    {
        [UnityTest]
        public IEnumerator FrameworkStartsConfiguredPersistentRootModule()
        {
            yield return SceneManager.LoadSceneAsync(FrameworkLifetimeScope.FrameworkScenePath);
            FrameworkLifetimeScope scope = Object.FindFirstObjectByType<FrameworkLifetimeScope>();
            var settings = scope.Container.Resolve<BlackholeProjectSettingsSO>();
            string path = settings.StartingModule.ModuleScene;
            float deadline = Time.realtimeSinceStartup + 20;
            while (SceneManager.GetActiveScene().path != path && Time.realtimeSinceStartup < deadline) yield return null;
            var scene = SceneManager.GetSceneByPath(path);
            Assert.That(settings.StartingModule.IsPersistant, Is.True);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
            Assert.That(SceneManager.GetActiveScene(), Is.EqualTo(scene));
        }
    }
}
