using Blackhole.GlobalDialogue.Contract;
using System;
using UnityEngine;

namespace Blackhole.GlobalDialogue.Application
{
    [Serializable]
    public struct ToastSettingData
    {
        public NotificationType type;
        public Color backgroundColor;
        public float showTimer;
    }
}
