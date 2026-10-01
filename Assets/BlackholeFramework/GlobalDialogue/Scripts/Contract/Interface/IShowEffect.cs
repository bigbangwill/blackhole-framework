using UnityEngine;

namespace Blackhole.GlobalDialogue.Contract
{
    public interface IShowEffect
    {
        void ShowEffectFromWorldPosition(Vector2 position ,EffectSetting setting);
    }
}
