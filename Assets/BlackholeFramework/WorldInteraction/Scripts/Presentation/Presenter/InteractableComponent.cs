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
        [Tooltip("Receive events without blocking targets behind this one or becoming selected.")]
        [SerializeField] private bool _passThrough;
        [Tooltip("Receive events even behind a blocking target, without becoming selected.")]
        [SerializeField] private bool _receiveAlongsideTarget;

        public bool PassThrough => _passThrough;
        public bool ReceiveAlongsideTarget => _receiveAlongsideTarget;

        private Action<WorldInteractionEventData> _onPointerDown;
        private Action<WorldInteractionEventData> _onPointerUp;
        private Action<WorldInteractionEventData> _onPointerClicked;

        private Action<WorldInteractionEventData> _onBeginDrag;
        private Action<WorldInteractionEventData> _onDrag;
        private Action<WorldInteractionEventData> _onEndDrag;
        private Action<WorldInteractionEventData> _onDeselect;

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
        public void AddBeginDragListener(Action<WorldInteractionEventData> listener)
        {
            _onBeginDrag += listener;
        }

        /// <inheritdoc />
        public void RemoveBeginDragListener(Action<WorldInteractionEventData> listener)
        {
            _onBeginDrag -= listener;
        }

        /// <inheritdoc />
        public void AddDragListener(Action<WorldInteractionEventData> listener)
        {
            _onDrag += listener;
        }

        /// <inheritdoc />
        public void RemoveDragListener(Action<WorldInteractionEventData> listener)
        {
            _onDrag -= listener;
        }

        /// <inheritdoc />
        public void AddEndDragListener(Action<WorldInteractionEventData> listener)
        {
            _onEndDrag += listener;
        }

        /// <inheritdoc />
        public void RemoveEndDragListener(Action<WorldInteractionEventData> listener)
        {
            _onEndDrag -= listener;
        }

        public void AddDeselectListener(Action<WorldInteractionEventData> listener) => _onDeselect += listener;

        public void RemoveDeselectListener(Action<WorldInteractionEventData> listener) => _onDeselect -= listener;

        /// <inheritdoc />
        public void OnPointerClicked(WorldInteractionContext context)
        {
            _onPointerClicked?.Invoke(CreateEventData(context));
        }

        /// <inheritdoc />
        public void OnPointerDown(WorldInteractionContext context)
        {
            _onPointerDown?.Invoke(CreateEventData(context));
        }

        /// <inheritdoc />
        public void OnPointerUp(WorldInteractionContext context, bool isHit)
        {
            _onPointerUp?.Invoke(CreateEventData(context, isHit));
        }

        /// <inheritdoc />
        public void OnBeginDrag(WorldInteractionContext context)
        {
            _onBeginDrag?.Invoke(CreateEventData(context));
        }

        /// <inheritdoc />
        public void OnDrag(WorldInteractionContext context)
        {
            _onDrag?.Invoke(CreateEventData(context));
        }

        /// <inheritdoc />
        public void OnEndDrag(WorldInteractionContext context, bool isHit)
        {
            _onEndDrag?.Invoke(CreateEventData(context, isHit));
        }

        public void Deselect(WorldInteractionContext context) => _onDeselect?.Invoke(CreateEventData(context));

        private static WorldInteractionEventData CreateEventData(WorldInteractionContext context, bool isHit = false)
        {
            return new WorldInteractionEventData(context.ScreenPosition, context.PointerId, isHit, (InteractionPointerButton)context.PointerButton);
        }
    }
}
