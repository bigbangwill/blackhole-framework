using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Blackhole.Framework.Composition.Editor
{
    [EditorWindowTitle(title = "Scenes", icon = "SceneAsset Icon")]
    public sealed class SceneBrowserWindow : EditorWindow
    {
        [SerializeField] private string _search = string.Empty;

        private readonly List<SceneEntry> _scenes = new();
        private ListView _sceneList;

        [MenuItem("Window/General/Scene Browser")]
        public static void ShowWindow()
        {
            GetWindow<SceneBrowserWindow>().RebuildUI();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Scenes", EditorGUIUtility.IconContent("SceneAsset Icon").image);
            minSize = new Vector2(240, 180);
            EditorApplication.projectChanged += RefreshScenes;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.delayCall += RebuildUI;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosed += OnSceneChanged;
            EditorSceneManager.sceneSaved += OnSceneChanged;
            EditorSceneManager.sceneDirtied += OnSceneChanged;
            EditorSceneManager.newSceneCreated += OnNewSceneCreated;
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= RefreshScenes;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.delayCall -= RebuildUI;
            EditorApplication.delayCall -= RefreshSceneState;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosed -= OnSceneChanged;
            EditorSceneManager.sceneSaved -= OnSceneChanged;
            EditorSceneManager.sceneDirtied -= OnSceneChanged;
            EditorSceneManager.newSceneCreated -= OnNewSceneCreated;
            EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged;
        }

        private void RebuildUI()
        {
            if (!this) return;

            CreateGUI();
            Repaint();
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();

            Toolbar toolbar = new();
            ToolbarSearchField searchField = new();
            searchField.style.flexGrow = 1;
            searchField.style.flexShrink = 1;
            searchField.style.flexBasis = 0;
            searchField.style.minWidth = 0;
            searchField.style.width = 0;
            searchField.SetValueWithoutNotify(_search);
            searchField.RegisterValueChangedCallback(evt =>
            {
                _search = evt.newValue;
                FilterScenes();
            });
            toolbar.Add(searchField);
            ToolbarButton refreshButton = new(RebuildUI) { text = "Refresh" };
            refreshButton.style.flexShrink = 0;
            toolbar.Add(refreshButton);
            rootVisualElement.Add(toolbar);

            _sceneList = new ListView
            {
                fixedItemHeight = EditorGUIUtility.singleLineHeight,
                selectionType = SelectionType.Single,
                virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,
                makeItem = MakeSceneRow,
                bindItem = BindSceneRow
            };
            _sceneList.style.flexGrow = 1;
            _sceneList.itemsChosen += OpenChosenScene;
            _sceneList.selectionChanged += _ => UpdateActionVisibility();
            rootVisualElement.Add(_sceneList);

            RefreshScenes();
            UpdateAvailability();
        }

        private void RefreshScenes()
        {
            _scenes.Clear();
            _scenes.AddRange(AssetDatabase.FindAssets("t:Scene", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => new SceneEntry(path))
                .OrderBy(scene => scene.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(scene => scene.Path, StringComparer.OrdinalIgnoreCase));
            FilterScenes();
        }

        private void FilterScenes()
        {
            if (_sceneList == null) return;

            string query = (_search ?? string.Empty).Trim();
            List<SceneEntry> filtered = _scenes.Where(scene => scene.Path.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            _sceneList.itemsSource = filtered;
            _sceneList.Rebuild();
        }

        private VisualElement MakeSceneRow()
        {
            VisualElement row = new();
            row.style.paddingLeft = 6;
            row.style.paddingRight = 6;
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;

            VisualElement status = new() { name = "scene-status", pickingMode = PickingMode.Ignore };
            status.style.width = 6;
            status.style.height = 6;
            status.style.marginRight = 6;
            status.style.borderTopLeftRadius = 3;
            status.style.borderTopRightRadius = 3;
            status.style.borderBottomLeftRadius = 3;
            status.style.borderBottomRightRadius = 3;
            row.Add(status);

            Label name = new() { name = "scene-name" };
            name.style.marginTop = 0;
            name.style.marginBottom = 0;
            name.style.textOverflow = TextOverflow.Ellipsis;
            name.style.flexGrow = 1;
            name.style.flexShrink = 1;
            name.style.minWidth = 0;
            row.Add(name);

            VisualElement actions = new() { name = "scene-actions" };
            actions.style.flexDirection = FlexDirection.Row;
            actions.style.flexShrink = 0;
            actions.style.visibility = Visibility.Hidden;
            actions.Add(MakeActionButton("open-scene", "↗", new Color(0.35f, 0.7f, 1f), () =>
            {
                if (row.userData is SceneEntry scene) OpenScene(scene, OpenSceneMode.Single);
            }));
            actions.Add(MakeActionButton("toggle-scene", "+", new Color(0.4f, 0.85f, 0.55f), () =>
            {
                if (row.userData is not SceneEntry scene) return;
                if (SceneManager.GetSceneByPath(scene.Path).isLoaded) UnloadScene(scene);
                else OpenScene(scene, OpenSceneMode.Additive);
            }));
            row.Add(actions);
            row.RegisterCallback<MouseEnterEvent>(_ =>
            {
                row.AddToClassList("scene-hovered");
                UpdateRowActionVisibility(row);
            });
            row.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                row.RemoveFromClassList("scene-hovered");
                UpdateRowActionVisibility(row);
            });
            row.RegisterCallback<FocusInEvent>(_ => actions.style.visibility = Visibility.Visible);
            row.RegisterCallback<FocusOutEvent>(_ => UpdateRowActionVisibility(row));

            row.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                if (row.userData is not SceneEntry scene) return;

                bool canLoad = !EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling;
                bool loaded = SceneManager.GetSceneByPath(scene.Path).isLoaded;
                evt.menu.AppendAction("Load Additive", _ => OpenScene(scene, OpenSceneMode.Additive),
                    canLoad && !loaded ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                if (loaded)
                {
                    evt.menu.AppendAction("Remove Scene", _ => UnloadScene(scene),
                        canLoad && SceneManager.loadedSceneCount > 1 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                }
                evt.StopPropagation();
            }));
            return row;
        }

        private void BindSceneRow(VisualElement row, int index)
        {
            SceneEntry scene = (SceneEntry)_sceneList.itemsSource[index];
            row.userData = scene;
            Scene editorScene = SceneManager.GetSceneByPath(scene.Path);
            bool loaded = editorScene.IsValid() && editorScene.isLoaded;
            bool active = loaded && editorScene == SceneManager.GetActiveScene();
            Label name = row.Q<Label>("scene-name");
            name.text = scene.Name + (loaded && editorScene.isDirty ? " *" : string.Empty);
            name.style.unityFontStyleAndWeight = active ? FontStyle.Bold : FontStyle.Normal;
            VisualElement status = row.Q("scene-status");
            status.style.backgroundColor = active ? new Color(0.35f, 0.7f, 1f) : loaded ? new Color(0.4f, 0.85f, 0.55f) : new Color(0.5f, 0.5f, 0.5f);
            status.tooltip = active ? "Active scene" : loaded ? "Loaded" : "Not loaded";
            row.tooltip = scene.Name;

            Button open = row.Q<Button>("open-scene");
            open.tooltip = "Open scene on its own";
            Button toggle = row.Q<Button>("toggle-scene");
            toggle.text = loaded ? "×" : "+";
            toggle.style.color = loaded ? new Color(1f, 0.45f, 0.4f) : new Color(0.4f, 0.85f, 0.55f);
            bool canUnload = !loaded || SceneManager.loadedSceneCount > 1;
            toggle.SetEnabled(canUnload);
            toggle.tooltip = !canUnload ? "Unity requires at least one loaded scene" : loaded ? "Remove scene from Hierarchy" : "Load scene additively";
            UpdateRowActionVisibility(row);
        }

        private static Button MakeActionButton(string name, string icon, Color color, Action action)
        {
            Button button = new(action) { name = name, text = icon };
            button.style.width = 20;
            button.style.height = 16;
            button.style.marginLeft = 2;
            button.style.marginRight = 0;
            button.style.marginTop = 0;
            button.style.marginBottom = 0;
            button.style.paddingLeft = 0;
            button.style.paddingRight = 0;
            button.style.paddingTop = 0;
            button.style.paddingBottom = 0;
            button.style.fontSize = 14;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.color = color;
            button.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
            button.RegisterCallback<ClickEvent>(evt => evt.StopPropagation());
            return button;
        }

        private void UpdateActionVisibility()
        {
            _sceneList?.Query<VisualElement>().Where(element => element.userData is SceneEntry).ForEach(UpdateRowActionVisibility);
        }

        private void UpdateRowActionVisibility(VisualElement row)
        {
            bool selected = row.userData is SceneEntry scene && ReferenceEquals(_sceneList?.selectedItem, scene);
            VisualElement actions = row.Q("scene-actions");
            bool focused = actions.Contains(actions.panel?.focusController?.focusedElement as VisualElement);
            actions.style.visibility = row.ClassListContains("scene-hovered") || selected || focused ? Visibility.Visible : Visibility.Hidden;
        }

        private void OpenChosenScene(IEnumerable<object> chosenItems)
        {
            OpenScene(chosenItems.OfType<SceneEntry>().FirstOrDefault(), OpenSceneMode.Single);
        }

        private void OpenScene(SceneEntry scene, OpenSceneMode mode)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (scene == null) return;
            if (mode == OpenSceneMode.Additive && SceneManager.GetSceneByPath(scene.Path).isLoaded) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.Path) == null)
            {
                RefreshScenes();
                return;
            }

            if (mode == OpenSceneMode.Single && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(scene.Path, mode);
            QueueSceneStateRefresh();
        }

        private void UnloadScene(SceneEntry scene)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;

            Scene editorScene = SceneManager.GetSceneByPath(scene.Path);
            if (!editorScene.IsValid() || !editorScene.isLoaded || SceneManager.loadedSceneCount <= 1) return;
            if (!EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new[] { editorScene })) return;

            EditorSceneManager.CloseScene(editorScene, true);
            QueueSceneStateRefresh();
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode) => QueueSceneStateRefresh();
        private void OnSceneChanged(Scene scene) => QueueSceneStateRefresh();
        private void OnNewSceneCreated(Scene scene, NewSceneSetup setup, NewSceneMode mode) => QueueSceneStateRefresh();
        private void OnActiveSceneChanged(Scene previous, Scene next) => QueueSceneStateRefresh();

        private void QueueSceneStateRefresh()
        {
            EditorApplication.delayCall -= RefreshSceneState;
            EditorApplication.delayCall += RefreshSceneState;
        }

        private void RefreshSceneState()
        {
            if (!this) return;

            _sceneList?.RefreshItems();
            UpdateAvailability();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            UpdateAvailability();
            QueueSceneStateRefresh();
        }

        private void UpdateAvailability()
        {
            _sceneList?.SetEnabled(!EditorApplication.isPlayingOrWillChangePlaymode);
        }

        private sealed class SceneEntry
        {
            public string Name { get; }
            public string Path { get; }

            public SceneEntry(string path)
            {
                Path = path;
                Name = System.IO.Path.GetFileNameWithoutExtension(path);
            }
        }
    }
}
