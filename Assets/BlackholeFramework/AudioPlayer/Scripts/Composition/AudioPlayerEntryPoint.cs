using Blackhole.AudioPlayer.Application.UseCases;
using VContainer.Unity;

namespace Blackhole.AudioPlayer.Composition
{
    public class AudioModuleEntryPoint : IInitializable
    {
        private readonly IAudioSettingsUseCase settingsUseCase;

        public AudioModuleEntryPoint(IAudioSettingsUseCase settingsUseCase)
        {
            this.settingsUseCase = settingsUseCase;
        }

        public void Initialize()
        {
            settingsUseCase.Load();
        }
    }
}
