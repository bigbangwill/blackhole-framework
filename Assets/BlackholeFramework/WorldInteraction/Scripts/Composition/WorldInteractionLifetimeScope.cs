using Blackhole.WorldInteraction.Application;
using Blackhole.WorldInteraction.Infrastructure;
using VContainer;
using VContainer.Unity;

namespace Blackhole.WorldInteraction.Composition
{
    /// <summary>
    /// Registers the WorldInteraction settings and services with the scene's VContainer scope.
    /// </summary>
    public class WorldInteractionLifetimeScope : LifetimeScope
    {
        /// <summary>
        /// Supplies the detection and interaction layer masks used by this module.
        /// </summary>
        public WorldInteractionSettingSO WorldInteractionSettingSO;

        /// <inheritdoc />
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(WorldInteractionSettingSO.DetectionLayerMask).Keyed(WorldInteractionLayerMasksEnum.DetectionLayer);
            builder.RegisterInstance(WorldInteractionSettingSO.InteractionLayerMask).Keyed(WorldInteractionLayerMasksEnum.InteractionLayer);

            builder.Register<IDetectionRaycaster, DetectionRaycaster>(Lifetime.Singleton);
        }
    }
}
