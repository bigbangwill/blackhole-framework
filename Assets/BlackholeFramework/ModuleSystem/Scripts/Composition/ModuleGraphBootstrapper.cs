using System;
using VContainer;

namespace Blackhole.ModuleSystem.Composition
{
    /// <summary>Installs a resolved module graph into a container scope.</summary>
    public static class ModuleGraphBootstrapper
    {
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
