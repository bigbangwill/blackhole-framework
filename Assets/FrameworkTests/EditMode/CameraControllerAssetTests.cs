using Blackhole.CameraController.Implementation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Blackhole.Development.Tests
{
    public sealed class CameraControllerAssetTests
    {
        [Test]
        public void FrameworkPrefabHasWheelBindingAndBoundaryPreviewSettings()
        {
            const string root = "Assets/BlackholeFramework/CameraController";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(root + "/Prefabs/CameraDragSurface.prefab");
            var controller = new SerializedObject(prefab.GetComponent<CameraControllerComponent>());
            var actions = controller.FindProperty("_cameraInputActions").objectReferenceValue as InputActionAsset;
            Assert.That(actions, Is.Not.Null);
            InputAction zoom = actions.FindAction("Camera/Zoom", true);
            Assert.That(zoom.bindings.Count, Is.EqualTo(1));
            Assert.That(zoom.bindings[0].path, Is.EqualTo("<Mouse>/scroll/y"));
            Assert.That(controller.FindProperty("_settings").objectReferenceValue, Is.EqualTo(AssetDatabase.LoadAssetAtPath<CameraControllerSettingsSO>(root + "/Data/CameraControllerSettings.asset")));
            Assert.That(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(actions)), Is.EqualTo("9d1e63b567244b33b50469fe21602e56"));
        }
    }
}
