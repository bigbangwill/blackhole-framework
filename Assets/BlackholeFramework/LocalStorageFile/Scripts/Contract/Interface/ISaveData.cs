using Blackhole.SharedKernel.Contract;
namespace Blackhole.LocalStorageFile.Contract
{
    public interface ISaveData
    {
        /// <summary>
        /// Saves the supplied bytes using the provided key.
        /// If data with the same key already exists, it is overwritten.
        /// </summary>
        /// <param name="key">The unique identifier used to store the data.</param>
        /// <param name="data">The bytes to save.</param>
        /// <returns>Success or the storage error that prevented saving.</returns>
        public Result<StorageSaveResults> SaveData(string key, byte[] data);

        /// <summary>
        /// Loads the bytes associated with the specified key.
        /// </summary>
        /// <param name="key">The unique identifier of the stored data.</param>
        /// <returns>The saved bytes or a storage error.</returns>
        public Result<byte[], StorageSaveResults> LoadData(string key);

        /// <summary>
        /// Determines whether data exists for the specified key.
        /// </summary>
        /// <param name="key">The unique identifier of the stored data.</param>
        /// <returns>
        /// <c>true</c> if data exists for the specified key; otherwise, <c>false</c>.
        /// </returns>
        public bool DataExists(string key);

        /// <summary>
        /// Deletes the data associated with the specified key if it exists.
        /// </summary>
        /// <param name="key">The unique identifier of the stored data.</param>
        public Result<StorageSaveResults> DeleteData(string key);
    }
}
