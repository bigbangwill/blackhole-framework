namespace Blackhole.WorldInteraction.Application
{
    /// <summary>
    /// Resolves pointer events against world-space interaction targets.
    /// Implementations preserve press state so subsequent pointer-up and click events
    /// are dispatched to the targets involved in the matching pointer sequence.
    /// </summary>
    public interface IDetectionRaycaster
    {
        /// <summary>
        /// Dispatches a click to targets hit both when the pointer went down and when it clicked.
        /// </summary>
        /// <param name="context">The pointer identity and current screen position.</param>
        void OnPointerClickedRaycast(WorldInteractionContext context);

        /// <summary>
        /// Records and notifies the targets beneath a pointer when it is pressed.
        /// </summary>
        /// <param name="context">The pointer identity and current screen position.</param>
        void OnPointerDownRaycast(WorldInteractionContext context);

        /// <summary>
        /// Notifies every target that received the matching pointer-down event.
        /// </summary>
        /// <param name="context">The pointer identity and current screen position.</param>
        void OnPointerUpRaycast(WorldInteractionContext context);
    }
}
