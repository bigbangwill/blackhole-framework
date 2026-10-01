using System;
using UnityEngine;
using VContainer;

namespace Blackhole.SharedKernel.Composition
{
    /// <summary>Shared module configuration that registers services without retaining scope state.</summary>
    public abstract class ModuleInstallerSO : ScriptableObject, IModuleInstaller
    {
        public abstract void InstallModule(IContainerBuilder builder);

        protected T RequireAsset<T>(T asset, string fieldName) where T : UnityEngine.Object
        {
            if (asset == null)
                throw new InvalidOperationException($"Installer '{name}' ({GetType().Name}) requires an assigned '{fieldName}' asset.");

            return asset;
        }
    }
}
