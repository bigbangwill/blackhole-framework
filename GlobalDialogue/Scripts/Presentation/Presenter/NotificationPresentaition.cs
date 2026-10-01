using Blackhole.GlobalDialogue.Application;
using Blackhole.GlobalDialogue.Contract;
using Blackhole.SharedKernel.Contract;
using Blackhole.SharedKernel.Presentaion;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using VContainer;

namespace Blackhole.GlobalDialogue.Presentation
{
    public class NotificationPresentaition : Orientation, INotificationOutput
    {
        private const string toast_Style = "toast-style";
        private const string toast_Message = "toast-label";
        private const string toast_Circle = "toast-circle";


        private VisualElement _root;
        private VisualElement notificationVE;
        private NotificationSettingDTO _setting;


        // Each toast has its own CancellationTokenSource.
        private readonly Dictionary<VisualElement, CancellationTokenSource> _toastTokens = new();

        private IScopedHandlerRegistery<INotificationOutput> _handler;

        [Inject]
        public void Construct(IScopedHandlerRegistery<INotificationOutput> handler, INotificationSettingUsecase settingUsecase)
        {
            _handler = handler;
            _setting = settingUsecase.GetSettingData();
        }



        protected override void Awake()
        {
            base.Awake();
            _handler.Register(this);
        }

        protected override void OnUIReloaded(PanelRenderer panelRenderer, VisualElement root)
        {
            base.OnUIReloaded(panelRenderer, root);
            _root = DocumentRoot;
            notificationVE = _root.Q<VisualElement>("NotificationVE");
        }

        public void ShowNotification(NotificationMessage message, Action finishedToast)
        {
            ToastLifeCycle(message.Message, _setting.ColorByNotifType(message.NotificationType),
                _setting.GetTimerByNotifType(message.NotificationType), finishedToast).Forget();
        }

        // Controls the complete lifecycle of one toast.
        private async UniTask ToastLifeCycle(string message, Color backgroundColor, float showTimer, Action finishedToast)
        {
            VisualElement toast = CreateToast(message, backgroundColor);

            // Create a separate cancellation source for this toast.
            CancellationTokenSource cts = new CancellationTokenSource();

            _toastTokens.Add(toast, cts);

            try
            {
                ToastShowAnimation(toast);
                await UniTask.Delay(TimeSpan.FromSeconds(showTimer), cancellationToken: cts.Token);
            }
            catch (OperationCanceledException)
            {
            }

            HideToast(toast);

            await UniTask.Delay(_setting.AnimationDuration);

            finishedToast?.Invoke();

            if (_toastTokens.ContainsKey(toast))
            {
                _toastTokens.Remove(toast);
            }
            cts.Dispose();
        }

        private VisualElement CreateToast(string message, Color backgroundColor)
        {
            VisualElement toast = new VisualElement();

            toast.style.opacity = 0f;
           
            toast.AddToClassList(toast_Style);

            Label messageLabel = new Label
            {
                text = message
            };

            messageLabel.AddToClassList(toast_Message);
            messageLabel.AddToClassList("global-font");

            VisualElement circle = new VisualElement();
            circle.style.backgroundColor = backgroundColor;
            circle.AddToClassList(toast_Circle);

            toast.Add(messageLabel);
            toast.Add(circle);
            toast.RegisterCallback<ClickEvent>(OnToastClicked);

            return toast;
        }

        private void OnToastClicked(ClickEvent evt)
        {
            VisualElement toast = evt.currentTarget as VisualElement;

            if (toast == null)
                return;

            if (_toastTokens.TryGetValue(toast, out CancellationTokenSource cts))
                cts.Cancel();

        }

        private void ToastShowAnimation(VisualElement toast)
        {
            notificationVE.Add(toast);

            toast.schedule.Execute(() =>
            {
                float height = toast.resolvedStyle.height;

                toast.style.top = -height;
                toast.schedule.Execute(() =>
                {
                    // Slide from above to its normal position.
                    toast.experimental.animation.Start(-height, 0f, _setting.AnimationDuration, (element, value) =>
                            {
                                element.style.top = value;
                            }).Ease(Easing.OutBack);
                    // Fade in at the same time.
                    toast.experimental.animation.Start(0f, 1f, _setting.AnimationDuration, (element, value) =>
                            {
                                element.style.opacity = value;
                            }).Ease(Easing.OutCubic);
                }).ExecuteLater(16);
            }).ExecuteLater(10);
        }

        private void HideToast(VisualElement toast)
        {
            if (toast.parent == null)
                return;

            float height = toast.resolvedStyle.height;
            // Slide the toast back upward.
            toast.experimental.animation.Start(0f, -height, _setting.AnimationDuration, (element, value) =>
                    {
                        element.style.top = value;
                    }).Ease(Easing.InCubic);
            // Fade out at the same time.
            toast.experimental.animation.Start(1f, 0f, _setting.AnimationDuration, (element, value) =>
                    {
                        element.style.opacity = value;
                    }).Ease(Easing.InCubic);
            // Remove the toast after its hide animation finishes.
            toast.schedule.Execute(() =>
            {
                toast.UnregisterCallback<ClickEvent>(OnToastClicked);
                if (toast.parent != null)
                {
                    toast.RemoveFromHierarchy();
                }
            }).ExecuteLater(_setting.AnimationDuration);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _handler.Unregister(this);

            foreach (CancellationTokenSource cts in _toastTokens.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }
            _toastTokens.Clear();
        }
    }
}