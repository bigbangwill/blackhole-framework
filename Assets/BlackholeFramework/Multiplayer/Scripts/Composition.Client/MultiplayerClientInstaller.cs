using Blackhole.SharedKernel.Composition;
using UnityEngine;
using VContainer;

namespace Blackhole.Multiplayer.Composition.Client
{
    [CreateAssetMenu(fileName = "MultiplayerClientInstaller", menuName = "Blackhole/Modules/Installers/MultiplayerClientInstaller")]
    public class MultiplayerClientInstaller : ModuleInstallerSO
    {
        public override void InstallModule(IContainerBuilder builder)
        {
            
        }
    }
}