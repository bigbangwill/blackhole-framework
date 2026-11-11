using Blackhole.Networking.Contract;
using Blackhole.SharedKernel.Composition;
using UnityEngine;
using VContainer;

namespace Blackhole.JoinTest.Composition
{
    [CreateAssetMenu(fileName = "JoinTestInstaller", menuName = "Blackhole/Modules/Installers/JoinTestInstaller")]
    public sealed class JoinTestInstaller : ModuleInstallerSO
    {
        public override void InstallModule(IContainerBuilder builder)
        {
            builder.Register<ClientNetworkHandlerTest>(Lifetime.Singleton).As<INetworkClientPayloadHandler>();
            builder.Register<ServerNetworkHandlerTest>(Lifetime.Singleton).As<INetworkPayloadHandler>();
        }
    }
}
