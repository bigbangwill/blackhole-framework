using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using VContainer;
using Blackhole.WorldInteraction.Application;

namespace Blackhole.WorldInteraction.Presentation
{
    /// <summary>
    /// Receives Unity EventSystem callbacks from the full-screen input canvas and
    /// forwards them to the world-space detection service.
    /// </summary>
    public class DetectionLayerPresenter : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Inject] private readonly IDetectionRaycaster _detectionRaycaster;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        private void Start()
        {
            BindCamera();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BindCamera();
        }

        private void OnActiveSceneChanged(Scene previous, Scene current) => BindCamera();

        private void BindCamera()
        {
            if (_detectionRaycaster == null || !TryGetComponent<Canvas>(out var canvas))
                return;

            Camera camera = _detectionRaycaster.MainCamera;
            if (canvas.worldCamera != camera)
                canvas.worldCamera = camera;
        }

        /// <inheritdoc />
        public void OnPointerClick(PointerEventData eventData)
        {
            _detectionRaycaster.OnPointerClickedRaycast(CreateContext(eventData));
        }

        /// <inheritdoc />
        public void OnPointerDown(PointerEventData eventData)
        {
            _detectionRaycaster.OnPointerDownRaycast(CreateContext(eventData));
        }

        /// <inheritdoc />
        public void OnPointerUp(PointerEventData eventData)
        {
            _detectionRaycaster.OnPointerUpRaycast(CreateContext(eventData));
        }

        private WorldInteractionContext CreateContext(PointerEventData eventData)
        {
            BindCamera();
            // InputSystemUIInputModule can reuse one pointer ID for multiple mouse
            // buttons, so the button must travel with the ID to keep presses separate.
            return new WorldInteractionContext(
                eventData.position,
                eventData.pointerId,
                (WorldInteractionPointerButton)eventData.button);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            eventData.eligibleForClick = false;
            _detectionRaycaster.OnBeginDragRaycast(CreateContext(eventData));
        }

        public void OnDrag(PointerEventData eventData)
        {
            _detectionRaycaster.OnDragRaycast(CreateContext(eventData));
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _detectionRaycaster.OnEndDragRaycast(CreateContext(eventData));
        }
    }
}
