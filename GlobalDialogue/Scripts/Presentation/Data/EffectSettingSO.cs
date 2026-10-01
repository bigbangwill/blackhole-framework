using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blackhole.GlobalDialogue.Presentation
{
    [CreateAssetMenu(fileName = "EffectSettingSO", menuName = "Blackhole/Modules/Global Dialogue/Effect Settings")]
    public class EffectSettingSO : ScriptableObject
    {
        [SerializeField] private List<EffectData> _effectData;
        public List<EffectData> EffectDatas => _effectData;
    }

    [Serializable]
    public class EffectData
    {
        public string EffectName;
        public Sprite Effect_Sprite;
    }
}
