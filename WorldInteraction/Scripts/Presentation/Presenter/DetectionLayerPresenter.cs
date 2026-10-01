using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;
using Blackhole.WorldInteraction.Application;

namespace Blackhole.WorldInteraction.Presentation
{
    /// <summary>
    /// Receives Unity EventSystem callbacks from the scene's detection surface and
    /// forwards them to the world-space detection service.
    /// </summary>
    public class DetectionLayerPresenter : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Inject] private readonly IDetectionRaycaster _detectionRaycaster;

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

        private static WorldInteractionContext CreateContext(PointerEventData eventData)
        {
            // InputSystemUIInputModule can reuse one pointer ID for multiple mouse
            // buttons, so the button must travel with the ID to keep presses separate.
            return new WorldInteractionContext(
                eventData.position,
                eventData.pointerId,
                (WorldInteractionPointerButton)eventData.button);
        }
    }
}
