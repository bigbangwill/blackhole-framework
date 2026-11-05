using Blackhole.EncryptionSaveData.Contract;
using Blackhole.LocalStorageFile.Contract;
using Blackhole.SharedKernel.Contract;

namespace Blackhole.EncryptionSaveData.Application
{
    public class SaveEncryptData : ISaveEncrypted
    {
        private readonly IDataEncryptor _encryptData;
        private readonly ISaveData saveData;
        public SaveEncryptData(IDataEncryptor encryptData, ISaveData saveData)
        {
            _encryptData = encryptData;
            this.saveData = saveData;
        }

        /// <summary>
        /// Encrypts the specified data and saves it in storage.
        /// </summary>
        /// <param name="key">The unique key used to identify the saved data.</param>
        /// <param name="data">The data to encrypt and save.</param>
        public Result<StorageSaveResults> SaveData(string key, byte[] data)
        {
            Result<byte[], StorageSaveResults> encryption = _encryptData.EncryptData(data);
            if (encryption.IsFailure)
                return Result<StorageSaveResults>.Failure(encryption.Error);

            return saveData.SaveData(key, encryption.Value);
        }

        /// <summary>
        /// Loads the encrypted data associated with the specified key and decrypts it.
        /// </summary>
        /// <param name="key">The unique key identifying the data to load.</param>
        /// <returns>The decrypted data.</returns>
        public Result<byte[], StorageSaveResults> LoadData(string key)
        {
            Result<byte[], StorageSaveResults> load = saveData.LoadData(key);

            if (load.IsFailure)
                return Result<byte[], StorageSaveResults>.Failure(load.Error);

            return _encryptData.DecryptData(load.Value);
        }

        public bool DataExists(string key)
        {
            return saveData.DataExists(key);
        }

        public Result<StorageSaveResults> DeleteData(string key)
        {
            return saveData.DeleteData(key);
        }
    }
}
