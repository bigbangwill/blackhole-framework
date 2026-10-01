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
        success
    }
}
