
namespace Blackhole.LocalStorageFile.Application
{
    public interface IDataStorage
    {
        /// <summary>
        /// Writes the specified data to a file associated with the provided key.
        /// If the save directory does not exist, it is created. If a file with the
        /// same key already exists, it is overwritten.
        /// </summary>
        /// <param name="key">The unique identifier used to name the save file.</param>
        /// <param name="data">The byte array to write to the file.</param>
        public void WriteData(string key, byte[] data);

        /// <summary>
        /// Reads the data associated with the specified key.
        /// </summary>
        /// <param name="key">The unique identifier of the save file.</param>
        /// <returns>The contents of the save file as a byte array.</returns>
        /// <exception cref="System.Exception">
        /// Thrown when no file exists for the specified key.
        /// </exception>
        public byte[] ReadData(string key);

        /// <summary>
        /// Determines whether a save file exists for the specified key.
        /// </summary>
        /// <param name="key">The unique identifier of the save file.</param>
        /// <returns>
        /// <c>true</c> if the save file exists; otherwise, <c>false</c>.
        /// </returns>
        public bool DataExits(string key);

        /// <summary>
        /// Deletes the save file associated with the specified key.
        /// </summary>
        /// <param name="key">The unique identifier of the save file.</param>
        /// <exception cref="System.Exception">
        /// Thrown when no file exists for the specified key.
        /// </exception>
        public void DeleteData(string key);
    }
}
