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
        public Result SaveData(string key, byte[] data)
        {
            Result<byte[], StorageSaveResults> encryption = _encryptData.EncryptData(data);
            if (encryption.IsSuccess)
            {
                byte[] encryptedData = encryption.Value;
                saveData.SaveData(key, encryptedData);
                return Result.Success();
            }
            else
            {
                return Result.Failure();
            }

        }

        /// <summary>
        /// Loads the encrypted data associated with the specified key and decrypts it.
        /// </summary>
        /// <param name="key">The unique key identifying the data to load.</param>
        /// <returns>The decrypted data.</returns>
        public Result<byte[], StorageSaveResults> LoadData(string key)
        {
            Result<byte[], StorageSaveResults> load = saveData.LoadData(key);

            if (load.Error == StorageSaveResults.NotFound)
                return Result<byte[], StorageSaveResults>.Failure(StorageSaveResults.NotFound);

            byte[] encryptedData = load.Value;
            Result<byte[], StorageSaveResults> decryption = _encryptData.DecryptData(encryptedData);
            if(decryption.IsSuccess)
            {
                byte[] decryptedData = decryption.Value;
                return Result<byte[], StorageSaveResults>.Success(decryptedData);
            }
            else
            {
                return Result<byte[], StorageSaveResults>.Failure(StorageSaveResults.DecryptionFail);
            }
          
        }

        public bool DataExists(string key)
        {
            return saveData.DataExists(key);
        }

        public Result<StorageSaveResults> DeleteData(string key)
        {
            Result<StorageSaveResults> delete = saveData.DeleteData(key);

            if (delete.Error == StorageSaveResults.NotFound)
                return Result<StorageSaveResults>.Failure(StorageSaveResults.NotFound);

            return Result<StorageSaveResults>.Success();
        }
    }
}
