using UnityEngine;
using Blackhole.AudioPlayer.Contract.Interface;
using Blackhole.AudioPlayer.Application.Ports;
using Blackhole.AudioPlayer.Application.Dto;

namespace Blackhole.AudioPlayer.Application.Services
{
    /// <summary>
    /// Handles audio playback requests and coordinates between audio data and playback systems.
    /// </summary>
    public class AudioService : IAudioService
    {
        private readonly IAudioPlayerPort _audioPort;
        private readonly ISoundLibraryPort _soundLibrary;


        public AudioService(
            IAudioPlayerPort audioPort,
            ISoundLibraryPort soundLibrary)
        {
            _audioPort = audioPort;
            _soundLibrary = soundLibrary;
        }


        /// <summary>
        /// Plays a sound by retrieving its configuration and selecting playback parameters.
        /// </summary>
        public void Play(string id, float volumeOverride = -1f)
        {
            if (!_soundLibrary.TryGetEntry(id, out SoundEntryDto entry))
            {
                Debug.LogError($"Sound entry '{id}' not found!");
                return;
            }

            if (entry.clips == null || entry.clips.Length == 0)
            {
                Debug.LogError($"Sound entry '{id}' has no clips!");
                return;
            }

            bool isMusic = (entry.type == SoundType.Music);

            // Music uses a single clip, while SFX randomly selects from available variations.
            AudioClip clip = isMusic
                ? entry.clips[0]
                : entry.clips[Random.Range(0, entry.clips.Length)];

            float volume = volumeOverride >= 0 ? volumeOverride : entry.volume;
            float pitch = Random.Range(entry.minPitch, entry.maxPitch);

            _audioPort.Play(clip, volume, pitch, loop: isMusic, isMusic: isMusic, entry.mixerGroup);
        }

        public void StopAll() => _audioPort.StopAll();
    }
}