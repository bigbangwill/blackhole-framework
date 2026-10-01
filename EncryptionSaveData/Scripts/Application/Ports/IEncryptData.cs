using Blackhole.LocalStorageFile.Contract;
using Blackhole.SharedKernel.Contract;

namespace Blackhole.EncryptionSaveData.Application
{
    public interface IDataEncryptor
    {
        /// <summary>
        /// Encrypts the provided byte array.
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <returns>The encrypted byte array.</returns>
        public Result<byte[],StorageSaveResults> EncryptData(byte[] data);

        /// <summary>
        /// Decrypts the provided encrypted byte array.
        /// </summary>
        /// <param name="data">The encrypted data to decrypt.</param>
        /// <returns>The decrypted byte array.</returns>
        public Result<byte[], StorageSaveResults> DecryptData(byte[] data);
    }
}
