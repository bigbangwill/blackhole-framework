using Blackhole.AudioPlayer.Application;
using Blackhole.AudioPlayer.Application.Dto;
using Blackhole.AudioPlayer.Application.Ports;
using Blackhole.AudioPlayer.Application.Services;
using Blackhole.AudioPlayer.Application.UseCases;
using Blackhole.AudioPlayer.Contract.Interface;
using Blackhole.AudioPlayer.Infrastructure;
using Blackhole.AudioPlayer.Infrastructure.Adapters;
using Blackhole.AudioPlayer.Infrastructure.ScriptableObjects;
using Blackhole.AudioPlayer.Infrastructure.Unity;
using Blackhole.LocalStorageFile.Composition;
using Blackhole.SharedKernel.Composition;
using UnityEngine;
using UnityEngine.Audio;
using VContainer;
using VContainer.Unity;

namespace Blackhole.AudioPlayer.Composition
{
    [CreateAssetMenu(fileName = "AudioPlayerInstaller", menuName = "Blackhole/Modules/Installers/AudioPlayerInstaller")]
    public class AudioPlayerInstaller : ModuleInstallerSO
    {
        [SerializeField] private AudioConfigSO config;
        [SerializeField] private AudioMixer audioMixerInstance;
        [SerializeField] private SoundLibrarySO soundLibrary;

        public override void InstallModule(IContainerBuilder builder)
        {
            builder.RegisterInstance(RequireAsset(config, nameof(config))).As<AudioConfigSO>();

            builder.RegisterInstance(RequireAsset(audioMixerInstance, nameof(audioMixerInstance)));

            builder.RegisterInstance(RequireAsset(soundLibrary, nameof(soundLibrary))).As<SoundLibrarySO>();

            builder.Register<AudioSettingsData>(Lifetime.Singleton);

            builder.Register<SoundLibraryAdapter>(Lifetime.Scoped).As<ISoundLibraryPort>();
            builder.RegisterComponentOnNewGameObject<UnityAudioPool>(Lifetime.Scoped).DontDestroyOnLoad().As<IAudioPlayerPort>();
            builder.Register<AudioService>(Lifetime.Scoped).As<IAudioService>();

            builder.Register<IAudioVolumeUseCase, AudioVolumeUseCase>(Lifetime.Singleton);
            builder.Register<IAudioSettingsUseCase, AudioSettingsUseCase>(Lifetime.Singleton);
            builder.Register<IAudioSettingsController, AudioSettingsController>(Lifetime.Singleton);

            builder.Register<IAudioMixerPort, UnityAudioMixerAdapter>(Lifetime.Singleton);

            builder.Register<IAudioSettingRepository, AudioSettingsRepository>(Lifetime.Singleton);

            builder.RegisterEntryPoint<AudioModuleEntryPoint>();
        }
    }
}
