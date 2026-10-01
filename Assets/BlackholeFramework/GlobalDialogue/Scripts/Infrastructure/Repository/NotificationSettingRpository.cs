using UnityEngine;
using Blackhole.GlobalDialogue.Application;

namespace Blackhole.GlobalDialogue.Infrastructure
{
    public class EffectDataSettingRpository : INotificationRepository
    {
        private readonly NotificationSettingsSO _setting;

        public EffectDataSettingRpository(NotificationSettingsSO setting)
        {
            _setting = setting;
        }
        public NotificationSettingDTO FetchData()
        {
            if (_setting != null)
                return new NotificationSettingDTO(_setting.Toast_Setting, _setting.MaxToastVisivleCount, _setting.AnimationDuration);
            else
                throw new System.Exception("Setting was not fount");
        }
    }
}
