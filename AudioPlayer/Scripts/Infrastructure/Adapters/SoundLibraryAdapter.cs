using Blackhole.AudioPlayer.Application.Ports;
using Blackhole.AudioPlayer.Infrastructure.ScriptableObjects;
using UnityEngine;

namespace Blackhole.AudioPlayer.Infrastructure.Adapters
{
    public class SoundLibraryAdapter : ISoundLibraryPort
    {
        private readonly SoundLibrarySO _soundLibrary;

        public SoundLibraryAdapter(SoundLibrarySO soundLibrary)
        {
            _soundLibrary = soundLibrary;
        }

        public bool TryGetEntry(string id, out SoundEntryDto entry)
        {
            if (_soundLibrary.TryGetEntry(id, out SoundEntry soEntry))
            {
                entry = new SoundEntryDto
                {
                    clips = soEntry.clips,
                    volume = soEntry.volume,
                    minPitch = soEntry.minPitch,
                    maxPitch = soEntry.maxPitch,
                    mixerGroup = soEntry.mixerGroup,
                    type = soEntry.type
                };
                return true;
            }
            entry = null;
            return false;
        }
    }
}