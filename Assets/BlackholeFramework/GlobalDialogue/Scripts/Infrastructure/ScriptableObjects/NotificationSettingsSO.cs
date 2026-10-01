using Blackhole.GlobalDialogue.Contract;
using Blackhole.Settings.Contract;
using System;
using Blackhole.GlobalDialogue.Application;
using System.Collections.Generic;
using UnityEngine;

namespace Blackhole.GlobalDialogue.Infrastructure
{
    [ProjectSettings("Notification Panel", 300)]
    [CreateAssetMenu(fileName = "NotificationSettings", menuName = "Blackhole/Modules/Global Dialogue/Notification Settings")]
    public class NotificationSettingsSO : ScriptableObject
    {
        [SerializeField] private List<ToastSettingData> _toasts_setting;
        public List<ToastSettingData> Toast_Setting => _toasts_setting;
        [SerializeField] private readonly int _maxToastVisivleCount = 2;
        public  int MaxToastVisivleCount => _maxToastVisivleCount;
        [SerializeField] private readonly int _animationDuration = 400;
        public int AnimationDuration => _animationDuration;
    }
}
