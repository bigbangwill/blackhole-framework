using Blackhole.Settings.Contract;
using System.IO;
using UnityEngine;

namespace Blackhole.LocalStorageFile.Infrastructure
{
    [ProjectSettings("Storage", 0)]
    [CreateAssetMenu(fileName = "LocalSaveSettingSO", menuName = "Blackhole/Modules/Local Storage File/Settings")]
    public class LocalSaveSettingSO : ScriptableObject
    {
        [SerializeField] private string extendedPath = "";
        public string RootPath => Path.Combine(UnityEngine.Application.persistentDataPath,extendedPath);
    }
}
