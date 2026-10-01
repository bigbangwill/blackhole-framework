using Blackhole.LocalStorageFile.Application;
using System.IO;


namespace Blackhole.LocalStorageFile.Infrastructure
{
    public class StorageData : IDataStorage
    {
        private readonly LocalSaveSettingSO settings;
        private readonly string rootPath;
        public StorageData(LocalSaveSettingSO settings)
        {
            this.settings = settings;
            rootPath = settings.RootPath ;
        }

        public void WriteData(string key, byte[] data)
        {
            if(!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }
            string path = Path.Combine(rootPath, key + ".sav");
            File.WriteAllBytes(path, data);

        }

        public byte[] ReadData(string key)
        {
            string path = Path.Combine(rootPath, key + ".sav");
            if(DataExits(key))
            {
                return File.ReadAllBytes(path);
            }
            else return null;
        }

        public bool DataExits(string key)
        {
            string path = Path.Combine(rootPath, key + ".sav");
            if (File.Exists(path))
                return true;
            else return false;
        }

        public void DeleteData(string key)
        {
            string path = Path.Combine(rootPath, key + ".sav");
            if (DataExits(key))
                File.Delete(path);
        }
    }
}
