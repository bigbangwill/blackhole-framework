using Blackhole.SharedKernel.Composition;
using UnityEngine;

namespace Blackhole.ModuleSystem.Composition
{
    [CreateAssetMenu(
        fileName = "PlatformModuleManifest", 
        menuName = "Blackhole/Modules/Module System/Platform Module Manifest", order = 2)]
    public class PlatformModuleManifestSO : ModuleManifestSO
    {
        [SerializeField] private ModuleInstallerSO _clientInstaller;
        [SerializeField] private ModuleInstallerSO _serverInstaller;

#if UNITY_SERVER
        public override ModuleInstallerSO Installer => _serverInstaller;
#else

        public override ModuleInstallerSO Installer => _clientInstaller;
#endif
    }
}