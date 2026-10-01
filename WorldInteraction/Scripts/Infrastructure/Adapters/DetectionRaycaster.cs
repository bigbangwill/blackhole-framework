using System.Collections.Generic;
using Blackhole.WorldInteraction.Application;
using Blackhole.WorldInteraction.Shared;
using UnityEngine;
using VContainer;

namespace Blackhole.WorldInteraction.Infrastructure
{
    /// <summary>
    /// Uses 2D overlap queries to translate pointer input into callbacks on
    /// <see cref="IInteractionTarget"/> components.
    /// </summary>
    public sealed class DetectionRaycaster : IDetectionRaycaster
    {
        // Mouse buttons can share one pointer ID in InputSystemUIInputModule, so the
        // button is part of the key. Touch input continues to use the Left value.
        private readonly Dictionary<(int PointerId, WorldInteractionPointerButton Button), HashSet<Collider2D>> _pressedCollidersByInput = new();

        // Reused by every query to avoid allocating a Collider2D array per pointer event.
        private readonly List<Collider2D> _raycastHits = new();

        private ContactFilter2D _contactFilter;
        private Camera _camera;

        /// <summary>
        /// Creates a raycaster restricted to the configured interaction layers.
        /// </summary>
        /// <param name="interactableLayerMask">Layers containing interaction targets.</param>
        public DetectionRaycaster([Key(WorldInteractionLayerMasksEnum.InteractionLayer)] LayerMask interactableLayerMask)
        {
            _contactFilter = new ContactFilter2D();
            _contactFilter.SetLayerMask(interactableLayerMask);
        }

        /// <inheritdoc />
        public void OnPointerDownRaycast(WorldInteractionContext context)
        {
            var pressedColliders = GetOrCreatePressedColliders(context);
            pressedColliders.Clear();

            Raycast(context.ScreenPosition);
            foreach (var hit in _raycastHits)
            {
                if (pressedColliders.Add(hit) && hit.TryGetComponent<IInteractionTarget>(out var target))
                    target.OnPointerDown(context);
            }
        }

        /// <inheritdoc />
        public void OnPointerUpRaycast(WorldInteractionContext context)
        {
            var input = GetInputKey(context);
            if (!_pressedCollidersByInput.TryGetValue(input, out var pressedColliders))
                return;

            // Every collider that received pointer-down must also receive pointer-up.
            // Whether the pointer is still over it only affects the click callback.
            foreach (var collider in pressedColliders)
            {
                if (collider != null && collider.TryGetComponent<IInteractionTarget>(out var target))
                    target.OnPointerUp(context);
            }
        }

        /// <inheritdoc />
        public void OnPointerClickedRaycast(WorldInteractionContext context)
        {
            var input = GetInputKey(context);
            if (!_pressedCollidersByInput.TryGetValue(input, out var pressedColliders))
                return;

            // Pointer-click is the final event in this interaction sequence.
            _pressedCollidersByInput.Remove(input);

            Raycast(context.ScreenPosition);
            foreach (var hit in _raycastHits)
            {
                if (pressedColliders.Contains(hit) && hit.TryGetComponent<IInteractionTarget>(out var target))
                    target.OnPointerClicked(context);
            }
        }

        private HashSet<Collider2D> GetOrCreatePressedColliders(WorldInteractionContext context)
        {
            var input = GetInputKey(context);
            if (_pressedCollidersByInput.TryGetValue(input, out var pressedColliders))
                return pressedColliders;

            pressedColliders = new HashSet<Collider2D>();
            _pressedCollidersByInput.Add(input, pressedColliders);
            return pressedColliders;
        }

        private static (int PointerId, WorldInteractionPointerButton Button) GetInputKey(WorldInteractionContext context)
        {
            return (context.PointerId, context.PointerButton);
        }

        private void Raycast(Vector2 screenPosition)
        {
            // Cache the tagged camera, but reacquire it if Unity destroys the object.
            if (_camera == null)
                _camera = Camera.main;

            _raycastHits.Clear();
            if (_camera == null)
                return;

            // Match the global trigger-query behavior used by the allocating overlap APIs.
            _contactFilter.useTriggers = Physics2D.queriesHitTriggers;

            var worldPosition = _camera.ScreenToWorldPoint(screenPosition);
            Physics2D.OverlapPoint(worldPosition, _contactFilter, _raycastHits);
        }
    }
}
