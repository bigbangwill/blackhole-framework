using Blackhole.AudioPlayer.Application.Dto;
using Blackhole.AudioPlayer.Application.UseCases;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Blackhole.AudioPlayer.Presentation
{
    /// <summary>
    /// Handles UI interactions for audio volume and mute settings.
    /// </summary>
    public class AudioSettingsUI : MonoBehaviour
    {
        [Header("Sliders")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider musicSlider;


        [Header("Mute Toggles")]
        [SerializeField] private Toggle masterMuteToggle;
        [SerializeField] private Toggle sfxMuteToggle;
        [SerializeField] private Toggle musicMuteToggle;


        [Header("Save Button")]
        [SerializeField] private Button saveButton;

        [Inject] private readonly IAudioVolumeUseCase volumeUseCase;
        [Inject] private readonly IAudioSettingsUseCase settingsUseCase;


        /// <summary>
        /// Initializes UI controls and loads saved audio settings.
        /// </summary>
        private void Start()
        {
            SetupSlider(
                masterSlider,
                masterMuteToggle,
                volumeUseCase.ChangeMasterVolume,
                volumeUseCase.ToggleMasterMute);

            SetupSlider(
                sfxSlider,
                sfxMuteToggle,
                volumeUseCase.ChangeSfxVolume,
                volumeUseCase.ToggleSfxMute);

            SetupSlider(
                musicSlider,
                musicMuteToggle,
                volumeUseCase.ChangeMusicVolume,
                volumeUseCase.ToggleMusicMute);


            if (saveButton != null)
                saveButton.onClick.AddListener(SaveSettings);

            LoadSettings();
        }


        /// <summary>
        /// Connects a volume slider and mute toggle to their respective actions.
        /// </summary>
        private void SetupSlider(
            Slider slider,
            Toggle toggle,
            System.Action<float> onVolumeChanged,
            System.Action<bool> onMuteChanged)
        {
            slider.minValue = 0.0001f;
            slider.maxValue = 1f;
            slider.value = 0.75f;


            slider.onValueChanged.AddListener(value =>
            {
                // Changing volume automatically unmutes the channel.
                if (toggle.isOn)
                    toggle.SetIsOnWithoutNotify(false);

                onVolumeChanged(value);
            });


            toggle.onValueChanged.AddListener(muted =>
            {
                onMuteChanged(muted);
            });
        }


        /// <summary>
        /// Saves current state audio settings.
        /// </summary>
        private void SaveSettings()
        {
            settingsUseCase.Save();
        }


        /// <summary>
        /// Loads saved settings and applies them to the UI and audio system.
        /// </summary>
        private void LoadSettings()
        {
            AudioSettingsData data = settingsUseCase.GetCurrentConfig();

            masterSlider.SetValueWithoutNotify(data.MasterVolume);
            sfxSlider.SetValueWithoutNotify(data.SfxVolume);
            musicSlider.SetValueWithoutNotify(data.MusicVolume);

            masterMuteToggle.isOn = data.MasterMuted;
            sfxMuteToggle.isOn = data.SfxMuted;
            musicMuteToggle.isOn = data.MusicMuted;


            // Apply loaded mute states to the audio system.
            if (masterMuteToggle.isOn)
                volumeUseCase.ToggleMasterMute(true);

            if (sfxMuteToggle.isOn)
                volumeUseCase.ToggleSfxMute(true);

            if (musicMuteToggle.isOn)
                volumeUseCase.ToggleMusicMute(true);
        }
    }
}