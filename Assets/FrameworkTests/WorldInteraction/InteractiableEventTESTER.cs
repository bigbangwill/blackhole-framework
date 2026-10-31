using UnityEngine;
using Blackhole.WorldInteraction.Contract;

namespace Blackhole.WorldInteraction.Presentation
{
    /// <summary>
    /// Development helper that logs every event emitted by an interaction source.
    /// Attach it beside an <see cref="IInteractionEventSource"/> implementation when
    /// validating a scene's interaction setup.
    /// </summary>
    [RequireComponent(typeof(IInteractionEventSource))]
    public class InteractiableEventTESTER : MonoBehaviour
    {
        private IInteractionEventSource _interactionEventSource;

        private void Awake()
        {
            _interactionEventSource = GetComponent<IInteractionEventSource>();
        }

        private void OnEnable()
        {
            _interactionEventSource.AddPointerClickedListener(OnClicked);
            _interactionEventSource.AddPointerDownListener(OnDown);
            _interactionEventSource.AddPointerUpListener(OnUp);
            _interactionEventSource.AddBeginDragListener(OnBeginDrag);
            _interactionEventSource.AddDragListener(OnDrag);
            _interactionEventSource.AddEndDragListener(OnEndDrag);
        }

        private void OnDisable()
        {
            _interactionEventSource.RemovePointerClickedListener(OnClicked);
            _interactionEventSource.RemovePointerDownListener(OnDown);
            _interactionEventSource.RemovePointerUpListener(OnUp);
            _interactionEventSource.RemoveBeginDragListener(OnBeginDrag);
            _interactionEventSource.RemoveDragListener(OnDrag);
            _interactionEventSource.RemoveEndDragListener(OnEndDrag);
        }

        private void OnDown(WorldInteractionEventData eventData)
        {
            Debug.Log($"Down at {eventData.ScreenPosition}");
            
        }

        private void OnUp(WorldInteractionEventData eventData)
        {
            Debug.Log($"Up at {eventData.ScreenPosition}, isHit: {eventData.IsPointerOverUI}");
        }

        private void OnClicked(WorldInteractionEventData eventData)
        {
            Debug.Log($"Clicked at {eventData.ScreenPosition}");
        }

        private void OnBeginDrag(WorldInteractionEventData eventData)
        {
            Debug.Log($"Begin drag at {eventData.ScreenPosition}");
        }

        private void OnDrag(WorldInteractionEventData eventData)
        {
            Debug.Log($"Drag at {eventData.ScreenPosition}");
        }

        private void OnEndDrag(WorldInteractionEventData eventData)
        {
            Debug.Log($"End drag at {eventData.ScreenPosition}, isHit: {eventData.IsPointerOverUI}");
        }
    }
}
