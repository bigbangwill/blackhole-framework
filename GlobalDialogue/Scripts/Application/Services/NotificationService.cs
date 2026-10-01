using Blackhole.GlobalDialogue.Contract;
using Blackhole.SharedKernel.Contract;
using System;
using System.Collections.Generic;
using VContainer;

namespace Blackhole.GlobalDialogue.Application
{
    public class NotificationService : IShowNotification
    {
        private readonly IScopedHandlerRegistery<INotificationOutput> _notificationOutputHandler;

        private readonly NotificationSettingDTO _setting;
        private int _activeToasts = 0;


        private readonly Queue<NotificationMessage> queuedNotifications = new();

        public NotificationService(IScopedHandlerRegistery<INotificationOutput> notificationOutput, INotificationSettingUsecase notificationRepository)
        {
            _notificationOutputHandler = notificationOutput ?? throw new ArgumentNullException(nameof(notificationOutput));
            _setting = notificationRepository.GetSettingData();
        }

        public void ShowNotification(NotificationMessage message)
        {
            if (_activeToasts >= _setting.MaxToastVisivleCount)
            {
                queuedNotifications.Enqueue(message);
                return;
            }
            _activeToasts++;

            if (_notificationOutputHandler.TryGet(out var output))
            {
                output.ShowNotification(message, OnFinishedNotification);
            }
        }

        private void OnFinishedNotification()
        {
            _activeToasts--;
            if (queuedNotifications.Count > 0)
            {
                NotificationMessage message = queuedNotifications.Dequeue();
                _activeToasts++;

                if (_notificationOutputHandler.TryGet(out var output))
                {
                    output.ShowNotification(message, OnFinishedNotification);
                }
            }
        }
    }
}