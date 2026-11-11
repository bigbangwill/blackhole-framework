using System;
using System.IO;
using Blackhole.CameraController.Implementation;
using Blackhole.Framework.Composition.Editor;
using Blackhole.WorldInteraction.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Blackhole.Development.Editor
{
    public static class UpgradeCameraControllerAssets
    {
        private const string Root = "Assets/BlackholeFramework/CameraController";

        [MenuItem("Blackhole/Development/Upgrade Camera Controller Assets")]
        public static void Upgrade()
        {
            const string inputPath = Root + "/Data/CameraController.inputactions";
            if (!File.Exists(inputPath))
            {
                var actions = ScriptableObject.CreateInstance<InputActionAsset>();
                try
                {
                    actions.name = "CameraController";
                    InputActionMap map = actions.AddActionMap("Camera");
                    map.AddAction("Zoom", InputActionType.Value, "<Mouse>/scroll/y", expectedControlLayout: "Axis");
                    File.WriteAllText(inputPath, actions.ToJson());
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(actions);
                }
            }
            AssetDatabase.ImportAsset(inputPath, ImportAssetOptions.ForceSynchronousImport);
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(inputPath);
            if (inputActions == null || inputActions.FindAction("Camera/Zoom") == null)
                throw new InvalidOperationException("Camera/Zoom input action could not be imported.");

            const string prefabPath = Root + "/Prefabs/CameraDragSurface.prefab";
            GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var controller = new SerializedObject(prefab.GetComponent<CameraControllerComponent>());
                controller.FindProperty("_settings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<CameraControllerSettingsSO>(Root + "/Data/CameraControllerSettings.asset");
                controller.FindProperty("_cameraInputActions").objectReferenceValue = inputActions;
                controller.ApplyModifiedPropertiesWithoutUndo();
                var interaction = new SerializedObject(prefab.GetComponent<InteractableComponent>());
                interaction.FindProperty("_passThrough").boolValue = true;
                interaction.FindProperty("_receiveAlongsideTarget").boolValue = true;
                interaction.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefab);
            }
            AssetDatabase.SaveAssets();
            var errors = FrameworkStartupValidator.Validate();
            if (errors.Count > 0) throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
            Debug.Log("Camera input assets upgraded; FrameworkStartupValidator passed.");
        }
    }
}
