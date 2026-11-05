using Blackhole.LocalStorageFile.Contract;
using Blackhole.SharedKernel.Contract;
using System;
using System.IO;
using System.Security;

namespace Blackhole.LocalStorageFile.Application
{
    public class SaveDataService : ISaveData
    {
        private readonly IDataStorage _dataStorage;

        public SaveDataService(IDataStorage dataStorage)
        {
            _dataStorage = dataStorage;
        }

        public Result<StorageSaveResults> SaveData(string key, byte[] data)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Result<StorageSaveResults>.Failure(StorageSaveResults.InvalidKey);

            if (data == null)
                return Result<StorageSaveResults>.Failure(StorageSaveResults.InvalidData);

            try
            {
                _dataStorage.WriteData(key, data);
                return Result<StorageSaveResults>.Success();
            }
            catch (Exception exception) when (IsStorageException(exception))
            {
                return Result<StorageSaveResults>.Failure(MapSaveError(exception));
            }
        }

        public Result<byte[], StorageSaveResults> LoadData(string key)
        {
            try
            {
                byte[] data = _dataStorage.ReadData(key);
                if (data == null)
                    return Result<byte[], StorageSaveResults>.Failure(StorageSaveResults.NotFound);

                return Result<byte[], StorageSaveResults>.Success(data);
            }
            catch (FileNotFoundException)
            {
                return Result<byte[], StorageSaveResults>.Failure(StorageSaveResults.NotFound);
            }
            catch (DirectoryNotFoundException)
            {
                return Result<byte[], StorageSaveResults>.Failure(StorageSaveResults.NotFound);
            }
            catch (Exception exception) when (IsStorageException(exception))
            {
                return Result<byte[], StorageSaveResults>.Failure(StorageSaveResults.StorageError);
            }
        }

        public bool DataExists(string key)
        {
            try
            {
                return _dataStorage.DataExits(key);
            }
            catch (Exception exception) when (IsStorageException(exception))
            {
                return false;
            }
        }

        public Result<StorageSaveResults> DeleteData(string key)
        {
            try
            {
                if (!_dataStorage.DataExits(key))
                    return Result<StorageSaveResults>.Failure(StorageSaveResults.NotFound);

                _dataStorage.DeleteData(key);
                return Result<StorageSaveResults>.Success();
            }
            catch (Exception exception) when (IsStorageException(exception))
            {
                return Result<StorageSaveResults>.Failure(StorageSaveResults.StorageError);
            }
        }

        private static StorageSaveResults MapSaveError(Exception exception)
        {
            return exception switch
            {
                UnauthorizedAccessException _ => StorageSaveResults.AccessDenied,
                SecurityException _ => StorageSaveResults.AccessDenied,
                PathTooLongException _ => StorageSaveResults.PathTooLong,
                FileNotFoundException _ => StorageSaveResults.NotFound,
                DirectoryNotFoundException _ => StorageSaveResults.NotFound,
                ArgumentException _ => StorageSaveResults.InvalidKey,
                NotSupportedException _ => StorageSaveResults.InvalidKey,
                _ => StorageSaveResults.StorageError
            };
        }

        private static bool IsStorageException(Exception exception)
        {
            return exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException ||
                exception is NotSupportedException || exception is SecurityException;
        }
    }
}
