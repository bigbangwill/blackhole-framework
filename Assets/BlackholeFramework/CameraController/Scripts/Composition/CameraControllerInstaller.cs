using Blackhole.SharedKernel.Composition;
using Blackhole.CameraController.Contract;
using Blackhole.CameraController.Implementation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Blackhole.CameraController.Composition
{
    [CreateAssetMenu(fileName = "CameraControllerInstaller", menuName = "Blackhole/Modules/Installers/CameraControllerInstaller")]
    public sealed class CameraControllerInstaller : ModuleInstallerSO
    {
        [SerializeField] private CameraControllerSettingsSO settings;
        [SerializeField] private CameraControllerComponent controllerPrefab;

        public override void InstallModule(IContainerBuilder builder)
        {
            builder.RegisterInstance(RequireAsset(settings, nameof(settings)));
            builder.RegisterComponentInNewPrefab(RequireAsset(controllerPrefab, nameof(controllerPrefab)), Lifetime.Singleton)
                .UnderTransform(GetCameraTransform)
                .AsSelf().As<ICameraController>();
        }

        private static Transform GetCameraTransform()
        {
            Camera camera = Camera.main;
            return camera != null ? camera.transform : null;
        }
    }
}
