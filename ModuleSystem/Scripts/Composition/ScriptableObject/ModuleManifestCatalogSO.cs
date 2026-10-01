using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blackhole.ModuleSystem.Composition
{
    [CreateAssetMenu(fileName = "ModuleManifestCatalog", menuName = "Blackhole/Modules/Module System/Module Manifest Catalog")]
    public sealed class ModuleManifestCatalogSO : ScriptableObject
    {
        [SerializeField] private ModuleManifestSO[] moduleManifests;

        public IReadOnlyList<ModuleManifestSO> ModuleManifests => moduleManifests ?? Array.Empty<ModuleManifestSO>();

        public ModuleManifestSO GetModule(string moduleId)
        {
            if (string.IsNullOrWhiteSpace(moduleId))
                throw new ArgumentException("Module ID cannot be null or empty.", nameof(moduleId));

            if (moduleManifests == null || moduleManifests.Length == 0)
            {
                throw new InvalidOperationException(
                    "The module manifest catalog is empty. Add module manifests to ModuleManifestCatalog.asset.");
            }

            ModuleManifestSO match = null;
            foreach (ModuleManifestSO module in moduleManifests)
            {
                if (module == null)
                    continue;

                if (module.ModuleID != moduleId)
                    continue;

                if (match != null)
                    throw new InvalidOperationException($"Multiple module manifests have ID '{moduleId}'. Module IDs must be unique.");

                match = module;
            }

            if (match != null)
                return match;

            throw new InvalidOperationException($"Module manifest with ID '{moduleId}' was not found.");
        }
    }
}
