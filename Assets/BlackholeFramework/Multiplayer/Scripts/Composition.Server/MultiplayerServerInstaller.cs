using Blackhole.SharedKernel.Composition;
using UnityEngine;
using VContainer;

namespace Blackhole.Multiplayer.Composition.Server
{
    [CreateAssetMenu(fileName = "MultiplayerServerInstaller", menuName = "Blackhole/Modules/Installers/MultiplayerServerInstaller")]
    public class MultiplayerServerInstaller : ModuleInstallerSO
    {
        public override void InstallModule(IContainerBuilder builder)
        {
            
        }
    }
}