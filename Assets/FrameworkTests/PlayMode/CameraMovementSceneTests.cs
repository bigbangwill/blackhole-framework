using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blackhole.CameraController.Implementation;
using Blackhole.Development.CameraMovement;
using Blackhole.Framework.Composition;
using Blackhole.WorldInteraction.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Blackhole.Development.Tests
{
    public sealed class CameraMovementSceneTests
    {
        [UnityTest]
        public IEnumerator FrameworkLoadsCameraTestAndWiresMovementControls()
        {
            yield return SceneManager.LoadSceneAsync(FrameworkLifetimeScope.FrameworkScenePath);
            float deadline = Time.realtimeSinceStartup + 20;
            CameraMovementTestView view = null;
            while (Time.realtimeSinceStartup < deadline)
            {
                view = UnityEngine.Object.FindFirstObjectByType<CameraMovementTestView>();
                if (view != null && view.Ready) break;
                yield return null;
            }
            Assert.That(view, Is.Not.Null, "The framework did not load the camera test module.");
            Assert.That(view.Ready, Is.True);
            Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo("Assets/FrameworkTests/CameraMovement/CameraMovementTestScene.unity"));
            Assert.That(SceneManager.GetSceneByPath("Assets/BlackholeFramework/WorldInteraction/WorldInteractionScene.unity").isLoaded, Is.True);
            Camera camera = view.TestCamera;
            var presenter = UnityEngine.Object.FindFirstObjectByType<DetectionLayerPresenter>();
            Assert.That(presenter.GetComponent<Canvas>().worldCamera, Is.EqualTo(camera));
            Assert.That(UnityEngine.Object.FindObjectsByType<CameraControllerComponent>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            yield return null;
            Assert.That(view.GetComponent<UIDocument>().rootVisualElement.Q<Button>("reset").resolvedStyle.height, Is.GreaterThan(0));

            Vector3 initialPosition = camera.transform.position;
            var pointer = new PointerEventData(EventSystem.current) { pointerId = 4, position = new Vector2(Screen.width * 0.7f, Screen.height * 0.4f) };
            Canvas.ForceUpdateCanvases();
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.That(hits.Count, Is.GreaterThan(0));
            Assert.That(hits[0].gameObject, Is.EqualTo(presenter.gameObject), "The HUD must let map input pass through outside its controls.");
            presenter.OnPointerDown(pointer);
            presenter.OnBeginDrag(pointer);
            pointer.position += new Vector2(140, 0);
            presenter.OnDrag(pointer);
            presenter.OnPointerUp(pointer);
            presenter.OnEndDrag(pointer);
            for (int i = 0; i < 12; i++) yield return null;
            Assert.That(camera.transform.position.x, Is.LessThan(initialPosition.x), "World input did not reach the injected camera controller.");

            view.ToggleInput();
            Vector3 pausedPosition = camera.transform.position;
            presenter.OnPointerDown(pointer);
            presenter.OnBeginDrag(pointer);
            pointer.position -= new Vector2(240, 0);
            presenter.OnDrag(pointer);
            presenter.OnPointerUp(pointer);
            presenter.OnEndDrag(pointer);
            for (int i = 0; i < 3; i++) yield return null;
            Assert.That(camera.transform.position, Is.EqualTo(pausedPosition));
            view.ResetCamera();
            Assert.That(view.InputEnabled, Is.True);
            Assert.That(camera.transform.position, Is.EqualTo(initialPosition));
            float initialSize = camera.orthographicSize;
            view.ZoomIn();
            Assert.That(camera.orthographicSize, Is.LessThan(initialSize));
            view.ZoomOut();
            Assert.That(camera.orthographicSize, Is.EqualTo(initialSize).Within(0.01f));
            CaptureMapWhenRequested(camera);
        }

        private static void CaptureMapWhenRequested(Camera camera)
        {
            string path = Environment.GetEnvironmentVariable("BLACKHOLE_CAMERA_CAPTURE");
            if (string.IsNullOrEmpty(path) || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            RenderTexture target = new(1280, 720, 24);
            RenderTexture previous = RenderTexture.active;
            var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                target.Release();
                UnityEngine.Object.Destroy(target);
                UnityEngine.Object.Destroy(image);
            }
        }
    }
}
