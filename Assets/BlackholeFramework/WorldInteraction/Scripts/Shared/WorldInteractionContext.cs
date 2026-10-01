using UnityEngine;

namespace Blackhole.WorldInteraction.Application
{
    /// <summary>
    /// Identifies the button responsible for a world-interaction event.
    /// </summary>
    public enum WorldInteractionPointerButton
    {
        /// <summary>The primary mouse button and the default value for touch input.</summary>
        Left,

        /// <summary>The secondary mouse button.</summary>
        Right,

        /// <summary>The middle mouse button.</summary>
        Middle
    }

    /// <summary>
    /// Carries the position and complete input identity through one interaction event.
    /// A pointer is uniquely identified by both <see cref="PointerId"/> and
    /// <see cref="PointerButton"/>.
    /// </summary>
    public readonly struct WorldInteractionContext
    {
        /// <summary>Gets the current pointer position in screen-space pixels.</summary>
        public Vector2 ScreenPosition { get; }

        /// <summary>Gets the EventSystem pointer or device identifier.</summary>
        public int PointerId { get; }

        /// <summary>Gets the button that generated the event.</summary>
        public WorldInteractionPointerButton PointerButton { get; }

        /// <summary>
        /// Creates a context for a pointer event.
        /// </summary>
        /// <param name="screenPosition">The pointer position in screen-space pixels.</param>
        /// <param name="pointerId">The EventSystem pointer or device identifier.</param>
        /// <param name="pointerButton">The button that generated the event.</param>
        public WorldInteractionContext(
            Vector2 screenPosition,
            int pointerId,
            WorldInteractionPointerButton pointerButton = WorldInteractionPointerButton.Left)
        {
            ScreenPosition = screenPosition;
            PointerId = pointerId;
            PointerButton = pointerButton;
        }
    }
}
