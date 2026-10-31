using UnityEngine;

namespace Blackhole.WorldInteraction.Contract
{
    /// <summary>
    /// Public event payload emitted by an interaction source.
    /// </summary>
    public readonly struct WorldInteractionEventData
    {
        /// <summary>
        /// Gets the pointer position in screen-space pixels when the event occurred.
        /// </summary>
        public Vector2 ScreenPosition { get; }

        /// <summary>Gets the identifier of the pointer that generated the event.</summary>
        public int PointerId { get; }

        /// <summary>Legacy name for the target-hit flag; this does not report UI hover.</summary>
        public bool IsPointerOverUI { get; }

        /// <summary>Gets whether the released pointer still hits this interaction target.</summary>
        public bool IsHit => IsPointerOverUI;

        public WorldInteractionEventData(Vector2 screenPosition, bool isPointerOverUI = false) : this(screenPosition, 0, isPointerOverUI) { }

        /// <summary>
        /// Creates an interaction event payload.
        /// </summary>
        /// <param name="screenPosition">The pointer position in screen-space pixels.</param>
        /// <param name="pointerId">The identifier supplied by the EventSystem.</param>
        /// <param name="isPointerOverUI">Indicates whether the released pointer still hits this target.</param>
        public WorldInteractionEventData(Vector2 screenPosition, int pointerId, bool isPointerOverUI = false)
        {
            ScreenPosition = screenPosition;
            PointerId = pointerId;
            IsPointerOverUI = isPointerOverUI;
        }
    }
}
