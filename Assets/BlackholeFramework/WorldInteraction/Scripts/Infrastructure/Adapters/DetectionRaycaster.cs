using System;
using System.Collections.Generic;
using Blackhole.WorldInteraction.Application;
using Blackhole.WorldInteraction.Shared;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using VContainer;

namespace Blackhole.WorldInteraction.Infrastructure
{
    /// <summary>Routes 2D pointer hits to world targets and captures the original recipients for dragging.</summary>
    public sealed class DetectionRaycaster : IDetectionRaycaster
    {
        // The Input System can reuse a pointer ID for multiple mouse buttons.
        private readonly Dictionary<(int, WorldInteractionPointerButton), List<IInteractionTarget>> _pressed = new();
        private readonly Dictionary<(int, WorldInteractionPointerButton), List<IInteractionTarget>> _dragged = new();
        private readonly List<Collider2D> _colliders = new();
        private readonly List<Hit> _hits = new();
        private readonly HashSet<IInteractionTarget> _recipients = new();
        private ContactFilter2D _contactFilter;
        private IInteractionTarget _selected;
        private Camera _activeSceneCamera;
        private Camera[] _cameras = Array.Empty<Camera>();

        public Camera MainCamera
        {
            get
            {
                Camera main = Camera.main;
                Scene scene = SceneManager.GetActiveScene();
                if (main == null || main.gameObject.scene == scene)
                    return main;
                if (IsMainCameraInScene(_activeSceneCamera, scene))
                    return _activeSceneCamera;

                // A persistent startup scene can still have an enabled MainCamera.
                int count = Camera.allCamerasCount;
                if (_cameras.Length < count)
                    _cameras = new Camera[count];
                count = Camera.GetAllCameras(_cameras);
                for (int i = 0; i < count; i++)
                {
                    Camera candidate = _cameras[i];
                    if (IsMainCameraInScene(candidate, scene))
                        return _activeSceneCamera = candidate;
                }
                return main;
            }
        }

        private static bool IsMainCameraInScene(Camera camera, Scene scene) =>
            camera != null && camera.isActiveAndEnabled && camera.gameObject.scene == scene && camera.CompareTag("MainCamera");

        public DetectionRaycaster([Key(WorldInteractionLayerMasksEnum.InteractionLayer)] LayerMask interactableLayerMask)
        {
            _contactFilter.SetLayerMask(interactableLayerMask);
        }

        public void OnPointerDownRaycast(WorldInteractionContext context)
        {
            var key = GetInputKey(context);
            _dragged.Remove(key);
            List<IInteractionTarget> targets = Raycast(context.ScreenPosition, out _);
            _pressed[key] = targets;
            foreach (IInteractionTarget target in targets)
            {
                if (IsAlive(target))
                    target.OnPointerDown(context);
            }
        }

        public void OnBeginDragRaycast(WorldInteractionContext context)
        {
            var key = GetInputKey(context);
            if (!_pressed.TryGetValue(key, out var targets))
                return;

            _dragged[key] = targets;
            foreach (IInteractionTarget target in targets)
            {
                if (IsAlive(target))
                    target.OnBeginDrag(context);
            }
        }

        public void OnDragRaycast(WorldInteractionContext context)
        {
            if (!_dragged.TryGetValue(GetInputKey(context), out var targets))
                return;

            foreach (IInteractionTarget target in targets)
            {
                if (IsAlive(target))
                    target.OnDrag(context);
            }
        }

        public void OnPointerUpRaycast(WorldInteractionContext context)
        {
            if (!_pressed.Remove(GetInputKey(context), out var targets))
                return;

            List<IInteractionTarget> hits = Raycast(context.ScreenPosition, out _);
            foreach (IInteractionTarget target in targets)
            {
                if (IsAlive(target))
                    target.OnPointerUp(context, hits.Contains(target));
            }
        }

        public void OnEndDragRaycast(WorldInteractionContext context)
        {
            if (!_dragged.Remove(GetInputKey(context), out var targets))
                return;

            List<IInteractionTarget> hits = Raycast(context.ScreenPosition, out _);
            foreach (IInteractionTarget target in targets)
            {
                if (IsAlive(target))
                    target.OnEndDrag(context, hits.Contains(target));
            }
        }

        public void OnPointerClickedRaycast(WorldInteractionContext context)
        {
            List<IInteractionTarget> targets = Raycast(context.ScreenPosition, out var selected);
            IInteractionTarget previous = _selected;
            _selected = selected;
            if (!ReferenceEquals(previous, selected) && IsAlive(previous))
                previous.Deselect(context);

            foreach (IInteractionTarget target in targets)
            {
                if (IsAlive(target))
                    target.OnPointerClicked(context);
            }
        }

        private List<IInteractionTarget> Raycast(Vector2 screenPosition, out IInteractionTarget selected)
        {
            selected = null;
            var targets = new List<IInteractionTarget>();
            Camera camera = MainCamera;
            if (camera == null)
                return targets;

            // Intersect the XY gameplay plane for both perspective and orthographic cameras.
            Ray ray = camera.ScreenPointToRay(screenPosition);
            if (!new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out float distance))
                return targets;

            _colliders.Clear();
            _hits.Clear();
            _recipients.Clear();
            _contactFilter.useTriggers = Physics2D.queriesHitTriggers;
            Physics2D.OverlapPoint(ray.GetPoint(distance), _contactFilter, _colliders);

            foreach (Collider2D collider in _colliders)
            {
                if (collider != null && collider.TryGetComponent<IInteractionTarget>(out var target) && IsAlive(target))
                    _hits.Add(new Hit(collider, target, camera));
            }

            _hits.Sort();
            foreach (Hit hit in _hits)
            {
                IInteractionTarget target = hit.Target;
                if ((selected != null && !target.ReceiveAlongsideTarget) || !_recipients.Add(target))
                    continue;

                targets.Add(target);
                if (!target.PassThrough && !target.ReceiveAlongsideTarget)
                    selected = target;
            }

            return targets;
        }

        private static bool IsAlive(IInteractionTarget target) =>
            target != null && (target is not UnityEngine.Object component || component != null);

        private static (int, WorldInteractionPointerButton) GetInputKey(WorldInteractionContext context) =>
            (context.PointerId, context.PointerButton);

        private readonly struct Hit : IComparable<Hit>
        {
            public IInteractionTarget Target { get; }
            private readonly int _layer;
            private readonly int _order;
            private readonly float _depth;
            private readonly ulong _entityId;

            public Hit(Collider2D collider, IInteractionTarget target, Camera camera)
            {
                Target = target;
                _entityId = EntityId.ToULong(collider.GetEntityId());
                var group = collider.GetComponentInParent<SortingGroup>();
                int layerId = 0;
                int order = 0;
                Vector3 position = collider.bounds.center;

                if (group != null)
                {
                    layerId = group.sortingLayerID;
                    order = group.sortingOrder;
                    position = group.transform.position;
                }
                else if (collider.TryGetComponent<Renderer>(out var renderer))
                {
                    layerId = renderer.sortingLayerID;
                    order = renderer.sortingOrder;
                    position = renderer.bounds.center;
                }

                _layer = SortingLayer.GetLayerValueFromID(layerId);
                _order = order;
                Vector3 offset = position - camera.transform.position;
                _depth = camera.orthographic ? Vector3.Dot(offset, camera.transform.forward) : offset.sqrMagnitude;
            }

            public int CompareTo(Hit other)
            {
                int layer = other._layer.CompareTo(_layer);
                if (layer != 0)
                    return layer;
                int order = other._order.CompareTo(_order);
                if (order != 0)
                    return order;
                int depth = _depth.CompareTo(other._depth);
                return depth != 0 ? depth : _entityId.CompareTo(other._entityId);
            }
        }
    }
}
