namespace Blackhole.AudioPlayer.Contract.Interface
{
    /// <summary>
    /// Public contract for requesting audio playback operations.
    /// </summary>
    public interface IAudioService
    {
        void Play(string id, float volumeOverride = -1f);

        void StopAll();
    }
}