using UnityEngine;
using Blackhole.GlobalDialogue.Contract;

namespace Blackhole.GlobalDialogue.Infrastructure
{
    public class PositionConvetor : IPositionConvertor
    {
        public Vector2 WorldPositionToUI(Vector2 worldPosition)
        {
            Camera camera = Camera.main;

            float worldHeight = camera.orthographicSize * 2f;
            float worldWidth = worldHeight * camera.aspect;

            // Shift world center (0) to normalized UI center (0.5).
            float x = (worldPosition.x / worldWidth + 0.5f) * Screen.width;
            float y = (0.5f - worldPosition.y / worldHeight) * Screen.height;

            return new Vector2(x, y);
        }

        public Vector2 UIToWorld(Vector2 uiPosition)
        {
            Camera camera = Camera.main;

            float worldHeight = camera.orthographicSize * 2f;
            float worldWidth = worldHeight * camera.aspect;

            // Convert UI position from pixels to world position.
            float x = (uiPosition.x / Screen.width - 0.5f) * worldWidth;
            float y = (0.5f - uiPosition.y / Screen.height) * worldHeight;

            return new Vector2(x, y);
        }
    }
}