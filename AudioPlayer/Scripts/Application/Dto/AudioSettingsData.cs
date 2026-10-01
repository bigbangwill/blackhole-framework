namespace Blackhole.AudioPlayer.Application.Dto
{
    /// <summary>
    /// Data container for storing audio volume levels and mute states.
    /// </summary>
    [System.Serializable]
    public class AudioSettingsData
    {
        public float MasterVolume = 0.75f;
        public float SfxVolume = 0.75f;
        public float MusicVolume = 0.75f;

        public bool MasterMuted = false;
        public bool SfxMuted = false;
        public bool MusicMuted = false;
    }
}