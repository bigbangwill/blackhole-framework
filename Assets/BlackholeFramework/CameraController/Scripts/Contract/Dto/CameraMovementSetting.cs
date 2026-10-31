using UnityEngine;

namespace Blackhole.CameraController.Contract
{
    public struct CameraMovementSetting
    {
        public readonly Rect Boundary;
        public readonly float Min_ZoomIn;
        public readonly float Max_ZoomOut;
        public readonly float Speed;
        public readonly float DragSmoothness;

        public CameraMovementSetting(Rect Boundary, float Min_ZoomIn, float Max_ZoomOut, float speed=1, float dragSmoothness=10)
        {
            this.Boundary = Boundary;
            this.Min_ZoomIn = Min_ZoomIn;
            this.Max_ZoomOut = Max_ZoomOut;
            Speed = speed;
            DragSmoothness = dragSmoothness;
        }
    }
}
