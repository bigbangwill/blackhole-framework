using System;
using UnityEngine;
using Blackhole.WorldInteraction.Shared;
using Blackhole.WorldInteraction.Contract;
using Blackhole.WorldInteraction.Application;

namespace Blackhole.WorldInteraction.Presentation
{
    /// <summary>
    /// Marks a GameObject as an interaction target and exposes its internal detection
    /// callbacks through the module's consumer-facing event-source contract.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class InteractableComponent : MonoBehaviour, IInteractionTarget, IInteractionEventSource
    {
        private Action<WorldInteractionEventData> _onPointerDown;
        private Action<WorldInteractionEventData> _onPointerUp;
        private Action<WorldInteractionEventData> _onPointerClicked;

        /// <inheritdoc />
        public void AddPointerDownListener(Action<WorldInteractionEventData> listener)
        {
            _onPointerDown += listener;
        }

        /// <inheritdoc />
        public void RemovePointerDownListener(Action<WorldInteractionEventData> listener)
        {
            _onPointerDown -= listener;
        }

        /// <inheritdoc />
        public void AddPointerUpListener(Action<WorldInteractionEventData> listener)
        {
            _onPointerUp += listener;
        }

        /// <inheritdoc />
        public void RemovePointerUpListener(Action<WorldInteractionEventData> listener)
        {
            _onPointerUp -= listener;
        }

        /// <inheritdoc />
        public void AddPointerClickedListener(Action<WorldInteractionEventData> listener)
        {
            _onPointerClicked += listener;
        }

        /// <inheritdoc />
        public void RemovePointerClickedListener(Action<WorldInteractionEventData> listener)
        {
            _onPointerClicked -= listener;
        }

        /// <inheritdoc />
        public void OnPointerClicked(WorldInteractionContext context)
        {
            _onPointerClicked?.Invoke(new WorldInteractionEventData(context.ScreenPosition));
        }

        /// <inheritdoc />
        public void OnPointerDown(WorldInteractionContext context)
        {
            _onPointerDown?.Invoke(new WorldInteractionEventData(context.ScreenPosition));
        }

        /// <inheritdoc />
        public void OnPointerUp(WorldInteractionContext context)
        {
            _onPointerUp?.Invoke(new WorldInteractionEventData(context.ScreenPosition));
        }
    }
}
