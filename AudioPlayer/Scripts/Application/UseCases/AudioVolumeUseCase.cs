using Blackhole.AudioPlayer.Application.Dto;
using Blackhole.AudioPlayer.Application.Ports;
using Blackhole.AudioPlayer.Contract.Interface;
using UnityEngine;

namespace Blackhole.AudioPlayer.Application.UseCases
{
    public class AudioVolumeUseCase : IAudioVolumeUseCase
    {
        private readonly IAudioMixerPort audioMixerPort;
        private readonly AudioSettingsData currentState;


        public AudioVolumeUseCase(IAudioMixerPort audioMixerPort, AudioSettingsData currentState)
        {
            this.audioMixerPort = audioMixerPort;
            this.currentState = currentState;
        }


        public void ChangeMasterVolume(float value)
        {
            currentState.MasterVolume = value;
            audioMixerPort.SetMasterVolume(value);

        }


        public void ChangeMusicVolume(float value)
        {
            currentState.MusicVolume = value;
            audioMixerPort.SetMusicVolume(value);
        }


        public void ChangeSfxVolume(float value)
        {
            currentState.SfxVolume = value;
            audioMixerPort.SetSfxVolume(value);
        }


        public void ToggleMasterMute(bool muted)
        {
            currentState.MasterMuted = muted;
            audioMixerPort.SetMasterMute(muted, currentState.MasterVolume);
        }


        public void ToggleMusicMute(bool muted)
        {
            currentState.MusicMuted = muted;
            audioMixerPort.SetMusicMute(muted, currentState.MusicVolume);
        }


        public void ToggleSfxMute(bool muted)
        {
            currentState.SfxMuted = muted;
            audioMixerPort.SetSfxMute(muted, currentState.SfxVolume);
        }

        public float GetMasterVolume()
        {
            return currentState.MasterVolume;
        }

        public float GetMusicVolume()
        {
            return currentState.MusicVolume;
        }

        public float GetSfxVolume()
        {
            return currentState.SfxVolume;
        }
    }
}