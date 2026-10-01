using Blackhole.SharedKernel.Composition;
using Blackhole.SharedKernel.Contract;
using Blackhole.LocalStorageFile.Application;
using Blackhole.LocalStorageFile.Infrastructure;
using Blackhole.LocalStorageFile.Contract;
using VContainer;
using UnityEngine;

namespace Blackhole.LocalStorageFile.Composition
{
    [CreateAssetMenu(fileName = "LocalStorageInstaller", menuName = "Blackhole/Modules/Installers/LocalStorageInstaller")]
    public class LocalStorageInstaller : ModuleInstallerSO
    {
        [SerializeField] private LocalSaveSettingSO localSaveSettingSO;

        public override void InstallModule(IContainerBuilder builder)
        {
            builder.RegisterInstance(RequireAsset(localSaveSettingSO, nameof(localSaveSettingSO)));
            builder.Register<ISaveData,SaveDataService>(Lifetime.Singleton);
            builder.Register<IDataSerializer, DataSerializer>(Lifetime.Singleton);
            builder.Register<IDataStorage,StorageData>(Lifetime.Singleton);  
        }
    }
}
