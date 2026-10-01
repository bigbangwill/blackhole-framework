using Blackhole.SharedKernel.Contract;
using System;
namespace Blackhole.LocalStorageFile.Contract
{
    public interface ISaveData
    {
        /// <summary>
        /// Serializes the specified object into byte array and saves it using the provided key.
        /// If data with the same key already exists, it is overwritten.
        /// </summary>
        /// <typeparam name="T">The type of the object to save.</typeparam>
        /// <param name="key">The unique identifier used to store the data.</param>
        /// <param name="data">The object to serialize and save.</param>
        public Result SaveData(string key, byte[] data);

        /// <summary>
        /// Loads the data associated with the specified key and deserializes it into the requested type.
        /// </summary>
        /// <typeparam name="T">The type of the object to load.</typeparam>
        /// <param name="key">The unique identifier of the stored data.</param>
        /// <returns>The deserialized object.</returns>
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
