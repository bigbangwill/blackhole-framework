namespace Blackhole.AudioPlayer.Contract.Interface
{
    public interface IAudioSettingsController
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

        void SaveCurrentState();
    }
}