using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Blackhole.Editor
{
    internal static class IsolatedModuleScopeSource
    {
        internal static string Build(string scopeNamespace, string scopeName, bool isRootModule)
        {
            return string.Join(Environment.NewLine, new[]
            {
                "using Blackhole.ModuleSystem.Composition;",
                "using Blackhole.SceneLoader.Contract;",
                "using Blackhole.SharedKernel.Composition;",
                "using Cysharp.Threading.Tasks;",
                "using UnityEngine;",
                "using VContainer;",
                "using VContainer.Unity;",
                string.Empty,
                $"namespace {scopeNamespace}",
                "{",
                $"    public sealed class {scopeName} : LifetimeScope",
                "    {",
                "        [SerializeField] private ModuleManifestCatalogSO _catalog;",
                "        [SerializeField] private ModuleManifestSO _module;",
                string.Empty,
                "        private ResolverDto _graph;",
                string.Empty,
                "        protected override void Configure(IContainerBuilder builder)",
                "        {",
                "            ScopeCancellationInstaller.Install(builder);",
                isRootModule ? "            builder.Register<RootModuleStartupRegistry>(Lifetime.Singleton);" : null,
                "            _graph = ModuleGraphBootstrapper.Install(builder, _module.ModuleID, _catalog);",
                "        }",
                string.Empty,
                "        private void Start()",
                "        {",
                "            LoadDependencyScenesAsync().Forget();",
                "        }",
                string.Empty,
                "        private async UniTask LoadDependencyScenesAsync()",
                "        {",
                "            var sceneLoader = Container.Resolve<ISceneTransitionUseCase>();",
                "            var cancellationToken = this.GetCancellationTokenOnDestroy();",
                string.Empty,
                "            foreach (string scenePath in _graph.Scenes)",
                "            {",
                "                if (scenePath == gameObject.scene.path)",
                "                    continue;",
                string.Empty,
                "                using (LifetimeScope.EnqueueParent(this))",
                "                {",
                "                    await sceneLoader.LoadSceneAsync(scenePath, true, cancellationToken);",
                "                }",
                "            }",
                string.Empty,
                isRootModule ? "            await Container.Resolve<RootModuleStartupRegistry>().GetRequired(_module.ModuleID).InitializeAsync(cancellationToken);" : null,
                "            Debug.Log($\"[{_module.ModuleID}] Dependency installers and scenes are ready.\");",
                "        }",
                "    }",
                "}",
                string.Empty
            }.Where(line => line != null));
        }

        internal static bool IsEmptyLifetimeScopeTemplate(string source)
        {
            string code = Regex.Replace(source, @"\s+", string.Empty);
            return Regex.IsMatch(code, @"\A(?:using[\w.]+;)*(?:namespace[\w.]+\{)?public(?:sealed)?class\w+:LifetimeScope\{protectedoverridevoidConfigure\(IContainerBuilder\w+\)\{\}\}\}?\z");
        }
    }
}
