using System.Collections;
using Blackhole.Framework.Composition;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Blackhole.Development.Tests
{
    public sealed class FrameworkStartupTests
    {
        [UnityTest]
        public IEnumerator FrameworkStartsPersistentRootModule()
        {
            yield return SceneManager.LoadSceneAsync(FrameworkLifetimeScope.FrameworkScenePath);
            float deadline = Time.realtimeSinceStartup + 20;
            while (!SmokeRootStartup.Completed && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(SmokeRootStartup.Completed, Is.True, "Framework did not initialize the root module.");
            var scene = SceneManager.GetSceneByPath("Assets/FrameworkTests/Smoke/FrameworkSmokeScene.unity");
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
            Assert.That(SceneManager.GetActiveScene(), Is.EqualTo(scene));
        }
    }
}
