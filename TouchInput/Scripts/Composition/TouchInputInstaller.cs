using Blackhole.TouchInput.Application;
using UnityEngine;
using Blackhole.TouchInput.Presentation;
using Blackhole.SharedKernel.Composition;
using VContainer;
using Blackhole.TouchInput.Contract;
using Blackhole.SharedKernel.Application;

namespace Blackhole.TouchInput.Composition
{
    [CreateAssetMenu(fileName = "TouchInputInstaller", menuName = "Blackhole/Modules/Installers/TouchInputInstaller")]
    public class TouchInputInstaller : ModuleInstallerSO
    {
        public override void InstallModule(IContainerBuilder builder)
        {
            builder.Register<InputStateProvider>(Lifetime.Singleton).As<IInputStateProvider>().As<IApplicationTick>();
            builder.Register<TouchInputController>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
