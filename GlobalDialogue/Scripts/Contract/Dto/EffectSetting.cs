using UnityEngine;

namespace Blackhole.GlobalDialogue.Contract
{
    public enum ImageAnchor
    {
        TopLeftToCenter,
        BottomLeftToCenter,
        CenterToCenter,
        BottomRightToCenter,
        TopRightToCenter,
        MidBottomToCenter
    }
    public struct EffectSetting
    {
        public readonly EffectType Type;
        public readonly ImageAnchor AnchorPosition;
        public readonly Vector2Int Offset;
        public readonly int ShowTime;

        /// <summary>
        /// Creates a new effect setting.
        /// </summary>
        /// <param name="type">The type of effect to display.</param>
        /// <param name="AnchorPosition">The anchor position used to place the effect.</param>
        /// <param name="offset">The positional offset applied to the effect.</param>
        /// <param name="ShowTime">The amount of time the effect is shown, in milliseconds (ms).</param>
        public EffectSetting(EffectType type, ImageAnchor AnchorPosition, Vector2Int offset, int ShowTime)
        {
            this.Type = type;
            this.Offset = offset;
            this.ShowTime = ShowTime;
            this.AnchorPosition = AnchorPosition;
        }
    }
}