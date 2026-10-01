using Blackhole.Settings.Contract;
using UnityEngine;

namespace Blackhole.WorldInteraction.Infrastructure
{
    /// <summary>
    /// Stores the physics layers used by the WorldInteraction module.
    /// </summary>
    [ProjectSettings("World Interaction", 0)]
    [CreateAssetMenu(fileName = "WorldInteractionSettingSO", menuName = "Blackhole/Modules/World Interaction/Settings")]
    public class WorldInteractionSettingSO : ScriptableObject
    {
        /// <summary>
        /// Colliders on these layers can receive world-interaction callbacks.
        /// </summary>
        public LayerMask InteractionLayerMask;

        /// <summary>
        /// Layers used by the scene-level surface that captures EventSystem pointer events.
        /// </summary>
        public LayerMask DetectionLayerMask;
    }

    /// <summary>
    /// Keys used to distinguish layer-mask registrations in VContainer.
    /// </summary>
    public enum WorldInteractionLayerMasksEnum
    {
        /// <summary>The layer containing the EventSystem detection surface.</summary>
        DetectionLayer,

        /// <summary>The layers containing interactable world colliders.</summary>
        InteractionLayer
    }
}
