using Blackhole.AudioPlayer.Application.Ports;
using Blackhole.AudioPlayer.Infrastructure.ScriptableObjects;
using UnityEngine;
using UnityEngine.Audio;

namespace Blackhole.AudioPlayer.Infrastructure.Unity
{
    public class UnityAudioMixerAdapter : IAudioMixerPort
    {
        private readonly AudioMixer audioMixer;
        private readonly AudioConfigSO config;


        public UnityAudioMixerAdapter(AudioMixer audioMixer, AudioConfigSO config)
        {
            this.audioMixer = audioMixer;
            this.config = config;
        }


        public void SetMasterVolume(float value)
        {
            audioMixer.SetFloat(
                config.MasterVolumeParameter,
                ConvertVolumeToDecibel(value));
        }


        public void SetMusicVolume(float value)
        {
            audioMixer.SetFloat(
                config.MusicVolumeParameter,
                ConvertVolumeToDecibel(value));
        }


        public void SetSfxVolume(float value)
        {
            audioMixer.SetFloat(
                config.SfxVolumeParameter,
                ConvertVolumeToDecibel(value));
        }


        public void SetMasterMute(bool muted, float volume)
        {
            float dB = muted
            ? -80f
            : ConvertVolumeToDecibel(volume);

            audioMixer.SetFloat(
                config.MasterVolumeParameter,
                Mathf.Max(dB, -80f));
        }


        public void SetMusicMute(bool muted, float volume)
        {
            float dB = muted
                ? -80f
                : ConvertVolumeToDecibel(volume);

            audioMixer.SetFloat(
                config.MusicVolumeParameter,
                Mathf.Max(dB, -80f));
        }


        public void SetSfxMute(bool muted, float volume)
        {
            float dB = muted
            ? -80f
            : ConvertVolumeToDecibel(volume);

            audioMixer.SetFloat(
                config.SfxVolumeParameter,
                Mathf.Max(dB, -80f));
        }


        private float ConvertVolumeToDecibel(float value)
        {
            if (value <= 0.0001f)
                return -80f;

            return Mathf.Log10(value) * 20f;
        }
    }
}