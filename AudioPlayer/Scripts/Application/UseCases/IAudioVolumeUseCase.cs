namespace Blackhole.AudioPlayer.Application.UseCases
{
    public interface IAudioVolumeUseCase
    {
        void ChangeMasterVolume(float value);
        void ChangeMusicVolume(float value);
        void ChangeSfxVolume(float value);


        void ToggleMasterMute(bool muted);
        void ToggleMusicMute(bool muted);
        void ToggleSfxMute(bool muted);

        float GetMasterVolume();
        float GetMusicVolume();
        float GetSfxVolume();
    }
}