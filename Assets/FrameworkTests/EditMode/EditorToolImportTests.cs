using System;
using System.Collections.Generic;
using System.Reflection;
using Blackhole.Framework.Composition.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Blackhole.Development.Tests
{
    public sealed class EditorToolImportTests
    {
        private static Type Generator => typeof(RootModulesWindow).Assembly.GetType("Blackhole.Framework.Composition.Editor.GameFlowSourceGenerator", true);

        [TestCase("Blackhole.One.Foo", "Blackhole.Two.Foo")]
        [TestCase("Blackhole.Foo", "Blackhole.FooScene")]
        public void AmbiguousRootNamesAreRejectedBeforeSourceGeneration(string first, string second)
        {
            var error = Assert.Throws<TargetInvocationException>(() => Generate("ValidateModuleIds", new[] { first, second }));
            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void NavigationSourcePreservesModuleIdsAndBothEntryOptionsPaths()
        {
            string source = (string)Generate("BuildUsecase", new[] { "Example.Root\"Name" });
            Assert.That(source, Does.Contain("\"Example.Root\\\"Name\""));
            Assert.That(source, Does.Contain("_moduleLoader.LoadRootModuleAsync(moduleId, cancellationToken)"));
            Assert.That(source, Does.Contain("_moduleLoader.LoadRootModuleAsync(moduleId, entryOption, cancellationToken)"));
            Assert.That(source, Does.Contain("catch (OperationCanceledException)"));
        }

        [TestCase("Blackhole.CustomFlow", true)]
        [TestCase("Blackhole.class", false)]
        [TestCase("Bad.Namespace;", false)]
        public void GeneratedNamespacesMustBeValidCSharp(string value, bool valid)
        {
            Action validate = () => Generator.GetMethod("ValidateNamespace", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { value });
            if (valid) Assert.DoesNotThrow(() => validate());
            else Assert.Throws<TargetInvocationException>(() => validate());
        }

        [Test]
        public void SceneBrowserRegistrationPreservesBuiltInTabs()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            Type hostView = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.HostView", true);
            Type registration = typeof(SceneBrowserWindow).Assembly.GetType("Blackhole.Framework.Composition.Editor.SceneBrowserTabRegistration", true);
            registration.GetMethod("Register", flags).Invoke(null, null);
            var builtIn = (IEnumerable<Type>)hostView.GetMethod("GetBuiltInPaneTypes", flags).Invoke(null, null);
            var registered = (IEnumerable<Type>)hostView.GetMethod("GetDefaultPaneTypes", flags).Invoke(null, null);
            Assert.That(registered, Is.SupersetOf(builtIn));
        }

        [Test]
        public void SceneBrowserBuildsItsListAndCanBeClosedWithoutChangingScenes()
        {
            var window = ScriptableObject.CreateInstance<SceneBrowserWindow>();
            var before = UnityEditor.SceneManagement.EditorSceneManager.GetSceneManagerSetup();
            try
            {
                Assert.DoesNotThrow(() => window.CreateGUI());
                Assert.That(window.rootVisualElement.childCount, Is.EqualTo(2));
                var list = (UnityEngine.UIElements.ListView)window.rootVisualElement.ElementAt(1);
                Assert.That(list.itemsSource.Count, Is.GreaterThan(0));
                Assert.That(UnityEditor.SceneManagement.EditorSceneManager.GetSceneManagerSetup().Length, Is.EqualTo(before.Length));
            }
            finally { UnityEngine.Object.DestroyImmediate(window); }
        }

        private static object Generate(string method, string[] ids) => Generator.GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { ids });
    }
}
