using Blackhole.SharedKernel.Composition;
using UnityEngine;
#if UNITY_EDITOR
using Unity.Multiplayer;
#endif

namespace Blackhole.ModuleSystem.Composition
{
    [CreateAssetMenu(fileName = "PlatformModuleManifest", menuName = "Blackhole/Modules/Module System/Platform Module Manifest", order = 2)]
    public class PlatformModuleManifestSO : ModuleManifestSO
    {
#if UNITY_EDITOR || !UNITY_SERVER
        [SerializeField] private ModuleInstallerSO _clientInstaller;
#endif
#if UNITY_EDITOR || UNITY_SERVER
        [SerializeField] private ModuleInstallerSO _serverInstaller;
#endif

#if UNITY_EDITOR
        public override ModuleInstallerSO Installer => MultiplayerRolesManager.ActiveMultiplayerRoleMask == MultiplayerRoleFlags.Server ? _serverInstaller : _clientInstaller;
#elif UNITY_SERVER
        public override ModuleInstallerSO Installer => _serverInstaller;
#else
        public override ModuleInstallerSO Installer => _clientInstaller;
#endif
    }
}
