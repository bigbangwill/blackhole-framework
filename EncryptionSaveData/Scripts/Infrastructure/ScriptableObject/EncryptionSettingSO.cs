using Blackhole.Settings.Contract;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Blackhole.EncryptionSaveData.Infrastructure
{
    [ProjectSettings("Storage", 10)]
    [CreateAssetMenu(fileName = "EncryptionSettingSO", menuName = "Blackhole/Modules/Encryption Save Data/Settings")]
    public class EncryptionSettingSO : ScriptableObject
    {
        [SerializeField] private string key = "configure-a-project-specific-key";
        public byte[] Key
        {
            get
            {
                using (SHA256 sha = SHA256.Create())
                {
                    return sha.ComputeHash(Encoding.UTF8.GetBytes(key));
                }
            }
        }

    }
}
