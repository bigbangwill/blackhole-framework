using Blackhole.AudioPlayer.Application.Dto;
using UnityEngine;

namespace Blackhole.AudioPlayer.Application.UseCases
{
    /// <summary>
    /// Handles saving, loading, and retrieving persisted audio settings.
    /// </summary>
    public class AudioSettingsUseCase : IAudioSettingsUseCase
    {
        private readonly IAudioVolumeUseCase _volumeUseCase;
        private readonly AudioSettingsData currentState;
        private readonly IAudioSettingRepository repository;


        public AudioSettingsUseCase(IAudioVolumeUseCase volumeUseCase, AudioSettingsData currentState, IAudioSettingRepository repository /*, AudioConfigSO config*/)
        {
            this._volumeUseCase = volumeUseCase;
            this.currentState = currentState;
            this.repository = repository;
        }


        /// <summary>
        /// Serializes and stores the current audio settings.
        /// </summary>
        public void Save()
        {
            repository.SaveAudioSettingsData(currentState);
        }


        /// <summary>
        /// Loads saved settings and applies volume values to the audio system.
        /// </summary>
        public void Load()
        {
            AudioSettingsData loaded = repository.GetAudioSettingsData();

            _volumeUseCase.ChangeMasterVolume(loaded.MasterVolume);
            _volumeUseCase.ChangeMusicVolume(loaded.MusicVolume);
            _volumeUseCase.ChangeSfxVolume(loaded.SfxVolume);

            _volumeUseCase.ToggleMasterMute(loaded.MasterMuted);
            _volumeUseCase.ToggleMusicMute(loaded.MusicMuted);
            _volumeUseCase.ToggleSfxMute(loaded.SfxMuted);
        }


        /// <summary>
        /// Retrieves saved audio settings from current state.
        /// </summary>
        public AudioSettingsData GetCurrentConfig()
        {
            return currentState;
        }
    }
}