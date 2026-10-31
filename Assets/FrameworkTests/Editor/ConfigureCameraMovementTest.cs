using System;
using System.IO;
using System.Linq;
using Blackhole.CameraController.Implementation;
using Blackhole.Development.CameraMovement;
using Blackhole.Framework.Composition;
using Blackhole.Framework.Composition.Editor;
using Blackhole.ModuleSystem.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blackhole.Development.Editor
{
    public static class ConfigureCameraMovementTest
    {
        public const string Root = "Assets/FrameworkTests/CameraMovement";
        public const string ScenePath = Root + "/CameraMovementTestScene.unity";
        public const string ModuleId = "Blackhole.Development.CameraMovement";

        [MenuItem("Blackhole/Development/Configure Camera Movement Test")]
        public static void Configure()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ScenePath)) CreateScene();
            CameraMovementTestInstaller installer = LoadOrCreate<CameraMovementTestInstaller>(Root + "/CameraMovementTestInstaller.asset");
            ModuleManifestSO manifest = LoadOrCreate<ModuleManifestSO>(Root + "/Blackhole.Development.CameraMovement.asset");
            SerializedObject module = new(manifest);
            module.FindProperty("moduleID").stringValue = ModuleId;
            module.FindProperty("moduleScene").stringValue = ScenePath;
            module.FindProperty("installer").objectReferenceValue = installer;
            module.FindProperty("isPersistant").boolValue = true;
            SerializedProperty dependencies = module.FindProperty("dependencies");
            dependencies.arraySize = 1;
            dependencies.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<ModuleManifestSO>("Assets/BlackholeFramework/CameraController/Blackhole.CameraController.asset");
            module.ApplyModifiedPropertiesWithoutUndo();
            BlackholeProjectSettingsSO settings = AssetDatabase.LoadAssetAtPath<BlackholeProjectSettingsSO>(FrameworkProjectSetup.SettingsPath);
            SerializedObject project = new(settings);
            project.FindProperty("startingModule").objectReferenceValue = manifest;
            project.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            FrameworkProjectSetup.RegenerateSettings();
            UnityEngine.Object[] buildSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/EditorBuildSettings.asset");
            UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(buildSettings, "ProjectSettings/EditorBuildSettings.asset", true);
            var errors = FrameworkStartupValidator.Validate();
            if (errors.Count > 0) throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
            EditorSceneManager.OpenScene(FrameworkLifetimeScope.FrameworkScenePath);
            Debug.Log("Camera movement test configured. Play the framework scene to load the camera test module.");
        }

        public static void ConfigureBatch()
        {
            try { Configure(); EditorApplication.Exit(0); }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        private static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CameraControllerSettingsSO settings = AssetDatabase.LoadAssetAtPath<CameraControllerSettingsSO>("Assets/BlackholeFramework/CameraController/Data/CameraControllerSettings.asset");
            Rect bounds = settings.CreateMovementSetting().Boundary;
            Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 6;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.045f, 0.075f);
            camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10);
            camera.gameObject.AddComponent<AudioListener>();
            CreateMap(bounds);

            const string panelPath = Root + "/CameraMovementPanelSettings.asset";
            if (AssetDatabase.LoadAssetAtPath<PanelSettings>(panelPath) == null)
                AssetDatabase.CopyAsset("Assets/BlackholeFramework/LoadingScreen/Scripts/Presentation/UI/LoadingScreenPanelSettings.asset", panelPath);
            PanelSettings panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelPath);
            panel.referenceResolution = new Vector2Int(1280, 720);
            panel.match = 0.5f;
            EditorUtility.SetDirty(panel);
            GameObject hud = new("Camera Movement Test HUD");
            UIDocument document = hud.AddComponent<UIDocument>();
            document.panelSettings = panel;
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Root + "/CameraMovementTest.uxml");
            CameraMovementTestView view = hud.AddComponent<CameraMovementTestView>();
            SerializedObject serializedView = new(view);
            serializedView.FindProperty("testCamera").objectReferenceValue = camera;
            serializedView.FindProperty("document").objectReferenceValue = document;
            serializedView.ApplyModifiedPropertiesWithoutUndo();
            CameraMovementTestScope scope = new GameObject("Camera Movement Test Scope").AddComponent<CameraMovementTestScope>();
            SerializedObject serializedScope = new(scope);
            serializedScope.FindProperty("view").objectReferenceValue = view;
            serializedScope.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void CreateMap(Rect bounds)
        {
            const string spritePath = Root + "/MapSquare.asset";
            Sprite square = AssetDatabase.LoadAllAssetsAtPath(spritePath).OfType<Sprite>().FirstOrDefault();
            if (square == null)
            {
                Texture2D texture = new(1, 1, TextureFormat.RGBA32, false) { name = "Map Square Texture", filterMode = FilterMode.Point };
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();
                AssetDatabase.CreateAsset(texture, spritePath);
                square = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1, 0, SpriteMeshType.FullRect);
                square.name = "Map Square";
                AssetDatabase.AddObjectToAsset(square, texture);
            }
            const string materialPath = Root + "/MapUnlit.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")) { name = "Map Unlit" };
                AssetDatabase.CreateAsset(material, materialPath);
            }
            Transform map = new GameObject("Camera Test Map").transform;
            void Rectangle(string name, Vector2 center, Vector2 size, Color color, int order)
            {
                SpriteRenderer renderer = new GameObject(name).AddComponent<SpriteRenderer>();
                renderer.transform.SetParent(map);
                renderer.transform.position = center;
                renderer.transform.localScale = new Vector3(size.x, size.y, 1);
                renderer.sprite = square;
                renderer.sharedMaterial = material;
                renderer.color = color;
                renderer.sortingOrder = order;
            }
            Color grid = new(0.12f, 0.22f, 0.31f);
            Color cyan = new(0.25f, 0.84f, 0.81f);
            Color gold = new(0.95f, 0.76f, 0.38f);
            Rectangle("Map Background", bounds.center, bounds.size, new Color(0.045f, 0.085f, 0.14f), -100);
            for (float x = bounds.xMin; x <= bounds.xMax; x += 2) Rectangle("Grid X " + x, new Vector2(x, bounds.center.y), new Vector2(0.025f, bounds.height), grid, -90);
            for (float y = bounds.yMin; y <= bounds.yMax; y += 2) Rectangle("Grid Y " + y, new Vector2(bounds.center.x, y), new Vector2(bounds.width, 0.025f), grid, -90);
            Rectangle("Origin X", new Vector2(bounds.center.x, 0), new Vector2(bounds.width, 0.045f), cyan * 0.55f, -80);
            Rectangle("Origin Y", new Vector2(0, bounds.center.y), new Vector2(0.045f, bounds.height), cyan * 0.55f, -80);
            Rectangle("West Boundary", new Vector2(bounds.xMin + 0.06f, bounds.center.y), new Vector2(0.12f, bounds.height), gold, -70);
            Rectangle("East Boundary", new Vector2(bounds.xMax - 0.06f, bounds.center.y), new Vector2(0.12f, bounds.height), gold, -70);
            Rectangle("South Boundary", new Vector2(bounds.center.x, bounds.yMin + 0.06f), new Vector2(bounds.width, 0.12f), gold, -70);
            Rectangle("North Boundary", new Vector2(bounds.center.x, bounds.yMax - 0.06f), new Vector2(bounds.width, 0.12f), gold, -70);
            Color[] colors = { cyan, new(0.95f, 0.43f, 0.38f), new(0.62f, 0.49f, 0.95f), gold };
            Vector2[] offsets = { new(-0.33f, 0.32f), new(0.33f, 0.32f), new(-0.33f, -0.32f), new(0.33f, -0.32f) };
            for (int i = 0; i < offsets.Length; i++)
            {
                Vector2 position = bounds.center + Vector2.Scale(bounds.size, offsets[i]);
                Rectangle("Landmark " + (i + 1), position, new Vector2(1.4f, 1.4f), colors[i], -60);
                Rectangle("Landmark Center " + (i + 1), position, new Vector2(0.65f, 0.65f), new Color(0.05f, 0.09f, 0.15f), -50);
            }
            Rectangle("Origin Marker", bounds.center, new Vector2(0.65f, 0.65f), cyan, -50);
            AssetDatabase.SaveAssets();
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
