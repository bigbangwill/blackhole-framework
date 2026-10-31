using Blackhole.Settings.Contract;
using UnityEngine;

namespace Blackhole.CameraController.Implementation
{
    [CreateAssetMenu(fileName = "CameraControllerSettings", menuName = "Blackhole/Modules/CameraController/Settings")]
    [ProjectSettings("CameraControllerSetting", 250)]
    public sealed class CameraControllerSettingsSO : ScriptableObject
    {
        [SerializeField] private Rect boundary = new Rect(-20f, -15f, 40f, 30f);
        [SerializeField, Min(0.01f)] private float minZoomIn = 2f;
        [SerializeField, Min(0.01f)] private float maxZoomOut = 12f;
        [SerializeField, Min(0.2f)] private float speed = 1f;
        [SerializeField, Min(0.01f)] private float dragSmoothness = 10f;

        public Contract.CameraMovementSetting CreateMovementSetting()
        {
            return new Contract.CameraMovementSetting(boundary, minZoomIn, maxZoomOut, speed, dragSmoothness);
        }
    }
}
