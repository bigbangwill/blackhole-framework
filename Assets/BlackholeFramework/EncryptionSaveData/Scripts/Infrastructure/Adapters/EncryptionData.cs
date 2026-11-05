using Blackhole.SharedKernel.Contract;
using Blackhole.EncryptionSaveData.Application;
using System;
using System.IO;
using System.Security.Cryptography;
using Blackhole.LocalStorageFile.Contract;

namespace Blackhole.EncryptionSaveData.Infrastructure
{
    public class EncryptionData : IDataEncryptor
    {
        private readonly byte[] key;

        public EncryptionData(EncryptionSettingSO setting)
        {
            key = setting.Key;
        }

        //AES 32 byte encryption
        public Result<byte[], StorageSaveResults> EncryptData(byte[] data)
        {
            if (!HasValidKey())
                return Result<byte[], StorageSaveResults>.Failure(StorageSaveResults.EncryptionKeyError);

            if (data == null)
                return Result<byte[], StorageSaveResults>.Failure(StorageSaveResults.EncryptionFail);

            try
            {
                using Aes aes = Aes.Create();
                aes.Key = key;

                // Generate a new IV every time and store it before the ciphertext.
                aes.GenerateIV();
                using MemoryStream output = new MemoryStream();
                output.Write(aes.IV, 0, aes.IV.Length);
                using (CryptoStream crypto = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    crypto.Write(data, 0, data.Length);
                    crypto.FlushFinalBlock();
                }
                return Result<byte[], StorageSaveResults>.Success(output.ToArray());
            }
            catch (CryptographicException)
            {
                return Result<byte[], StorageSaveResults>.Failure(StorageSaveResults.EncryptionFail);
            }
        }
        public Result<byte[], StorageSaveResults> DecryptData(byte[] encryptedData)
        {
            if (!HasValidKey())
                return Result<byte[], StorageSaveResults>.Failure(StorageSaveResults.EncryptionKeyError);

            const int ivLength = 16;
            if (encryptedData == null || encryptedData.Length < ivLength * 2 || (encryptedData.Length - ivLength) % ivLength != 0)
                return Result<byte[], StorageSaveResults>.Failure(StorageSaveResults.CorruptedData);

            try
            {
                using Aes aes = Aes.Create();
                aes.Key = key;

                // The first 16 bytes contain the IV.
                byte[] iv = new byte[ivLength];
                Array.Copy(encryptedData, 0, iv, 0, iv.Length);
                aes.IV = iv;
                using MemoryStream input = new MemoryStream(encryptedData, iv.Length, encryptedData.Length - iv.Length);
                using CryptoStream crypto = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using MemoryStream output = new MemoryStream();
                crypto.CopyTo(output);
                return Result<byte[], StorageSaveResults>.Success(output.ToArray());
            }
            catch (CryptographicException)
            {
                return Result<byte[], StorageSaveResults>.Failure(StorageSaveResults.DecryptionFail);
            }
        }

        private bool HasValidKey()
        {
            return key != null && (key.Length == 16 || key.Length == 24 || key.Length == 32);
        }
    }
}
