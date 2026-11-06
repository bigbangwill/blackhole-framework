using Blackhole.SharedKernel.Composition;
using UnityEngine;
using VContainer;

namespace Blackhole.Multiplayer.Composition
{
    [CreateAssetMenu(fileName = "MultiplayerInstaller", menuName = "Blackhole/Modules/Installers/MultiplayerInstaller")]
    public sealed class MultiplayerInstaller : ModuleInstallerSO
    {
        public override void InstallModule(IContainerBuilder builder)
        {
        }
    }
}
