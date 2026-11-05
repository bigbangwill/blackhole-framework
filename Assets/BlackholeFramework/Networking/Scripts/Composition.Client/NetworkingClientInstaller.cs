using System;
using Blackhole.Networking.Contract;
using Blackhole.Networking.Infrastructure.Client;
using Blackhole.SharedKernel.Composition;
using UnityEngine;
using VContainer;

namespace Blackhole.Networking.Composition.Client
{
    [CreateAssetMenu(fileName = "NetworkingClientInstaller", menuName = "Blackhole/Modules/Installers/Networking Client Installer")]
    public sealed class NetworkingClientInstaller : ModuleInstallerSO
    {
        [SerializeField] private string host = "127.0.0.1";
        [SerializeField] private ushort port = NetworkSessionOptions.DefaultPort;

        public NetworkRole Role => NetworkRole.Client;

        public override void InstallModule(IContainerBuilder builder)
        {
            if (builder.Exists(typeof(NetworkSessionOptions)))
                throw new InvalidOperationException("Install only one networking role per module session.");

            builder.RegisterInstance(new NetworkSessionOptions(Role, host, port));
            builder.Register<UtpClientRuntime>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
