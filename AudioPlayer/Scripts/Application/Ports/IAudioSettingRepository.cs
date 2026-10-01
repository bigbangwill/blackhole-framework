using Blackhole.AudioPlayer.Application.Dto;

namespace Blackhole.AudioPlayer.Application
{
    public interface IAudioSettingRepository
    {
        public AudioSettingsData GetAudioSettingsData();
        public void SaveAudioSettingsData(AudioSettingsData data);
    }
}