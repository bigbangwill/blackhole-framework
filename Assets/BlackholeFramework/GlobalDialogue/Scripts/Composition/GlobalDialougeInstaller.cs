using Blackhole.GlobalDialogue.Application;
using Blackhole.GlobalDialogue.Contract;
using Blackhole.GlobalDialogue.Infrastructure;
using Blackhole.GlobalDialogue.Presentation;
using Blackhole.SharedKernel.Composition;
using Blackhole.SharedKernel.Contract;
using UnityEngine;
using VContainer;

namespace Blackhole.GlobalDialogue.Composition
{
    [CreateAssetMenu(fileName = "GlobalDialougeInstaller", menuName = "Blackhole/Modules/Installers/GlobalDialougeInstaller")]
    public class GlobalDialougeInstaller : ModuleInstallerSO
    {
        [SerializeField] private NotificationSettingsSO notificationSettings;
        [SerializeField] private EffectSettingSO effectSettings;

        public override void InstallModule(IContainerBuilder builder)
        {
            builder.RegisterInstance(RequireAsset(notificationSettings, nameof(notificationSettings)));
            builder.RegisterInstance(RequireAsset(effectSettings, nameof(effectSettings)));

            builder.Register<IScopedHandlerRegistery<IEffectOutput>, ScopedHandlerRegistery<IEffectOutput>>(Lifetime.Singleton);
            builder.Register<IShowEffect, EffectService>(Lifetime.Singleton);
            builder.Register<IScopedHandlerRegistery<INotificationOutput>, ScopedHandlerRegistery<INotificationOutput>>(Lifetime.Singleton);
            builder.Register<IShowNotification, NotificationService>(Lifetime.Singleton);
            builder.Register<INotificationRepository, EffectDataSettingRpository>(Lifetime.Singleton);
            builder.Register<INotificationSettingUsecase, NotificationSettingUsecase>(Lifetime.Singleton);
            builder.Register<IShowConfirmationPanel, ConfirmationPanelService>(Lifetime.Singleton);
            builder.Register<IScopedHandlerRegistery<IConfirmatinOutput>, ScopedHandlerRegistery<IConfirmatinOutput>>(Lifetime.Singleton);
            builder.Register<IPositionConvertor, PositionConvetor>(Lifetime.Singleton).AsSelf();
        }
    }
}
