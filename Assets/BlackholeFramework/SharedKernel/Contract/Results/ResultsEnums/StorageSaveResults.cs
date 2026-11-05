using UnityEngine;

namespace Blackhole.LocalStorageFile.Contract
{
   public enum StorageSaveResults
    {
        NotFound,
        CorruptedData,
        EncryptionKeyError,
        EncryptionFail,
        DecryptionFail,
        success,
        StorageError,
        InvalidKey,
        InvalidData,
        AccessDenied,
        PathTooLong
    }
}
