using Blackhole.SharedKernel.Contract;
using Blackhole.EncryptionSaveData.Application;
using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using Blackhole.LocalStorageFile.Contract;

namespace Blackhole.EncryptionSaveData.Infrastructure
{
    public class EncryptionData : IDataEncryptor
    {
        private readonly EncryptionSettingSO setting;
        private readonly byte[] key;

        public EncryptionData(EncryptionSettingSO setting)
        {
            this.setting = setting;
            key = setting.Key;
        }

        //AES 32 byte encryption
        public Result<byte[], StorageSaveResults> EncryptData(byte[] data)
        {

            if (key == null)
                Debug.Log(nameof(key));

            if (key.Length != 16 &&
                key.Length != 24 &&
                key.Length != 32)
            {
                return Result<byte[], StorageSaveResults>.Failure(StorageSaveResults.EncryptionKeyError);
            }

            using Aes aes = Aes.Create();
            aes.Key = key;

            // Generate a new IV every time.
            aes.GenerateIV();
            using MemoryStream output = new MemoryStream();

            // Write IV at the beginning.
            output.Write(aes.IV, 0, aes.IV.Length);
            using (CryptoStream crypto = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                crypto.Write(data, 0, data.Length);
                crypto.FlushFinalBlock();
            }
            return Result<byte[], StorageSaveResults>.Success(output.ToArray());
        }
        public Result<byte[], StorageSaveResults> DecryptData(byte[] encryptedData)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;

            if (key == null)
                Debug.Log(nameof(key));

            if (key.Length != 16 &&
                key.Length != 24 &&
                key.Length != 32)
            {
                return Result<byte[], StorageSaveResults>.Failure(StorageSaveResults.EncryptionKeyError);
            }

            // Read IV.The first 16 byt is IV.
            byte[] iv = new byte[16];
            Array.Copy(encryptedData, 0, iv, 0, iv.Length);
            aes.IV = iv;
            using MemoryStream input = new MemoryStream(encryptedData, iv.Length, encryptedData.Length - iv.Length);
            using CryptoStream crypto = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using MemoryStream output = new MemoryStream();
            crypto.CopyTo(output);
            return Result<byte[], StorageSaveResults>.Success(output.ToArray());
        }
    }
}
