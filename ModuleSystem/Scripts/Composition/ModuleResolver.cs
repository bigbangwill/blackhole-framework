using System;
using System.Collections.Generic;
using Blackhole.SharedKernel.Composition;

namespace Blackhole.ModuleSystem.Composition
{
    public sealed class ModuleResolver
    {
        private readonly ModuleManifestCatalogSO moduleCatalog;

        public ModuleResolver(ModuleManifestCatalogSO moduleCatalog)
        {
            this.moduleCatalog = moduleCatalog ?? throw new ArgumentNullException(nameof(moduleCatalog));
        }

        public ResolverDto GetResolverDto(string moduleID, InstalledModules parentModules = null)
        {
            if (string.IsNullOrWhiteSpace(moduleID))
                throw new ArgumentException("Module ID cannot be null or empty.", nameof(moduleID));

            ModuleManifestSO targetModule = moduleCatalog.GetModule(moduleID);

            ResolverDto resolverDto = new();

            HashSet<string> visited = new();
            HashSet<string> visiting = new();

            ResolveModule(targetModule, visited, visiting, resolverDto, parentModules);

            return resolverDto;
        }

        private void ResolveModule(ModuleManifestSO module, HashSet<string> visited, HashSet<string> visiting, ResolverDto resolverDto, InstalledModules parentModules)
        {
            if (module == null)
                return;

            string moduleID = module.ModuleID;

            if (string.IsNullOrWhiteSpace(moduleID))
                throw new InvalidOperationException("A module manifest has an empty ModuleID.");

            if (visited.Contains(moduleID))
                return;

            // An inherited module already has its dependency graph in its owning scope.
            if (!module.AllowChildScopeOverride && parentModules != null && parentModules.Contains(moduleID))
            {
                visited.Add(moduleID);
                return;
            }

            if (visiting.Contains(moduleID))
            {
                throw new InvalidOperationException(
                    $"Circular module dependency detected at module '{moduleID}'.");
            }

            visiting.Add(moduleID);

            foreach (ModuleManifestSO dependency in module.Dependencies ?? Array.Empty<ModuleManifestSO>())
            {
                if (dependency == null)
                    throw new InvalidOperationException($"Module '{moduleID}' has a missing dependency reference. Reassign it in its manifest.");
                ResolveModule(dependency, visited, visiting, resolverDto, parentModules);
            }

            visiting.Remove(moduleID);
            visited.Add(moduleID);

            ModuleInstallerSO installer = module.Installer;
            if (installer == null)
                throw new InvalidOperationException($"Module '{moduleID}' has no installer assigned in its manifest.");
            resolverDto.Installers.Add(installer);
            resolverDto.ModuleIDs.Add(moduleID);

            if (!string.IsNullOrWhiteSpace(module.ModuleScene))
            {
                resolverDto.Scenes.Add(module.ModuleScene);
            }
        }
    }

    public sealed class ResolverDto
    {
        public readonly List<string> ModuleIDs = new();
        public readonly List<IModuleInstaller> Installers = new();
        public readonly List<string> Scenes = new();
    }

    /// <summary>Immutable snapshot of modules available in a scope and its ancestors.</summary>
    public sealed class InstalledModules
    {
        private readonly HashSet<string> moduleIDs;
        private readonly InstalledModules parent;

        public InstalledModules(IEnumerable<string> moduleIDs, InstalledModules parent = null)
        {
            this.moduleIDs = new HashSet<string>(moduleIDs, StringComparer.Ordinal);
            this.parent = parent;
        }

        public bool Contains(string moduleID) => moduleIDs.Contains(moduleID) || (parent?.Contains(moduleID) ?? false);
    }
}
