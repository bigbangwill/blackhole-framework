using Blackhole.Settings.Contract;
using UnityEngine;

namespace Blackhole.AudioPlayer.Infrastructure.ScriptableObjects
{
    [ProjectSettings("Audio", 0)]
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Blackhole/Modules/Audio Player/Audio Config")]
    public class AudioConfigSO : ScriptableObject
    {
        [SerializeField] private int _sfxChannelCount = 5;
        [SerializeField] private int _musicChannelCount = 1;

        [SerializeField] private string masterVolumeParameter = "MasterVolume";
        [SerializeField] private string musicVolumeParameter = "MusicVolume";
        [SerializeField] private string sfxVolumeParameter = "SFXVolume";

        [SerializeField]
        private string settingsKey = "audio_settings";

        public int SfxChannelCount => _sfxChannelCount;
        public int MusicChannelCount => _musicChannelCount;

        public string MasterVolumeParameter => masterVolumeParameter;
        public string MusicVolumeParameter => musicVolumeParameter;
        public string SfxVolumeParameter => sfxVolumeParameter;

        public string SettingsKey => settingsKey;
    }
}
