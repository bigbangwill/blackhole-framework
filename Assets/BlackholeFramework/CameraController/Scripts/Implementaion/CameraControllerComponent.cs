using Blackhole.CameraController.Contract;
using Blackhole.WorldInteraction.Contract;
using System.Collections.Generic;
using UnityEngine;
using VContainer;


namespace Blackhole.CameraController.Implementation
{
    public class CameraControllerComponent : MonoBehaviour, ICameraController
    {
        private Camera _camera;
        private bool _canInteractCamera;
        private CameraMovementSetting _movementSetting;
        [Tooltip("Assign the surface instance in the scene that contains an InteractableComponent.")]
        [SerializeField] private GameObject _surface;

        private IInteractionEventSource _input;

        private const float PinchThreshold = 8f;
        private readonly Dictionary<int, Vector2> _pointers = new();
        private Vector2 _lastDragScreenPosition;
        private Vector3 _targetPosition;
        private int _dragPointerId;
        private bool _isDragging;
        private bool _isPinching;
        private float _pinchStartDistance;
        private float _previousPinchDistance;

        [Inject]
        public void Configure(CameraControllerSettingsSO settings)
        {
            CameraMovementSetting(settings.CreateMovementSetting());
        }

        private void OnEnable()
        {
            if (_surface == null)
                return;

            _input = _surface.GetComponent<IInteractionEventSource>();
            if (_input == null)
            {
                Debug.LogError("The camera surface must contain an IInteractionEventSource.", this);
                return;
            }

            _input.AddPointerDownListener(OnPointerDown);
            _input.AddPointerUpListener(OnPointerUp);
            _input.AddBeginDragListener(OnBeginDrag);
            _input.AddDragListener(OnDrag);
            _input.AddEndDragListener(OnEndDrag);
        }

        private void OnDisable()
        {
            ResetGesture();
            if (_input == null)
                return;

            _input.RemovePointerDownListener(OnPointerDown);
            _input.RemovePointerUpListener(OnPointerUp);
            _input.RemoveBeginDragListener(OnBeginDrag);
            _input.RemoveDragListener(OnDrag);
            _input.RemoveEndDragListener(OnEndDrag);
            _input = null;
        }

        private void OnPointerDown(WorldInteractionEventData data)
        {
            if (!_canInteractCamera || _camera == null || _pointers.Count >= 2 || _pointers.ContainsKey(data.PointerId))
                return;

            _pointers.Add(data.PointerId, data.ScreenPosition);
            if (_pointers.Count == 2)
            {
                _isDragging = false;
                _isPinching = false;
                _targetPosition = _camera.transform.position;
                GetPinchPositions(out var first, out var second);
                _pinchStartDistance = Vector2.Distance(first, second);
                _previousPinchDistance = _pinchStartDistance;
            }
        }

        private void OnPointerUp(WorldInteractionEventData data)
        {
            if (!_pointers.Remove(data.PointerId))
                return;

            _isDragging = false;
            _isPinching = false;
            if (_pointers.Count == 1 && _canInteractCamera && _camera != null)
            {
                // The remaining finger may already be dragging, so it will not
                // necessarily receive another begin-drag event.
                foreach (var pointer in _pointers)
                {
                    _dragPointerId = pointer.Key;
                    _lastDragScreenPosition = pointer.Value;
                }
                _targetPosition = _camera.transform.position;
                _isDragging = true;
            }
        }

        private void OnBeginDrag(WorldInteractionEventData data)
        {
            if (!_canInteractCamera || _camera == null || !_pointers.ContainsKey(data.PointerId))
                return;

            _pointers[data.PointerId] = data.ScreenPosition;
            if (_pointers.Count != 1)
                return;

            _dragPointerId = data.PointerId;
            _isDragging = true;
            _lastDragScreenPosition = data.ScreenPosition;
            _targetPosition = _camera.transform.position;
        }

        private void OnDrag(WorldInteractionEventData data)
        {
            if (!_canInteractCamera || _camera == null || !_pointers.ContainsKey(data.PointerId))
                return;

            _pointers[data.PointerId] = data.ScreenPosition;
            if (_pointers.Count == 2)
            {
                ApplyPinch();
                return;
            }

            if (!_isDragging || data.PointerId != _dragPointerId)
                return;
            // Project both positions with the current camera so smoothing cannot
            // turn the camera's own movement into another pointer movement.
            Vector2 previous = _camera.ScreenToWorldPoint(_lastDragScreenPosition);
            Vector2 current = _camera.ScreenToWorldPoint(data.ScreenPosition);
            Vector2 movement = (previous - current) * Mathf.Max(0f, _movementSetting.Speed);
            _targetPosition = ClampToBoundary(_targetPosition + (Vector3)movement);
            _lastDragScreenPosition = data.ScreenPosition;
        }

        private void OnEndDrag(WorldInteractionEventData data)
        {
            // Pointer-up normally arrives first. Removing by ID makes either
            // event order safe and leaves another finger's drag intact.
            OnPointerUp(data);
        }

        private void ApplyPinch()
        {
            if (!_camera.orthographic)
                return;

            GetPinchPositions(out var first, out var second);
            float distance = Vector2.Distance(first, second);
            if (!_isPinching)
            {
                if (Mathf.Abs(distance - _pinchStartDistance) < PinchThreshold)
                    return;
                _isPinching = true;
            }

            if (distance < 1f || _previousPinchDistance < 1f)
            {
                _previousPinchDistance = distance;
                return;
            }

            Vector2 midpoint = (first + second) * 0.5f;
            Vector2 worldBefore = _camera.ScreenToWorldPoint(midpoint);
            float minSize = Mathf.Max(0.01f, _movementSetting.Min_ZoomIn);
            float maxSize = Mathf.Max(minSize, _movementSetting.Max_ZoomOut);
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize * _previousPinchDistance / distance, minSize, maxSize);

            // Fit the viewport before compensating for the zoom around the fingers.
            _camera.transform.position = ClampToBoundary(_camera.transform.position);
            Vector2 worldAfter = _camera.ScreenToWorldPoint(midpoint);
            _camera.transform.position = ClampToBoundary(_camera.transform.position + (Vector3)(worldBefore - worldAfter));
            _targetPosition = _camera.transform.position;
            _previousPinchDistance = distance;
            FitSurfaceToCamera();
        }

        private void GetPinchPositions(out Vector2 first, out Vector2 second)
        {
            var positions = _pointers.Values.GetEnumerator();
            positions.MoveNext();
            first = positions.Current;
            positions.MoveNext();
            second = positions.Current;
        }

        private void ResetGesture()
        {
            _pointers.Clear();
            _isDragging = false;
            _isPinching = false;
            if (_camera != null)
                _targetPosition = _camera.transform.position;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Camera.current == null || Camera.current.cameraType != CameraType.SceneView)
                return;

            Rect boundary = _movementSetting.Boundary;
            if (boundary.width <= 0f || boundary.height <= 0f)
                return;

            Color previousColor = Gizmos.color;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(new Vector3(boundary.center.x, boundary.center.y, 0f), new Vector3(boundary.width, boundary.height, 0f));
            Gizmos.color = previousColor;
        }
#endif

        private void LateUpdate()
        {
            if (!_canInteractCamera || _camera == null)
                return;

            _targetPosition = ClampToBoundary(_targetPosition);
            float blend = _movementSetting.DragSmoothness * Time.deltaTime;
            _camera.transform.position = ClampToBoundary(Vector3.Lerp(_camera.transform.position, _targetPosition, blend));
            FitSurfaceToCamera();
        }

        private Vector3 ClampToBoundary(Vector3 position)
        {
            Rect bounds = _movementSetting.Boundary;
            if (_camera == null || !_camera.orthographic || bounds.width <= 0f || bounds.height <= 0f)
                return position;

            // If the view is larger than the map, zoom in enough to fit it.
            float maximumSize = Mathf.Min(bounds.height / 2f, bounds.width / (2f * _camera.aspect));
            _camera.orthographicSize = Mathf.Min(_camera.orthographicSize, maximumSize);

            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;
            position.x = Mathf.Clamp(position.x, bounds.xMin + halfWidth, bounds.xMax - halfWidth);
            position.y = Mathf.Clamp(position.y, bounds.yMin + halfHeight, bounds.yMax - halfHeight);
            return position;
        }

        private void FitSurfaceToCamera()
        {
            if (_camera == null || !_camera.orthographic || _surface == null
                || !_surface.TryGetComponent<BoxCollider2D>(out var collider))
                return;

            Vector3 scale = _surface.transform.lossyScale;
            if (Mathf.Approximately(scale.x, 0f) || Mathf.Approximately(scale.y, 0f))
                return;

            float height = _camera.orthographicSize * 2f;
            Vector2 size = new Vector2(height * _camera.aspect / Mathf.Abs(scale.x), height / Mathf.Abs(scale.y));
            // WorldInteraction checks colliders on the XY plane at Z = 0.
            Vector3 cameraPosition = _camera.transform.position;
            Vector3 position = new Vector3(cameraPosition.x, cameraPosition.y, 0f);
            if (_surface.transform.position == position && collider.size == size && collider.offset == Vector2.zero)
                return;

            _surface.transform.position = position;
            collider.offset = Vector2.zero;
            collider.size = size;
            Physics2D.SyncTransforms();
        }

        public void ActivateCamera(Camera cam)
        {
            _camera = cam;
            ResetGesture();
            if (_camera != null)
            {
                _camera.transform.position = ClampToBoundary(_camera.transform.position);
                _targetPosition = _camera.transform.position;
                FitSurfaceToCamera();
            }
        }
        public void DeActivateCamera()
        {
            ResetGesture();
            _camera = null;
        }
        public void SetCanInteract(bool canInteract)
        {
            _canInteractCamera = canInteract;
            if (!canInteract)
                ResetGesture();
        }
        public void CameraMovementSetting(CameraMovementSetting setting)
        {
            _movementSetting = setting;
            if (_camera != null)
            {
                _camera.transform.position = ClampToBoundary(_camera.transform.position);
                _targetPosition = ClampToBoundary(_targetPosition);
                FitSurfaceToCamera();
            }
        }
    }
}
