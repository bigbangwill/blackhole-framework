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

        /// <summary>Adds a listener for begin-drag events.</summary>
        /// <param name="listener">The listener to add.</param>
        void AddBeginDragListener(Action<WorldInteractionEventData> listener);

        /// <summary>Removes a listener from begin-drag events.</summary>
        /// <param name="listener">The listener to remove.</param>
        void RemoveBeginDragListener(Action<WorldInteractionEventData> listener);

        /// <summary>Adds a listener for drag events.</summary>
        /// <param name="listener">The listener to add.</param>
        void AddDragListener(Action<WorldInteractionEventData> listener);

        /// <summary>Removes a listener from drag events.</summary>
        /// <param name="listener">The listener to remove.</param>
        void RemoveDragListener(Action<WorldInteractionEventData> listener);

        /// <summary>Adds a listener for end-drag events.</summary>
        /// <param name="listener">The listener to add.</param>
        void AddEndDragListener(Action<WorldInteractionEventData> listener);

        /// <summary>Removes a listener from end-drag events.</summary>
        /// <param name="listener">The listener to remove.</param>
        void RemoveEndDragListener(Action<WorldInteractionEventData> listener);

        /// <summary>Subscribes to deselection on sources that track selection.</summary>
        void AddDeselectListener(Action<WorldInteractionEventData> listener) { }

        /// <summary>Unsubscribes from deselection.</summary>
        void RemoveDeselectListener(Action<WorldInteractionEventData> listener) { }
    }
}
