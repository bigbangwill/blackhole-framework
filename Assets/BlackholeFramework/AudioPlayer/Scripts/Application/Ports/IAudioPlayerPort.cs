using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Blackhole.AudioPlayer.Application.Ports
{
    /// <summary>
    /// Abstraction for audio playback independent of the underlying audio system.
    /// </summary>
    public interface IAudioPlayerPort
    {
        void Play(AudioClip clip, float volume, float pitch, bool loop, bool isMusic, AudioMixerGroup mixerGroup);

        void StopAll();

        void PauseAll() => throw new NotSupportedException("This audio player does not support pausing playback.");

        void ResumeAll() => throw new NotSupportedException("This audio player does not support resuming playback.");
    }
}
