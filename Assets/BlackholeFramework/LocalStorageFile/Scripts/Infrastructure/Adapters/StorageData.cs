using Blackhole.LocalStorageFile.Application;
using System;
using System.IO;

namespace Blackhole.LocalStorageFile.Infrastructure
{
    public class StorageData : IDataStorage
    {
        private readonly string rootPath;
        public StorageData(LocalSaveSettingSO settings)
        {
            rootPath = Path.GetFullPath(settings.RootPath);
        }

        public void WriteData(string key, byte[] data)
        {
            string path = GetPath(key);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, data);
        }

        public byte[] ReadData(string key)
        {
            return File.ReadAllBytes(GetPath(key));
        }

        public bool DataExits(string key)
        {
            return File.Exists(GetPath(key));
        }

        public void DeleteData(string key)
        {
            File.Delete(GetPath(key));
        }

        private string GetPath(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("A storage key is required.", nameof(key));

            string relativePath = key.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
            if (Path.IsPathRooted(relativePath))
                throw new ArgumentException("Storage keys must be relative paths.", nameof(key));

            string path = Path.GetFullPath(Path.Combine(rootPath, relativePath + ".sav"));
            string rootPrefix = rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            StringComparison comparison = Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (!path.StartsWith(rootPrefix, comparison))
                throw new ArgumentException("Storage keys must stay inside the save directory.", nameof(key));

            return path;
        }
    }
}
