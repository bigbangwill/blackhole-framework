using UnityEngine;

namespace Blackhole.CameraController.Contract
{
    public interface ICameraController
    {
        void ActivateCamera(Camera cam);
        void DeActivateCamera();
        void SetCanInteract(bool canInteract);
        void CameraMovementSetting(CameraMovementSetting setting);
    }
}
