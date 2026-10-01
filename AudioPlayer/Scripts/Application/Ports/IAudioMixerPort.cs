namespace Blackhole.AudioPlayer.Application.Ports
{
    /// <summary>
    /// Abstraction for controlling audio mixer channels independently of Unity.
    /// </summary>
    public interface IAudioMixerPort
    {
        void SetMasterVolume(float value);

        void SetMusicVolume(float value);

        void SetSfxVolume(float value);


        void SetMasterMute(bool muted, float volume);

        void SetMusicMute(bool muted, float volume);

        void SetSfxMute(bool muted, float volume);
    }
}