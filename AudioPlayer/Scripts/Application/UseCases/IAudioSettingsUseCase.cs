using Blackhole.AudioPlayer.Application.Dto;

namespace Blackhole.AudioPlayer.Application.UseCases
{
    /// <summary>
    /// Defines operations for managing persisted audio settings.
    /// </summary>
    public interface IAudioSettingsUseCase
    {
        void Save();

        void Load();

        AudioSettingsData GetCurrentConfig();
    }
}