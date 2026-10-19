using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Blackhole.Development.Tests
{
    public sealed class RootModuleScaffoldingTests
    {
        private const string EntryPoint = "Blackhole.Example.Composition.ExampleEntryPoint";
        private const string Scope = "using VContainer;\npublic class ExampleLifetimeScope : LifetimeScope\n{\n    protected override void Configure(IContainerBuilder builder)\n    {\n        builder.Register<CustomService>(Lifetime.Singleton);\n    }\n}\n";

        [Test]
        public void ExistingRegistrationsArePreservedAndRootRegistrationIsIdempotent()
        {
            string updated = Register(Scope);
            Assert.That(updated, Does.Contain("builder.Register<CustomService>(Lifetime.Singleton);"));
            Assert.That(updated, Does.Contain($"builder.RegisterEntryPoint<global::{EntryPoint}>(global::VContainer.Lifetime.Singleton);"));
            Assert.That(updated, Does.StartWith("using VContainer.Unity;\n"));
            Assert.That(Register(updated), Is.EqualTo(updated));
        }

        [TestCase("// builder.RegisterEntryPoint<ExampleEntryPoint>(Lifetime.Singleton);")]
        [TestCase("var text = \"RegisterEntryPoint<ExampleEntryPoint>(builder)\";")]
        [TestCase("builder.RegisterEntryPoint<Other.ExampleEntryPoint>(Lifetime.Singleton);")]
        public void CommentsStringsAndUnrelatedTypesDoNotSuppressRootRegistration(string existingCode)
        {
            string source = Scope.Replace("builder.Register<CustomService>(Lifetime.Singleton);", existingCode);
            Assert.That(Register(source), Does.Contain($"RegisterEntryPoint<global::{EntryPoint}>"));
        }

        [TestCase("ExampleEntryPoint")]
        [TestCase("global::Blackhole.Example.Composition.ExampleEntryPoint")]
        public void ExistingRootEntryPointIsNotDuplicated(string typeName)
        {
            string source = Scope.Replace("builder.Register<CustomService>(Lifetime.Singleton);", $"builder.RegisterEntryPoint<{typeName}>(Lifetime.Singleton);");
            Assert.That(Register(source), Is.EqualTo(source));
        }

        [Test]
        public void ConfigureParameterAndWindowsLineEndingsArePreserved()
        {
            string source = Scope.Replace("builder", "container").Replace("\n", "\r\n");
            string updated = Register(source);
            Assert.That(updated, Does.Contain("container.RegisterEntryPoint<"));
            Assert.That(updated.Replace("\r\n", string.Empty), Does.Not.Contain("\n"));
        }

        [Test]
        public void MissingConfigureFailsWithoutRewritingTheScope()
        {
            TargetInvocationException error = Assert.Throws<TargetInvocationException>(() => Register("public class ExampleLifetimeScope {}"));
            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
        }

        private static string Register(string source)
        {
            Type helper = AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType("Blackhole.Editor.RootModuleScopeSource")).First(type => type != null);
            MethodInfo method = helper.GetMethod("EnsureEntryPointRegistration", BindingFlags.Static | BindingFlags.NonPublic);
            return (string)method.Invoke(null, new object[] { source, EntryPoint });
        }
    }
}
