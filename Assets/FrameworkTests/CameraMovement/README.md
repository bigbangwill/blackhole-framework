# Camera movement test

Open `Assets/BlackholeFramework/Framework/BlackholeFrameworkScene.unity` and press Play. The framework loads the persistent `Blackhole.Development.CameraMovement` test module, including its CameraController and WorldInteraction dependencies, then activates `CameraMovementTestScene`.

Drag the grid with the mouse or one finger. Use two fingers on a touch device to pinch zoom. The Zoom + and Zoom - buttons provide desktop zoom checks. Pan toward the gold map boundary to verify that the entire camera viewport stays within the map. Pause input to check that gestures stop moving the camera, and Reset to return to the origin.

The HUD shows the camera position, orthographic size and input state. The map contains a cyan origin, grid lines, a gold boundary and colored landmarks. It uses the real injected camera service and the WorldInteraction input canvas.

Run **Blackhole > Development > Configure Camera Movement Test** to select this module as the development starting module and open the framework scene again. **Blackhole > Development > Configure Smoke Project** switches back to the original smoke test. Existing scene content is preserved when reconfiguring the camera test.

The scene, module, UI, materials and setup tooling are development-only. Framework release archives exclude this folder and clear the development starting-module reference.
