using System;
using VContainer;
using VContainer.Unity;

namespace Blackhole.ModuleSystem.Composition
{
    /// <summary>Installs a resolved module graph into a container scope.</summary>
    public static class ModuleGraphBootstrapper
    {
        /// <summary>Installs the graph and starts loading its scenes when the owning scope starts.</summary>
        public static ResolverDto InstallAndLoadScenes(IContainerBuilder builder, string rootModuleId, ModuleManifestCatalogSO moduleCatalog, InstalledModules inheritedModules = null)
        {
            ResolverDto resolverDto = Install(builder, rootModuleId, moduleCatalog, inheritedModules);
            builder.RegisterEntryPoint<ModuleGraphStartup>().AsSelf().WithParameter(resolverDto);
            return resolverDto;
        }

        public static ResolverDto Install(IContainerBuilder builder, string rootModuleId, ModuleManifestCatalogSO moduleCatalog, InstalledModules inheritedModules = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (string.IsNullOrWhiteSpace(rootModuleId))
                throw new ArgumentException("Root module ID cannot be null or whitespace.", nameof(rootModuleId));

            if (moduleCatalog == null)
                throw new ArgumentNullException(nameof(moduleCatalog), "A module manifest catalog is required to bootstrap a module graph.");

            builder.RegisterInstance(moduleCatalog).AsSelf();

            ModuleResolver resolver = new(moduleCatalog);
            ResolverDto resolverDto = resolver.GetResolverDto(rootModuleId, inheritedModules);

            foreach (var installer in resolverDto.Installers)
            {
                installer.InstallModule(builder);
            }

            builder.RegisterInstance(new InstalledModules(resolverDto.ModuleIDs, inheritedModules));
            return resolverDto;
        }
    }
}
