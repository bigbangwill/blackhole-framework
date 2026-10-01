using Blackhole.SharedKernel.Composition;
using Blackhole.EncryptionSaveData.Application;
using Blackhole.EncryptionSaveData.Contract;
using Blackhole.EncryptionSaveData.Infrastructure;
using UnityEngine;
using VContainer;

namespace Blackhole.EncryptionSaveData.Composition
{
    [CreateAssetMenu(fileName = "EncryptionSaveInstaller", menuName = "Blackhole/Modules/Installers/EncryptionSaveInstaller")]
    public class EncryptionSaveInstaller : ModuleInstallerSO
    {
        [SerializeField] private EncryptionSettingSO settingData;

        public override void InstallModule(IContainerBuilder builder)
        {
            builder.RegisterInstance(RequireAsset(settingData, nameof(settingData)));
            builder.Register<IDataEncryptor, EncryptionData>(Lifetime.Singleton);
            builder.Register<ISaveEncrypted, SaveEncryptData>(Lifetime.Singleton);
        }
    }
}
