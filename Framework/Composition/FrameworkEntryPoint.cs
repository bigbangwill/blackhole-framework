using System.Threading;
using Blackhole.ModuleSystem.Contract;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Blackhole.Framework.Composition
{
    public sealed class FrameworkEntryPoint : IAsyncStartable
    {
        private readonly BlackholeProjectSettingsSO projectSettings;
        private readonly IModuleLoader moduleLoader;

        public FrameworkEntryPoint(BlackholeProjectSettingsSO projectSettings, IModuleLoader moduleLoader)
        {
            this.projectSettings = projectSettings;
            this.moduleLoader = moduleLoader;
        }

        public async UniTask StartAsync(CancellationToken cancellationToken)
        {
            await moduleLoader.LoadRootModuleAsync(projectSettings.StartingModule.ModuleID, cancellationToken);
        }
    }
}
