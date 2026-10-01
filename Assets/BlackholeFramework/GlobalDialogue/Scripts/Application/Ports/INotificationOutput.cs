using Blackhole.GlobalDialogue.Contract;
using System;

namespace Blackhole.GlobalDialogue.Application
{
    public interface INotificationOutput
    {
         void ShowNotification(NotificationMessage message,Action finishedToast);
    }
}