using Blackhole.GlobalDialogue.Contract;
using UnityEngine;

namespace Blackhole.GlobalDialogue.Application
{
    public interface IEffectOutput
    {
        void PresentEffect(Vector2 position, EffectSetting setting);
    }
}
