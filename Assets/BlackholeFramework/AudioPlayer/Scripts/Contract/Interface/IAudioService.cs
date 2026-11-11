using System;

namespace Blackhole.AudioPlayer.Contract.Interface
{
    /// <summary>
    /// Public contract for requesting audio playback operations.
    /// </summary>
    public interface IAudioService
    {
        void Play(string id, float volumeOverride = -1f);

        void StopAll();

        /// <summary>Pauses currently playing audio, retaining its playback position.</summary>
        void PauseAll() => throw new NotSupportedException("This audio service does not support pausing playback.");

        /// <summary>Resumes audio paused by PauseAll.</summary>
        void ResumeAll() => throw new NotSupportedException("This audio service does not support resuming playback.");
    }
}
