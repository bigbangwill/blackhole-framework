using Blackhole.Messaging.Contract;
using Blackhole.Messaging.Infrastructure;
using Blackhole.SharedKernel.Composition;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace Blackhole.Messaging.Composition
{
    [CreateAssetMenu(fileName = "MessagingInstaller", menuName = "Blackhole/Modules/Installers/MessagingInstaller")]
    public sealed class MessagingInstaller : ModuleInstallerSO
    {
        public override void InstallModule(IContainerBuilder builder)
        {
            builder.RegisterMessagePipe();

            builder.Register(typeof(MessagePipeEventPublisher<>), Lifetime.Singleton).As(typeof(IEventPublisher<>));
            builder.Register(typeof(MessagePipeEventSubscriber<>), Lifetime.Singleton).As(typeof(IEventSubscriber<>));
        }
    }
}
