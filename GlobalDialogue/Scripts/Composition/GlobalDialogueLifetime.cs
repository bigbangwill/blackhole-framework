using Blackhole.GlobalDialogue.Application;
using Blackhole.GlobalDialogue.Presentation;
using Blackhole.GlobalDialogue.Infrastructure;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Blackhole.GlobalDialogue.Composition
{
    public class GlobalDialogueLifetime : LifetimeScope
    {

        [SerializeField] private NotificationPresentaition presenter;
        [SerializeField] private EffectPresentation effect_Presenter;
        [SerializeField] private ConfirmationPresenter globalPanel_Presenter;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(presenter).As<INotificationOutput>();
            builder.RegisterComponent(effect_Presenter).AsImplementedInterfaces();
            builder.RegisterComponent(globalPanel_Presenter).AsImplementedInterfaces();
        }
    }
}