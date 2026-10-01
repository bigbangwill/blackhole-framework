using UnityEngine;
using UnityEngine.Audio;
using Blackhole.AudioPlayer.Application.Dto;

namespace Blackhole.AudioPlayer.Application.Ports
{
    /// <summary>
    /// Audio data returned by the sound library.
    /// </summary>
    public class SoundEntryDto
    {
        public AudioClip[] clips;
        public float volume;
        public float minPitch;
        public float maxPitch;
        public AudioMixerGroup mixerGroup;
        public SoundType type;
    }

    /// <summary>
    /// Abstraction for retrieving audio data by string identifier.
    /// </summary>
    public interface ISoundLibraryPort
    {
        bool TryGetEntry(string id, out SoundEntryDto entry);
    }
}