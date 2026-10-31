using System;
using Blackhole.Networking.Contract;
using Blackhole.Networking.Infrastructure.UTP;
using Blackhole.SharedKernel.Composition;
using UnityEngine;
using VContainer;

namespace Blackhole.Networking.Composition
{
    [CreateAssetMenu(fileName = "NetworkingInstaller", menuName = "Blackhole/Modules/Installers/NetworkingInstaller")]
    public sealed class NetworkingInstaller : ModuleInstallerSO
    {
        [SerializeField] private NetworkRole role = NetworkRole.Client;
        [SerializeField] private string host = "127.0.0.1";
        [SerializeField] private ushort port = NetworkSessionOptions.DefaultPort;

        public NetworkRole Role => role;

        public override void InstallModule(IContainerBuilder builder)
        {
            if (builder.Exists(typeof(NetworkSessionOptions)))
                throw new InvalidOperationException("Install only one networking role per module session.");

            var options = new NetworkSessionOptions(role, host, port);
            builder.RegisterInstance(options);

            // Singleton belongs to this module session and is shared with its scene scopes.
            if (role == NetworkRole.Server)
                builder.Register<UtpServerRuntime>(Lifetime.Singleton).AsImplementedInterfaces();
            else
                builder.Register<UtpClientRuntime>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
