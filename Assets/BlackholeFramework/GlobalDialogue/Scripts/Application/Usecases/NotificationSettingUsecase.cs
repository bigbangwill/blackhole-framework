using UnityEngine;

namespace Blackhole.GlobalDialogue.Application
{
    public class NotificationSettingUsecase : INotificationSettingUsecase
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationSettingUsecase(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public NotificationSettingDTO GetSettingData()
        {
            return _notificationRepository.FetchData();
        }
    }
}
