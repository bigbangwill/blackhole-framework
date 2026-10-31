using Blackhole.CameraController.Contract;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blackhole.Development.CameraMovement
{
    public sealed class CameraMovementTestView : MonoBehaviour
    {
        [SerializeField] private Camera testCamera;
        [SerializeField] private UIDocument document;

        private ICameraController controller;
        private CameraMovementSetting settings;
        private Label status;
        private Button pause;

        public Camera TestCamera => testCamera;
        public bool Ready { get; private set; }
        public bool InputEnabled { get; private set; }

        public void Initialize(ICameraController cameraController, CameraMovementSetting movementSettings)
        {
            controller = cameraController;
            settings = movementSettings;
            VisualElement root = document.rootVisualElement;
            root.pickingMode = PickingMode.Ignore;
            status = root.Q<Label>("status");
            pause = root.Q<Button>("pause");
            root.Q<Button>("reset").clicked += ResetCamera;
            root.Q<Button>("zoomIn").clicked += ZoomIn;
            root.Q<Button>("zoomOut").clicked += ZoomOut;
            pause.clicked += ToggleInput;
            Ready = true;
            ResetCamera();
        }

        public void ResetCamera()
        {
            controller.SetCanInteract(false);
            testCamera.transform.position = new Vector3(settings.Boundary.center.x, settings.Boundary.center.y, -10);
            testCamera.orthographicSize = Mathf.Clamp(6, settings.Min_ZoomIn, settings.Max_ZoomOut);
            controller.CameraMovementSetting(settings);
            controller.ActivateCamera(testCamera);
            InputEnabled = true;
            controller.SetCanInteract(true);
            pause.text = "Pause input";
        }

        public void ZoomIn() => SetZoom(testCamera.orthographicSize / 1.3f);
        public void ZoomOut() => SetZoom(testCamera.orthographicSize * 1.3f);

        public void ToggleInput()
        {
            InputEnabled = !InputEnabled;
            controller.SetCanInteract(InputEnabled);
            pause.text = InputEnabled ? "Pause input" : "Resume input";
        }

        private void SetZoom(float size)
        {
            testCamera.orthographicSize = Mathf.Clamp(size, settings.Min_ZoomIn, settings.Max_ZoomOut);
            controller.ActivateCamera(testCamera);
        }

        private void Update()
        {
            if (!Ready) return;
            Vector3 position = testCamera.transform.position;
            status.text = $"X {position.x:0.0}    Y {position.y:0.0}    Zoom {testCamera.orthographicSize:0.0}    {(InputEnabled ? "INPUT ON" : "INPUT PAUSED")}";
        }

        private void OnDestroy()
        {
            if (controller == null) return;
            controller.SetCanInteract(false);
            controller.DeActivateCamera();
        }
    }
}
