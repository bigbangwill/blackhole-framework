using System;
using Blackhole.Networking.Contract;
using Blackhole.Networking.Infrastructure.Server;
using Blackhole.SharedKernel.Composition;
using UnityEngine;
using VContainer;

namespace Blackhole.Networking.Composition.Server
{
    [CreateAssetMenu(fileName = "NetworkingServerInstaller", menuName = "Blackhole/Modules/Installers/Networking Server Installer")]
    public sealed class NetworkingServerInstaller : ModuleInstallerSO
    {
        [SerializeField] private string host = "0.0.0.0";
        [SerializeField] private ushort port = NetworkSessionOptions.DefaultPort;

        public NetworkRole Role => NetworkRole.Server;

        public override void InstallModule(IContainerBuilder builder)
        {
            if (builder.Exists(typeof(NetworkSessionOptions)))
                throw new InvalidOperationException("Install only one networking role per module session.");

            builder.RegisterInstance(new NetworkSessionOptions(Role, host, port));
            builder.Register<UtpServerRuntime>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
