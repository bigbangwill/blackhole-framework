using System;
using System.Collections;
using System.Reflection;
using Blackhole.CameraController.Contract;
using Blackhole.CameraController.Implementation;
using Blackhole.WorldInteraction.Application;
using Blackhole.WorldInteraction.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace Blackhole.Development.Tests
{
    public sealed class CameraControllerZoomTests
    {
        [TestCase(20f, 110f)]
        [TestCase(1f, 90f)]
        public void PinchFromOutsideZoomRangePreservesInitialFraming(float initialSize, float distance)
        {
            using var rig = new CameraRig(initialSize);
            Vector2 start = rig.Camera.pixelRect.center;
            rig.Input.OnPointerDown(new WorldInteractionContext(start, 1));
            rig.Input.OnPointerDown(new WorldInteractionContext(start + Vector2.right * 100, 2));
            rig.Input.OnDrag(new WorldInteractionContext(start + Vector2.right * distance, 2));
            Assert.That(rig.Camera.orthographicSize, Is.EqualTo(initialSize * 100 / distance).Within(0.001f));
            rig.Input.OnDrag(new WorldInteractionContext(start + Vector2.right * 100, 2));
            float expectedReturn = initialSize > 10 ? initialSize : initialSize * 100 / distance;
            Assert.That(rig.Camera.orthographicSize, Is.EqualTo(expectedReturn).Within(0.001f));
        }

        [Test]
        public void WheelZoomAnchorsCursorHonorsGatesAndOwnsItsInputActions()
        {
            var actions = ScriptableObject.CreateInstance<InputActionAsset>();
            actions.AddActionMap("Camera").AddAction("Zoom", InputActionType.Value, "<Mouse>/scroll/y", expectedControlLayout: "Axis");
            var previousBackgroundBehavior = InputSystem.settings.backgroundBehavior;
            var previousEditorInputBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            try
            {
                using var rig = new CameraRig(5, actions);
                Vector2 cursor = rig.Camera.pixelRect.center + new Vector2(rig.Camera.pixelWidth * 0.1f, 0);
                Vector3 anchor = rig.Camera.ScreenToWorldPoint(cursor);
                Scroll(rig, mouse, cursor, 1);
                Assert.That(rig.Camera.orthographicSize, Is.EqualTo(5 / 1.15f).Within(0.001f));
                Assert.That(Vector3.Distance(rig.Camera.ScreenToWorldPoint(cursor), anchor), Is.LessThan(0.001f));
                Scroll(rig, mouse, cursor, -100);
                Assert.That(rig.Camera.orthographicSize, Is.EqualTo(10).Within(0.001f));
                Scroll(rig, mouse, cursor, 100);
                Assert.That(rig.Camera.orthographicSize, Is.EqualTo(2).Within(0.001f));
                rig.Controller.SetCanInteract(false);
                Scroll(rig, mouse, cursor, -1);
                Assert.That(rig.Camera.orthographicSize, Is.EqualTo(2).Within(0.001f));
                rig.Controller.SetCanInteract(true);
                Scroll(rig, mouse, rig.Camera.pixelRect.max + Vector2.one * 10, -1);
                Assert.That(rig.Camera.orthographicSize, Is.EqualTo(2).Within(0.001f));
                rig.Camera.orthographic = false;
                Scroll(rig, mouse, cursor, -1);
                Assert.That(rig.Camera.orthographicSize, Is.EqualTo(2).Within(0.001f));
                rig.Camera.orthographic = true;
                var instance = (InputActionAsset)Field("_inputActionsInstance").GetValue(rig.Controller);
                Assert.That(instance, Is.Not.SameAs(actions));
                Assert.That(actions.enabled, Is.False);
                rig.Controller.enabled = false;
                Assert.That(instance.enabled, Is.False);
                rig.Controller.enabled = true;
                Scroll(rig, mouse, cursor, -1);
                Assert.That(rig.Camera.orthographicSize, Is.EqualTo(2 * 1.15f).Within(0.001f));
                rig.Controller.DeActivateCamera();
                Scroll(rig, mouse, cursor, -1);
                Assert.That(rig.Camera.orthographicSize, Is.EqualTo(2 * 1.15f).Within(0.001f));
            }
            finally
            {
                InputSystem.RemoveDevice(mouse);
                InputSystem.settings.backgroundBehavior = previousBackgroundBehavior;
                InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorInputBehavior;
                UnityEngine.Object.DestroyImmediate(actions);
            }
        }

        [UnityTest]
        public IEnumerator DisabledInteractionStillFitsSurfaceToExternalCameraChanges()
        {
            using var rig = new CameraRig(5);
            rig.Controller.SetCanInteract(false);
            rig.Camera.transform.position = new Vector3(3, 4, -10);
            rig.Camera.orthographicSize = 8;
            rig.Camera.aspect = 1.5f;
            yield return null;
            yield return null;
            Assert.That(rig.Surface.transform.position, Is.EqualTo(new Vector3(3, 4, 0)));
            Assert.That(rig.Surface.GetComponent<BoxCollider2D>().size, Is.EqualTo(new Vector2(24, 16)));
            Assert.That(rig.Camera.transform.position, Is.EqualTo(new Vector3(3, 4, -10)));
            Assert.That(rig.Camera.orthographicSize, Is.EqualTo(8));
        }

        private static FieldInfo Field(string name) => typeof(CameraControllerComponent).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);

        private static void Scroll(CameraRig rig, Mouse mouse, Vector2 position, float steps)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (InputSystem.settings.scrollDeltaBehavior == InputSettings.ScrollDeltaBehavior.KeepPlatformSpecificInputRange)
                steps *= 120;
#endif
            InputSystem.QueueStateEvent(mouse, new MouseState { position = position, scroll = new Vector2(0, steps) });
            InputSystem.Update();
            Assert.That(Mouse.current, Is.SameAs(mouse), "Wheel input must come from the simulated mouse.");
            Assert.That(mouse.scroll.y.ReadValue(), Is.EqualTo(steps), "The simulated scroll event was not processed.");
            var action = (InputAction)Field("_scrollZoomAction").GetValue(rig.Controller);
            Assert.That(action.ReadValue<float>(), Is.EqualTo(steps), $"Wheel action: enabled={action.enabled}, phase={action.phase}, controls={action.controls.Count}, update={InputState.currentUpdateType}, viewport={rig.Camera.pixelRect}, cursor={position}");
            typeof(CameraControllerComponent).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(rig.Controller, null);
        }

        private sealed class CameraRig : IDisposable
        {
            public readonly Camera Camera;
            public readonly GameObject Surface;
            public readonly InteractableComponent Input;
            public readonly CameraControllerComponent Controller;

            public CameraRig(float size, InputActionAsset actions = null)
            {
                Camera = new GameObject("Zoom Test Camera").AddComponent<Camera>();
                Camera.orthographic = true;
                Camera.orthographicSize = size;
                Camera.transform.position = new Vector3(0, 0, -10);
                Surface = new GameObject("Zoom Test Surface");
                Surface.SetActive(false);
                Surface.AddComponent<BoxCollider2D>();
                Input = Surface.AddComponent<InteractableComponent>();
                Controller = Surface.AddComponent<CameraControllerComponent>();
                Field("_surface").SetValue(Controller, Surface);
                Field("_cameraInputActions").SetValue(Controller, actions);
                Surface.SetActive(true);
                Controller.CameraMovementSetting(new CameraMovementSetting(default, 2, 10));
                Controller.ActivateCamera(Camera);
                Controller.SetCanInteract(true);
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(Surface);
                UnityEngine.Object.DestroyImmediate(Camera.gameObject);
            }
        }
    }
}
