using UnityEngine;
using Blackhole.WorldInteraction.Contract;
using Blackhole.AudioPlayer.Contract.Interface;
using VContainer;

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
        [Inject] private IAudioService _audioService;

        private void Awake()
        {
            _interactionEventSource = GetComponent<IInteractionEventSource>();
        }

        private void OnEnable()
        {
            _interactionEventSource.AddPointerClickedListener(OnClicked);
            _interactionEventSource.AddPointerDownListener(OnDown);
            _interactionEventSource.AddPointerUpListener(OnUp);
        }

        private void OnDisable()
        {
            _interactionEventSource.RemovePointerClickedListener(OnClicked);
            _interactionEventSource.RemovePointerDownListener(OnDown);
            _interactionEventSource.RemovePointerUpListener(OnUp);
        }

        private void OnDown(WorldInteractionEventData eventData)
        {
            Debug.Log($"Down at {eventData.ScreenPosition}");
            
        }

        private void OnUp(WorldInteractionEventData eventData)
        {
            Debug.Log($"Up at {eventData.ScreenPosition}");
        }

        private void OnClicked(WorldInteractionEventData eventData)
        {
            Debug.Log($"Clicked at {eventData.ScreenPosition}");
            _audioService.Play("Bubbles");
        }
    }
}
