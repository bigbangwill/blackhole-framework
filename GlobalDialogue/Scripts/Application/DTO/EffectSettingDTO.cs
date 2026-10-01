using Blackhole.GlobalDialogue.Contract;

namespace Blackhole.GlobalDialogue.Application
{
    public class EffectSettingDTO 
    {
        public readonly EffectSetting Setting;
        
        public EffectSettingDTO(EffectSetting setting)
        {
            Setting = setting;
        }
    }
}
