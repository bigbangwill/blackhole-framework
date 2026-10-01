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

        /// <summary>
        /// Creates an interaction event payload.
        /// </summary>
        /// <param name="screenPosition">The pointer position in screen-space pixels.</param>
        public WorldInteractionEventData(Vector2 screenPosition)
        {
            ScreenPosition = screenPosition;
        }
    }
}
