using Blackhole.AudioPlayer.Application;
using Blackhole.AudioPlayer.Application.Dto;
using Blackhole.AudioPlayer.Infrastructure.ScriptableObjects;
using Blackhole.LocalStorageFile.Contract;
using Blackhole.SharedKernel.Contract;

namespace Blackhole.AudioPlayer.Infrastructure
{
    public class AudioSettingsRepository : IAudioSettingRepository
    {
        private readonly IDataSerializer serializer;
        private readonly ISaveData saveData;
        private readonly AudioConfigSO config;

        public AudioSettingsRepository(IDataSerializer serializer, ISaveData saveData, AudioConfigSO config)
        {
            this.serializer = serializer;
            this.saveData = saveData;
            this.config = config;
        }

        public AudioSettingsData GetAudioSettingsData()
        {
            if (!saveData.DataExists(config.SettingsKey))
            {
                return new AudioSettingsData();
            }
            byte[] data = saveData.LoadData(config.SettingsKey).Value;
            return serializer.DeserializeData<AudioSettingsData>(data).Value;
        }

        public void SaveAudioSettingsData(AudioSettingsData data)
        {
            byte[] serializedData = serializer.SerializeData(data).Value;
            saveData.SaveData(config.SettingsKey, serializedData);
        }
    }
}