using System;
using Blackhole.ModuleSystem.Composition;
using Blackhole.SharedKernel.Composition;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Blackhole.Framework.Composition
{
    public sealed class FrameworkLifetimeScope : LifetimeScope
    {
        public const string FrameworkScenePath = "Packages/com.bigbang.blackhole-framework/Framework/BlackholeFrameworkScene.unity";
        public const string FrameworkModuleId = "Blackhole.Framework";

        [SerializeField] private BlackholeProjectSettingsSO projectSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            if (projectSettings == null)
                throw new InvalidOperationException("Assign framework project settings to the FrameworkLifetimeScope in the framework scene.");

            if (projectSettings.ModuleCatalog == null)
                throw new InvalidOperationException("Framework project settings do not specify a module catalog.");

            if (projectSettings.StartingModule == null || string.IsNullOrWhiteSpace(projectSettings.StartingModule.ModuleID))
                throw new InvalidOperationException("Framework project settings do not specify a starting module.");

            if (string.IsNullOrWhiteSpace(projectSettings.StartingModule.ModuleScene))
                throw new InvalidOperationException("The configured starting module must reference a scene.");

            if (projectSettings.ModuleCatalog.GetModule(projectSettings.StartingModule.ModuleID) != projectSettings.StartingModule)
                throw new InvalidOperationException("The configured starting module must be the manifest registered in the module catalog.");

            ScopeCancellationInstaller.Install(builder);
            builder.RegisterInstance(projectSettings);

            ModuleGraphBootstrapper.Install(builder, FrameworkModuleId, projectSettings.ModuleCatalog);
        }
    }
}
