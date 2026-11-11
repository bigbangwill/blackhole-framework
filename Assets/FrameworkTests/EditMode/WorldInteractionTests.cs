using System.Collections.Generic;
using Blackhole.WorldInteraction.Application;
using Blackhole.WorldInteraction.Contract;
using Blackhole.WorldInteraction.Infrastructure;
using Blackhole.WorldInteraction.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Blackhole.Development.Tests
{
    public sealed class WorldInteractionTests
    {
        private Camera camera;
        private GameObject target;
        private InteractableComponent interaction;
        private DetectionRaycaster raycaster;

        [SetUp]
        public void SetUp()
        {
            camera = new GameObject("Interaction Test Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 0, -10);
            camera.orthographic = true;
            camera.orthographicSize = 5;
            target = new GameObject("Interaction Target", typeof(BoxCollider2D), typeof(InteractableComponent));
            target.layer = 30;
            interaction = target.GetComponent<InteractableComponent>();
            raycaster = new DetectionRaycaster(1 << 30);
            Physics2D.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(camera.gameObject);
        }

        [Test]
        public void DragRemainsCapturedOutsideTargetAndEndsAfterPointerUp()
        {
            var events = new List<string>();
            interaction.AddBeginDragListener(data => events.Add("begin:" + data.PointerId));
            interaction.AddDragListener(data => events.Add("drag:" + data.PointerId));
            interaction.AddPointerUpListener(data => events.Add("up:" + data.IsHit));
            interaction.AddEndDragListener(data => events.Add("end:" + data.IsHit));
            raycaster.OnPointerDownRaycast(Context(Vector2.zero, 7));
            WorldInteractionContext outside = Context(new Vector2(3, 0), 7);
            raycaster.OnBeginDragRaycast(outside);
            raycaster.OnDragRaycast(outside);
            raycaster.OnPointerUpRaycast(outside);
            raycaster.OnEndDragRaycast(outside);
            raycaster.OnDragRaycast(outside);
            raycaster.OnEndDragRaycast(outside);
            Assert.That(events, Is.EqualTo(new[] { "begin:7", "drag:7", "up:False", "end:False" }));
        }

        [Test]
        public void ReleasingOnePointerDoesNotReleaseTheOther()
        {
            var released = new List<int>();
            interaction.AddPointerUpListener(data => released.Add(data.PointerId));
            raycaster.OnPointerDownRaycast(Context(Vector2.zero, 1));
            raycaster.OnPointerDownRaycast(Context(Vector2.zero, 2));
            raycaster.OnPointerUpRaycast(Context(Vector2.zero, 1));
            raycaster.OnPointerUpRaycast(Context(Vector2.zero, 1));
            raycaster.OnPointerUpRaycast(Context(Vector2.zero, 2));
            Assert.That(released, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void PerspectiveCameraFindsTargetOnGameplayPlane()
        {
            camera.orthographic = false;
            target.transform.position = new Vector3(2, 1, 0);
            Physics2D.SyncTransforms();
            int clicks = 0;
            interaction.AddPointerClickedListener(data => clicks++);
            raycaster.OnPointerClickedRaycast(Context(new Vector2(2, 1), 4));
            Assert.That(clicks, Is.EqualTo(1));
        }

        [Test]
        public void SortingGroupSelectsTheFrontTargetAndKeepsTheCameraSurfaceInTheRoute()
        {
            var front = new GameObject("Front Target", typeof(SortingGroup), typeof(BoxCollider2D), typeof(InteractableComponent));
            var surface = new GameObject("Drag Surface", typeof(BoxCollider2D), typeof(InteractableComponent));
            try
            {
                front.layer = surface.layer = target.layer;
                front.GetComponent<SortingGroup>().sortingOrder = 5;
                var dragSurface = surface.GetComponent<InteractableComponent>();
                var serialized = new SerializedObject(dragSurface);
                serialized.FindProperty("_receiveAlongsideTarget").boolValue = true;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Physics2D.SyncTransforms();

                var events = new List<string>();
                var selectable = front.GetComponent<InteractableComponent>();
                selectable.AddPointerClickedListener(data => events.Add("front:" + data.PointerButton));
                selectable.AddDeselectListener(data => events.Add("deselect"));
                dragSurface.AddPointerClickedListener(data => events.Add("surface"));
                interaction.AddPointerClickedListener(data => events.Add("behind"));
                Vector2 screen = camera.WorldToScreenPoint(Vector3.zero);
                raycaster.OnPointerClickedRaycast(new WorldInteractionContext(screen, 7, WorldInteractionPointerButton.Right));
                raycaster.OnPointerClickedRaycast(Context(new Vector2(3, 0), 7));
                Assert.That(events, Is.EqualTo(new[] { "front:Right", "surface", "deselect" }));
            }
            finally
            {
                Object.DestroyImmediate(front);
                Object.DestroyImmediate(surface);
            }
        }

        [Test]
        public void MultipleCollidersOnOneTargetReceiveOneEventPerMouseButton()
        {
            target.AddComponent<CircleCollider2D>();
            Physics2D.SyncTransforms();
            var buttons = new List<InteractionPointerButton>();
            interaction.AddPointerUpListener(data => buttons.Add(data.PointerButton));
            var left = Context(Vector2.zero, 1);
            var right = new WorldInteractionContext(left.ScreenPosition, left.PointerId, WorldInteractionPointerButton.Right);
            raycaster.OnPointerDownRaycast(left);
            raycaster.OnPointerDownRaycast(right);
            raycaster.OnPointerUpRaycast(right);
            raycaster.OnPointerUpRaycast(left);
            Assert.That(buttons, Is.EqualTo(new[] { InteractionPointerButton.Right, InteractionPointerButton.Left }));
        }

        private WorldInteractionContext Context(Vector2 world, int pointerId) => new(camera.WorldToScreenPoint(world), pointerId);
    }
}
