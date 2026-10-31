using Blackhole.WorldInteraction.Application;

namespace Blackhole.WorldInteraction.Shared
{
    /// <summary>
    /// Internal callback surface implemented by components that can receive detected
    /// world-interaction events.
    /// </summary>
    public interface IInteractionTarget
    {
        /// <summary>Handles the start of a pointer press over this target.</summary>
        /// <param name="context">The pointer event context.</param>
        public void OnPointerDown(WorldInteractionContext context);

        /// <summary>Handles release of a pointer that previously pressed this target.</summary>
        /// <param name="context">The pointer event context.</param>
        public void OnPointerUp(WorldInteractionContext context, bool isHit);

        /// <summary>Handles an EventSystem click detected over this target.</summary>
        /// <param name="context">The pointer event context.</param>
        public void OnPointerClicked(WorldInteractionContext context);

        public void OnBeginDrag(WorldInteractionContext context);
        public void OnDrag(WorldInteractionContext context);
        public void OnEndDrag(WorldInteractionContext context, bool isHit);
    }
}
