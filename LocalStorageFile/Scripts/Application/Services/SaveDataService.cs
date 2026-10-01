using Blackhole.LocalStorageFile.Contract;
using Blackhole.SharedKernel.Contract;

namespace Blackhole.LocalStorageFile.Application
{
    public class SaveDataService : ISaveData
    {
        private readonly IDataStorage _dataStorage;

        public SaveDataService(IDataStorage dataStorage)
        {
            _dataStorage = dataStorage;
        }

        public Result SaveData(string key, byte[] data)
        {
            _dataStorage.WriteData(key, data);
            return Result.Success();
        }

        public Result<byte[], StorageSaveResults> LoadData(string key)
        {
            if (DataExists(key))
            {
                byte[] data = _dataStorage.ReadData(key);
                return Result<byte[], StorageSaveResults>.Success(data);
            }
            else
            {
                return Result<byte[], StorageSaveResults>.Failure(StorageSaveResults.NotFound);
            }

        }

        public bool DataExists(string key)
        {
            return _dataStorage.DataExits(key);
        }

        public Result<StorageSaveResults> DeleteData(string key)
        {
            if (_dataStorage.DataExits(key))
            {
                _dataStorage.DeleteData(key);
                return Result<StorageSaveResults>.Success();
            }
            else
                return Result<StorageSaveResults>.Failure(StorageSaveResults.NotFound);
        }

    }
}
