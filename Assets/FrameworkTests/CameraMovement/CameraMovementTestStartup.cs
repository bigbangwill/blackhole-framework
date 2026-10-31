using System.Threading;
using Blackhole.CameraController.Contract;
using Blackhole.CameraController.Implementation;
using Blackhole.ModuleSystem.Composition;
using Cysharp.Threading.Tasks;

namespace Blackhole.Development.CameraMovement
{
    public sealed class CameraMovementTestStartup : RootModuleStartup
    {
        private readonly CameraMovementTestView view;
        private readonly ICameraController controller;
        private readonly CameraControllerSettingsSO settings;

        public CameraMovementTestStartup(RootModuleStartupRegistry registry, CameraMovementTestView view, ICameraController controller, CameraControllerSettingsSO settings) : base(registry)
        {
            this.view = view;
            this.controller = controller;
            this.settings = settings;
        }

        public override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            view.Initialize(controller, settings.CreateMovementSetting());
            return UniTask.CompletedTask;
        }
    }
}
