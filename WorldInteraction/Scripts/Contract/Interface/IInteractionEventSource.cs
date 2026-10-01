using System;

namespace Blackhole.WorldInteraction.Contract
{
    /// <summary>
    /// Exposes world-interaction callbacks without coupling consumers to Unity's EventSystem.
    /// </summary>
    public interface IInteractionEventSource
    {
        /// <summary>Adds a listener for pointer-down events.</summary>
        /// <param name="listener">The listener to add.</param>
        void AddPointerDownListener(Action<WorldInteractionEventData> listener);

        /// <summary>Removes a listener from pointer-down events.</summary>
        /// <param name="listener">The listener to remove.</param>
        void RemovePointerDownListener(Action<WorldInteractionEventData> listener);

        /// <summary>Adds a listener for pointer-up events.</summary>
        /// <param name="listener">The listener to add.</param>
        void AddPointerUpListener(Action<WorldInteractionEventData> listener);

        /// <summary>Removes a listener from pointer-up events.</summary>
        /// <param name="listener">The listener to remove.</param>
        void RemovePointerUpListener(Action<WorldInteractionEventData> listener);

        /// <summary>Adds a listener for completed pointer-click events.</summary>
        /// <param name="listener">The listener to add.</param>
        void AddPointerClickedListener(Action<WorldInteractionEventData> listener);

        /// <summary>Removes a listener from pointer-click events.</summary>
        /// <param name="listener">The listener to remove.</param>
        void RemovePointerClickedListener(Action<WorldInteractionEventData> listener);
    }
}
