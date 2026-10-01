using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Blackhole.AudioPlayer.Application.Dto;

namespace Blackhole.AudioPlayer.Infrastructure.ScriptableObjects
{
    [Serializable]
    public class SoundEntry
    {
        public string id;
        public SoundType type;
        public AudioMixerGroup mixerGroup;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0f, 3f)] public float minPitch = 0.9f;
        [Range(0f, 3f)] public float maxPitch = 1.1f;
        public AudioClip[] clips;
    }

    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "Blackhole/Modules/Audio Player/Sound Library")]
    public class SoundLibrarySO : ScriptableObject
    {
        [SerializeField] private List<SoundEntry> entries = new List<SoundEntry>();

        public bool TryGetEntry(string id, out SoundEntry entry)
        {
            entry = entries.Find(e => e.id == id);
            return entry != null;
        }

#if UNITY_EDITOR
        // This runs automatically in the Editor whenever you modify the asset.
        private void OnValidate()
        {
            foreach (var entry in entries)
            {
                if (entry.mixerGroup == null) continue;

                // Check if the mixer group name is exactly "Master" (case-insensitive)
                if (entry.mixerGroup.name.Equals("Master", StringComparison.OrdinalIgnoreCase))
                {
                    string errorMsg = $"Sound entry '{entry.id}' is assigned to the MASTER mixer group. " +
                                      $"Assign it to 'SFX' or 'Music' instead. " +
                                      $"Entry type is set to '{entry.type}'.";

                    Debug.LogError(errorMsg, this);
                    // throw new Exception(errorMsg); // Stops the Editor from continuing with invalid data
                }
            }
        }
#endif
    }
}
