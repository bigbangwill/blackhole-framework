using Blackhole.AudioPlayer.Contract.Interface;

namespace Blackhole.AudioPlayer.Application.UseCases
{
    public class AudioSettingsController : IAudioSettingsController
    {
        private readonly IAudioVolumeUseCase volumeUseCase;
        private readonly IAudioSettingsUseCase settingsUseCase;

        public AudioSettingsController( IAudioVolumeUseCase volumeUseCase, IAudioSettingsUseCase settingsUseCase)
        {
            this.volumeUseCase = volumeUseCase;
            this.settingsUseCase = settingsUseCase;
        }

        public void ChangeMasterVolume(float value) => volumeUseCase.ChangeMasterVolume(value);
        public void ChangeMusicVolume(float value) => volumeUseCase.ChangeMusicVolume(value);
        public void ChangeSfxVolume(float value) => volumeUseCase.ChangeSfxVolume(value);

        public void ToggleMasterMute(bool muted) => volumeUseCase.ToggleMasterMute(muted);
        public void ToggleMusicMute(bool muted) => volumeUseCase.ToggleMusicMute(muted);
        public void ToggleSfxMute(bool muted) => volumeUseCase.ToggleSfxMute(muted);

        public float GetMasterVolume() => volumeUseCase.GetMasterVolume();
        public float GetMusicVolume() => volumeUseCase.GetMusicVolume();
        public float GetSfxVolume() => volumeUseCase.GetSfxVolume();

        public void SaveCurrentState() => settingsUseCase.Save();
    }
}