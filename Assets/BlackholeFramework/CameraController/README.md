# CameraController

Add Blackhole.CameraController as a dependency of a module that controls a 2D camera. The installer provides ICameraController and injects movement defaults from Data/CameraControllerSettings.asset.

```csharp
controller.ActivateCamera(camera);
controller.SetCanInteract(true);
```

Call CameraMovementSetting to override the Rect boundary, minimum and maximum orthographic sizes, drag speed or smoothing. SetCanInteract(false) cancels gestures; DeActivateCamera releases the camera. The controller pans using WorldInteraction drag events and supports two-pointer orthographic pinch zoom centered between the fingers. Bounds constrain the whole viewport and can reduce the zoom-out limit to fit the map.

The drag-surface prefab contains a BoxCollider2D and InteractableComponent. Set its layer to one included in WorldInteractionSettings.InteractionLayerMask if your project changes the default layers. Keep the gameplay plane at Z=0 and provide an active EventSystem. Configure your own camera; the module does not create or activate a camera automatically.
