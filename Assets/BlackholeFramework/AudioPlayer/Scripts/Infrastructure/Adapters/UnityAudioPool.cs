using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Blackhole.AudioPlayer.Application.Ports;
using Blackhole.AudioPlayer.Infrastructure.ScriptableObjects;
using VContainer;

namespace Blackhole.AudioPlayer.Infrastructure.Adapters
{
    [DisallowMultipleComponent]
    public class UnityAudioPool : MonoBehaviour, IAudioPlayerPort, IDisposable
    {
        public AudioConfigSO Config { get; set; }

        private AudioSource[] _sfxSources;
        private int _sfxIndex;
        private AudioSource _musicSource;
        private readonly HashSet<AudioSource> _pausedSources = new();

        private void Awake()
        {
            if (Config != null)
                Initialize(Config);
        }

        [Inject]
        public void Initialize(AudioConfigSO config)
        {
            if (_sfxSources != null)
                return;

            Config = config != null ? config : throw new ArgumentNullException(nameof(config));
            int sfxCount = Mathf.Max(1, config.SfxChannelCount);

            // SFX Child
            GameObject sfxChild = new GameObject("SFX");
            sfxChild.transform.SetParent(transform, false);
            _sfxSources = new AudioSource[sfxCount];
            for (int i = 0; i < sfxCount; i++)
            {
                _sfxSources[i] = sfxChild.AddComponent<AudioSource>();
                _sfxSources[i].playOnAwake = false;
            }

            // Music Child
            GameObject musicChild = new GameObject("Music");
            musicChild.transform.SetParent(transform, false);
            _musicSource = musicChild.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
        }

        public void Play(AudioClip clip, float volume, float pitch, bool loop, bool isMusic, AudioMixerGroup mixerGroup)
        {
            if (_sfxSources == null)
                Initialize(Config != null ? Config : throw new InvalidOperationException("Configure the audio pool before starting playback."));

            if (isMusic)
            {
                bool wasPaused = _pausedSources.Remove(_musicSource);
                bool sameClip = _musicSource.clip == clip;
                bool alreadyPlaying = sameClip && _musicSource.isPlaying;
                if (!sameClip)
                {
                    _musicSource.Stop();
                    _musicSource.clip = clip;
                }

                _musicSource.outputAudioMixerGroup = mixerGroup;
                _musicSource.volume = volume;
                _musicSource.pitch = pitch;
                _musicSource.loop = loop;

                if (wasPaused && sameClip)
                    _musicSource.UnPause();
                else if (!alreadyPlaying)
                    _musicSource.Play();
            }
            else
            {
                AudioSource target = GetSfxSource();
                if (target == null)
                    return;

                target.Stop();
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
            if (_sfxSources == null)
                return;

            foreach (AudioSource source in _sfxSources)
                source.Stop();
            _musicSource.Stop();
            _pausedSources.Clear();
        }

        public void PauseAll()
        {
            if (_sfxSources == null)
                return;

            foreach (AudioSource source in _sfxSources)
                Pause(source);
            Pause(_musicSource);
        }

        public void ResumeAll()
        {
            foreach (AudioSource source in _pausedSources)
                source.UnPause();
            _pausedSources.Clear();
        }

        public void Dispose()
        {
            if (this == null)
                return;

            StopAll();
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private AudioSource GetSfxSource()
        {
            foreach (AudioSource source in _sfxSources)
            {
                if (!source.isPlaying && !_pausedSources.Contains(source))
                    return source;
            }

            for (int i = 0; i < _sfxSources.Length; i++)
            {
                AudioSource source = _sfxSources[_sfxIndex];
                _sfxIndex = (_sfxIndex + 1) % _sfxSources.Length;
                if (!_pausedSources.Contains(source))
                    return source;
            }
            return null;
        }

        private void Pause(AudioSource source)
        {
            if (!source.isPlaying)
                return;

            source.Pause();
            _pausedSources.Add(source);
        }
    }
}
