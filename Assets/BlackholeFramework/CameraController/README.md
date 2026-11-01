# CameraController

Add Blackhole.CameraController as a dependency of a module that controls a 2D camera. The installer provides ICameraController and injects movement defaults from Data/CameraControllerSettings.asset.

```csharp
controller.ActivateCamera(camera);
controller.SetCanInteract(true);
```

Call CameraMovementSetting to override the Rect boundary, minimum and maximum orthographic sizes, drag speed or smoothing. SetCanInteract(false) cancels gestures; DeActivateCamera releases the camera. The controller pans using WorldInteraction drag events and supports two-pointer orthographic pinch zoom centered between the fingers. Mouse-wheel zoom is centered on the cursor and only applies inside the camera viewport. Bounds constrain the whole viewport and can reduce the zoom-out limit to fit the map.

Cameras initially outside the zoom range move gradually toward it without snapping on the first zoom input. Above the maximum, zooming back out can restore the initial framing until the configured range is reached. Below the minimum, zoom can only move toward the configured range. Surface position and size follow external camera changes even while interaction is disabled.

The supplied prefab references CameraControllerSettings for edit-time boundary gizmos and CameraController.inputactions for wheel input. Custom prefabs can assign an InputActionAsset with a Camera/Zoom axis action bound to <Mouse>/scroll/y; without that optional asset, drag and pinch remain available. Each controller enables its own cloned action asset and disables it when the component is disabled. Configure remains the injection API for runtime movement settings.

The drag-surface prefab contains a BoxCollider2D and InteractableComponent. Set its layer to one included in WorldInteractionSettings.InteractionLayerMask if your project changes the default layers. Keep the gameplay plane at Z=0. The persistent framework startup scene owns the shared EventSystem and InputSystemUIInputModule; WorldInteraction uses that system. Standalone test roots must provide their own EventSystem. Configure your own camera; the module does not create or activate a camera automatically.
