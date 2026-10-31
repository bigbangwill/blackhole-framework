using Blackhole.ModuleSystem.Composition;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Blackhole.Development.Tests
{
    public sealed class IsolatedScopeFixture : LifetimeScope
    {
        [SerializeField] private ModuleManifestCatalogSO _catalog;
        [SerializeField] private ModuleManifestSO _module;
        public ModuleManifestCatalogSO Catalog => _catalog;
        public ModuleManifestSO Module => _module;
        protected override void Configure(IContainerBuilder builder) { }
    }
}
