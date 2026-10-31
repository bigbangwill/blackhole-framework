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
        private readonly Dictionary<(int PointerId, WorldInteractionPointerButton Button), Dictionary<Collider2D, IInteractionTarget>> _pressedCollidersByInput = new();
        private readonly Dictionary<(int PointerId, WorldInteractionPointerButton Button), Dictionary<Collider2D, IInteractionTarget>> _draggedCollidersByInput = new();

        // Reused by every query to avoid allocating a Collider2D array per pointer event.
        private readonly List<Collider2D> _raycastHits = new();

        private ContactFilter2D _contactFilter;
        private Camera _camera;

        public Camera MainCamera
        {
            get
            {
                if (_camera == null)
                    _camera = Camera.main;

                return _camera;
            }
        }

        /// <summary>
        /// Creates a raycaster restricted to the configured interaction layers.
        /// </summary>
        /// <param name="interactableLayerMask">Layers containing interaction targets.</param>
        public DetectionRaycaster([Key(WorldInteractionLayerMasksEnum.InteractionLayer)] LayerMask interactableLayerMask)
        {
            _contactFilter = new ContactFilter2D();
            _contactFilter.SetLayerMask(interactableLayerMask);
        }

        public void OnBeginDragRaycast(WorldInteractionContext context)
        {
            var input = GetInputKey(context);
            if (!_pressedCollidersByInput.TryGetValue(input, out var pressedColliders))
                return;

            // Capture the original targets even when the pointer has left their colliders.
            _draggedCollidersByInput[input] = new Dictionary<Collider2D, IInteractionTarget>(pressedColliders);
            foreach (var hit in pressedColliders)
            {
                if (hit.Key != null)
                    hit.Value.OnBeginDrag(context);
            }
        }

        public void OnDragRaycast(WorldInteractionContext context)
        {
            var input = GetInputKey(context);
            if (!_draggedCollidersByInput.TryGetValue(input, out var pressedColliders))
                return;

            foreach (var colDic in pressedColliders)
            {
                if (colDic.Key != null)
                    colDic.Value.OnDrag(context);
            }
        }

        public void OnEndDragRaycast(WorldInteractionContext context)
        {
            var input = GetInputKey(context);
            if (!_draggedCollidersByInput.TryGetValue(input, out var pressedColliders))
                return;

            _draggedCollidersByInput.Remove(input);
            Raycast(context.ScreenPosition);

            foreach (var colDic in pressedColliders)
            {
                if (colDic.Key != null)
                {
                    bool isHit = _raycastHits.Contains(colDic.Key);
                    colDic.Value.OnEndDrag(context, isHit);
                }
            }
        }

        /// <inheritdoc />
        public void OnPointerDownRaycast(WorldInteractionContext context)
        {
            _draggedCollidersByInput.Remove(GetInputKey(context));
            var pressedColliders = GetOrCreatePressedColliders(context);
            pressedColliders.Clear();

            Raycast(context.ScreenPosition);
            foreach (var hit in _raycastHits)
            {
                if (!pressedColliders.ContainsKey(hit) && hit.TryGetComponent<IInteractionTarget>(out var target))
                {
                    pressedColliders.Add(hit, target);
                    target.OnPointerDown(context);
                }
            }
        }

        /// <inheritdoc />
        public void OnPointerUpRaycast(WorldInteractionContext context)
        {
            var input = GetInputKey(context);
            if (!_pressedCollidersByInput.TryGetValue(input, out var pressedColliders))
                return;

            _pressedCollidersByInput.Remove(input);
            Raycast(context.ScreenPosition);

            // Every collider that received pointer-down must also receive pointer-up.
            // Report whether the released pointer still hits each original target.
            foreach (var colDic in pressedColliders)
            {
                if (colDic.Key != null)
                {
                    bool isHit = _raycastHits.Contains(colDic.Key);
                    colDic.Value.OnPointerUp(context, isHit);
                }
            }
        }

        /// <inheritdoc />
        public void OnPointerClickedRaycast(WorldInteractionContext context)
        {
            Raycast(context.ScreenPosition);
            foreach (var hit in _raycastHits)
            {
                if (hit.TryGetComponent<IInteractionTarget>(out var target))
                {
                    target.OnPointerClicked(context);
                }
            }
        }

        private Dictionary<Collider2D, IInteractionTarget> GetOrCreatePressedColliders(WorldInteractionContext context)
        {
            var input = GetInputKey(context);
            if (_pressedCollidersByInput.TryGetValue(input, out var pressedColliders))
                return pressedColliders;

            pressedColliders = new Dictionary<Collider2D, IInteractionTarget>();
            _pressedCollidersByInput.Add(input, pressedColliders);
            return pressedColliders;
        }

        private static (int PointerId, WorldInteractionPointerButton Button) GetInputKey(WorldInteractionContext context)
        {
            return (context.PointerId, context.PointerButton);
        }

        private void Raycast(Vector2 screenPosition)
        {
            Camera camera = MainCamera;
            _raycastHits.Clear();
            if (camera == null)
                return;

            // Match the global trigger-query behavior used by the allocating overlap APIs.
            _contactFilter.useTriggers = Physics2D.queriesHitTriggers;

            // Canvas events provide screen pixels. Intersect the camera ray with
            // the XY gameplay plane so both orthographic and perspective cameras work.
            var ray = camera.ScreenPointToRay(screenPosition);
            var worldPlane = new Plane(Vector3.forward, Vector3.zero);
            if (worldPlane.Raycast(ray, out float distance))
                Physics2D.OverlapPoint(ray.GetPoint(distance), _contactFilter, _raycastHits);
        }
    }
}
