using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Blackhole.CameraController.Contract;
using Blackhole.CameraController.Implementation;
using Blackhole.WorldInteraction.Application;
using Blackhole.WorldInteraction.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Blackhole.WorldInteraction.Composition;

namespace Blackhole.Development.Tests
{
    public sealed class CameraControllerTests
    {
        [UnityTest]
        public IEnumerator WorldInputSceneBindsCameraAndDispatchesPointerAwareDrag()
        {
            GameObject[] previousCameras = GameObject.FindGameObjectsWithTag("MainCamera");
            foreach (GameObject previous in previousCameras) previous.tag = "Untagged";
            var cameraObject = new GameObject("World Input Test Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.transform.position = new Vector3(0, 0, -10);
            const string path = "Assets/BlackholeFramework/WorldInteraction/WorldInteractionScene.unity";
            yield return SceneManager.LoadSceneAsync(path, LoadSceneMode.Additive);
            yield return null;
            Scene scene = SceneManager.GetSceneByPath(path);
            GameObject target = new("World Input Target", typeof(BoxCollider2D), typeof(InteractableComponent));
            try
            {
                var scope = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<WorldInteractionLifetimeScope>()).Single();
                target.layer = Enumerable.Range(0, 32).First(index => (scope.WorldInteractionSettingSO.InteractionLayerMask.value & (1 << index)) != 0);
                var presenter = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<DetectionLayerPresenter>()).Single();
                Assert.That(presenter.GetComponent<Canvas>().worldCamera, Is.EqualTo(camera));
                Assert.That(EventSystem.current, Is.Not.Null);
                int drags = 0;
                target.GetComponent<InteractableComponent>().AddDragListener(data => { Assert.That(data.PointerId, Is.EqualTo(9)); drags++; });
                Physics2D.SyncTransforms();
                var pointer = new PointerEventData(EventSystem.current) { position = camera.WorldToScreenPoint(Vector3.zero), pointerId = 9 };
                Canvas.ForceUpdateCanvases();
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, hits);
                Assert.That(hits.Any(hit => hit.gameObject == presenter.gameObject), Is.True, "The transparent input canvas must remain raycastable.");
                presenter.OnPointerDown(pointer);
                presenter.OnBeginDrag(pointer);
                pointer.position += new Vector2(100, 0);
                presenter.OnDrag(pointer);
                presenter.OnPointerUp(pointer);
                presenter.OnEndDrag(pointer);
                Assert.That(drags, Is.EqualTo(1));
            }
            finally
            {
                Object.Destroy(target);
                Object.Destroy(cameraObject);
                foreach (GameObject previous in previousCameras) if (previous != null) previous.tag = "MainCamera";
            }
            yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTest]
        public IEnumerator CameraPansPinchesStaysInBoundsAndStopsWhenDisabled()
        {
            var cameraObject = new GameObject("Camera Movement Test");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.transform.position = new Vector3(0, 0, -10);
            var surface = new GameObject("Camera Input");
            surface.SetActive(false);
            surface.AddComponent<BoxCollider2D>();
            var interaction = surface.AddComponent<InteractableComponent>();
            var controller = surface.AddComponent<CameraControllerComponent>();
            typeof(CameraControllerComponent).GetField("_surface", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(controller, surface);
            surface.SetActive(true);
            controller.CameraMovementSetting(new CameraMovementSetting(new Rect(-20, -20, 40, 40), 2, 10, 1, 1000));
            controller.ActivateCamera(camera);
            controller.SetCanInteract(true);
            try
            {
                Vector2 start = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                interaction.OnPointerDown(new WorldInteractionContext(start, 1));
                interaction.OnBeginDrag(new WorldInteractionContext(start, 1));
                Vector2 end = start + new Vector2(100, 0);
                interaction.OnDrag(new WorldInteractionContext(end, 1));
                yield return null;
                yield return null;
                Assert.That(camera.transform.position.x, Is.LessThan(0));
                Assert.That(surface.transform.position.x, Is.EqualTo(camera.transform.position.x).Within(0.01f));

                interaction.OnPointerDown(new WorldInteractionContext(end + new Vector2(100, 0), 2));
                interaction.OnDrag(new WorldInteractionContext(end + new Vector2(200, 0), 2));
                Assert.That(camera.orthographicSize, Is.LessThan(5));
                Assert.That(camera.orthographicSize, Is.InRange(2, 10));
                interaction.OnPointerUp(new WorldInteractionContext(end, 1), true);
                interaction.OnEndDrag(new WorldInteractionContext(end, 1), true);
                interaction.OnDrag(new WorldInteractionContext(end + new Vector2(10000, 0), 2));
                yield return null;
                yield return null;
                float halfWidth = camera.orthographicSize * camera.aspect;
                Assert.That(camera.transform.position.x, Is.InRange(-20 + halfWidth - 0.01f, 20 - halfWidth + 0.01f));

                controller.SetCanInteract(false);
                Vector3 stopped = camera.transform.position;
                interaction.OnDrag(new WorldInteractionContext(start, 2));
                yield return null;
                Assert.That(camera.transform.position, Is.EqualTo(stopped));
            }
            finally
            {
                Object.Destroy(surface);
                Object.Destroy(cameraObject);
            }
        }
    }
}
