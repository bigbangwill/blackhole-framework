using UnityEngine;

namespace Blackhole.WorldInteraction.Application
{
    /// <summary>
    /// Resolves pointer events against world-space interaction targets.
    /// Implementations capture pressed targets for drag and release events.
    /// </summary>
    public interface IDetectionRaycaster
    {
        Camera MainCamera { get; }

        void OnBeginDragRaycast(WorldInteractionContext context);
        void OnDragRaycast(WorldInteractionContext context);
        void OnEndDragRaycast(WorldInteractionContext context);

        /// <summary>
        /// Dispatches an EventSystem click to targets beneath the current pointer.
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
