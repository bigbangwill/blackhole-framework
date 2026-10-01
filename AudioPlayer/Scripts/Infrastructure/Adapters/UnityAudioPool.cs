using UnityEngine;
using UnityEngine.Audio;
using Blackhole.AudioPlayer.Application.Ports;
using Blackhole.AudioPlayer.Infrastructure.ScriptableObjects;
using VContainer;

namespace Blackhole.AudioPlayer.Infrastructure.Adapters
{
    public class UnityAudioPool : MonoBehaviour, IAudioPlayerPort
    {
        [Inject] public AudioConfigSO Config { get; set; }

        private AudioSource[] _sfxSources;
        private int _sfxIndex;
        private AudioSource _musicSource;

        void Awake()
        {
            int sfxCount = Config.SfxChannelCount;
            int musicCount = Config.MusicChannelCount;

            // SFX Child
            GameObject sfxChild = new GameObject("SFX");
            sfxChild.transform.SetParent(transform);
            _sfxSources = new AudioSource[sfxCount];
            for (int i = 0; i < sfxCount; i++)
            {
                _sfxSources[i] = sfxChild.AddComponent<AudioSource>();
                _sfxSources[i].playOnAwake = false;
            }

            // Music Child
            GameObject musicChild = new GameObject("Music");
            musicChild.transform.SetParent(transform);
            _musicSource = musicChild.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
        }

        public void Play(AudioClip clip, float volume, float pitch, bool loop, bool isMusic, AudioMixerGroup mixerGroup)
        {
            if (isMusic)
            {
                if (_musicSource.isPlaying) _musicSource.Stop();
                _musicSource.outputAudioMixerGroup = mixerGroup;
                _musicSource.clip = clip;
                _musicSource.volume = volume;
                _musicSource.pitch = pitch;
                _musicSource.loop = loop;
                _musicSource.Play();
            }
            else
            {
                AudioSource target = null;
                for (int i = 0; i < _sfxSources.Length; i++)
                {
                    if (!_sfxSources[i].isPlaying)
                    {
                        target = _sfxSources[i];
                        break;
                    }
                }

                if (target == null)
                {
                    target = _sfxSources[_sfxIndex];
                    _sfxIndex = (_sfxIndex + 1) % _sfxSources.Length;
                }

                target.outputAudioMixerGroup = mixerGroup;
                target.clip = clip;
                target.volume = volume;
                target.pitch = pitch;
                target.loop = loop;
                target.Play();
            }
        }

        public void StopAll()
        {
            foreach (var s in _sfxSources) s.Stop();
            _musicSource.Stop();
        }
    }
}