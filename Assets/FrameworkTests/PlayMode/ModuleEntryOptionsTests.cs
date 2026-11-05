#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using Blackhole.ModuleSystem.Composition;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;
using VContainer.Unity;

namespace Blackhole.Development.Tests
{
    public sealed class ModuleEntryOptionsTests
    {
        [UnityTest]
        public IEnumerator BaseTypedEntryOptionsRemainResolvableByBaseAndConcreteType()
        {
            var manifest = ScriptableObject.CreateInstance<ModuleManifestSO>();
            var serialized = new SerializedObject(manifest);
            serialized.FindProperty("moduleID").stringValue = "EntryOptionsTest";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var catalog = ScriptableObject.CreateInstance<ModuleManifestCatalogSO>();
            serialized = new SerializedObject(catalog);
            serialized.FindProperty("moduleManifests").arraySize = 1;
            serialized.FindProperty("moduleManifests").GetArrayElementAtIndex(0).objectReferenceValue = manifest;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            LifetimeScope project = LifetimeScope.Create(builder => builder.RegisterInstance(new InstalledModules(Array.Empty<string>())));
            LifetimeScope session = null;
            try
            {
                var loader = new RuntimeModuleLoader(project, catalog, null, null);
                BaseOptions options = new SpecificOptions();
                MethodInfo create = typeof(RuntimeModuleLoader).GetMethod("CreateSessionScope", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(typeof(BaseOptions));
                session = (LifetimeScope)create.Invoke(loader, new object[] { "EntryOptionsTest", new ResolverDto(), options });
                Assert.That(session.Container.Resolve<BaseOptions>(), Is.SameAs(options));
                Assert.That(session.Container.Resolve<SpecificOptions>(), Is.SameAs(options));
            }
            finally
            {
                if (session != null) { session.Dispose(); UnityEngine.Object.Destroy(session.gameObject); }
                project.Dispose();
                UnityEngine.Object.Destroy(project.gameObject);
                UnityEngine.Object.Destroy(catalog);
                UnityEngine.Object.Destroy(manifest);
            }
            yield return null;
        }

        public abstract class BaseOptions { }
        public sealed class SpecificOptions : BaseOptions { }
    }
}
#endif
