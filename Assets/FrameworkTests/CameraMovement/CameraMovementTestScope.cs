using VContainer;
using VContainer.Unity;
using UnityEngine;

namespace Blackhole.Development.CameraMovement
{
    public sealed class CameraMovementTestScope : LifetimeScope
    {
        [SerializeField] private CameraMovementTestView view;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(view);
            builder.RegisterEntryPoint<CameraMovementTestStartup>();
        }
    }
}
