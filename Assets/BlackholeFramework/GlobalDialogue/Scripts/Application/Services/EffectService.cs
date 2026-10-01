using Blackhole.GlobalDialogue.Contract;
using Blackhole.SharedKernel.Contract;
using System;
using UnityEngine;

namespace Blackhole.GlobalDialogue.Application
{
    public class EffectService : IShowEffect
    {
        private readonly IScopedHandlerRegistery<IEffectOutput> _effectOutputHandler;

        public EffectService(IScopedHandlerRegistery<IEffectOutput> effectOutput)
        {
            _effectOutputHandler = effectOutput ?? throw new ArgumentNullException(nameof(effectOutput));
        }

        public void ShowEffectFromWorldPosition(Vector2 position, EffectSetting effectSetting)
        {
            if (_effectOutputHandler.TryGet(out var output))
            {
                output.PresentEffect(position, effectSetting);
            }
        }
    }
}