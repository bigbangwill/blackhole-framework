using Blackhole.GlobalDialogue.Contract;
using System.Collections.Generic;
using UnityEngine;

namespace Blackhole.GlobalDialogue.Application
{
    public class NotificationSettingDTO
    {
        public readonly List<ToastSettingData> Toast_Setting;
        public readonly int MaxToastVisivleCount;
        public readonly int AnimationDuration;

        public NotificationSettingDTO(List<ToastSettingData> Toast_Setting, int MaxToastVisivleCount, int AnimationDuration)
        {
            this.Toast_Setting = Toast_Setting;
            this.AnimationDuration = AnimationDuration;
            this.MaxToastVisivleCount = MaxToastVisivleCount;
        }

        public Color ColorByNotifType(NotificationType type)
        {
            Color temp = Color.slateBlue;
            foreach (var setting in Toast_Setting)
            {
                if (type == setting.type)
                {
                    return setting.backgroundColor;
                }
                else if (setting.type == NotificationType.Default)
                {
                    temp = setting.backgroundColor;
                }

            }
            return temp;
        }
        public float GetTimerByNotifType(NotificationType type)
        {
            float default_Timer = 1;
            foreach (var setting in Toast_Setting)
            {
                if (type == setting.type)
                {
                    return setting.showTimer;
                }
            }
            return default_Timer;
        }
    }
}
