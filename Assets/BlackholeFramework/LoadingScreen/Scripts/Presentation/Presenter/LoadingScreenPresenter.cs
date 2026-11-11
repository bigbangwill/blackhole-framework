using System;
using Blackhole.LoadingScreen.Application;
using Blackhole.LoadingScreen.Contract;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blackhole.LoadingScreen.Presentation
{
    [RequireComponent(typeof(PanelRenderer))]
    public class LoadingScreenPresenter : MonoBehaviour, ILoadingScreenPresenter
    {
        private PanelRenderer _panelRenderer;

        private const float FadeDurationSeconds = 0.5f;
        private const float SpinnerDegreesPerSecond = 300f;
        private const string VisibleTransitionClass = "loading-transition-panel--visible";
        private VisualElement _blackPanel;
        private VisualElement _transparentPanel;
        private VisualElement _transitionPanel;
        private VisualElement _transitionSpinner;
        private const float TransparentPanelMaxOpacity = 0f;
        private int _transitionVersion;
        private int _overlayVersion;
        private bool _overlayVisible;
        private float _spinnerAngle;

        private void Awake()
        {
            _panelRenderer = GetComponent<PanelRenderer>();
        }

        private void OnEnable()
        {
            _panelRenderer.RegisterUIReloadCallback(OnLoadCallback);
        }

        private void OnDisable()
        {
            _transitionVersion++;
            CloseTransitionAsync();
            _panelRenderer.UnregisterUIReloadCallback(OnLoadCallback);
            Hide(_blackPanel);
            Hide(_transparentPanel);
            _blackPanel = null;
            _transparentPanel = null;
            _transitionPanel = null;
            _transitionSpinner = null;
        }

        private void OnLoadCallback(PanelRenderer panelRenderer, VisualElement rootElement)
        {
            if (_blackPanel != null || _transparentPanel != null)
                _transitionVersion++;

            _blackPanel = rootElement.Q<VisualElement>("BlackPanel");
            _transparentPanel = rootElement.Q<VisualElement>("TransparentPanel");
            _transitionPanel = rootElement.Q<VisualElement>("TransitionPanel");
            _transitionSpinner = _transitionPanel?.Q<VisualElement>("TransitionSpinner");
            Hide(_blackPanel);
            Hide(_transparentPanel);
            SetOverlayVisible(_overlayVisible);
        }

        private void Update()
        {
            if (!_overlayVisible || _transitionSpinner == null)
                return;

            _spinnerAngle = (_spinnerAngle + SpinnerDegreesPerSecond * Time.unscaledDeltaTime) % 360f;
            _transitionSpinner.style.rotate = new Rotate(new Angle(_spinnerAngle, AngleUnit.Degree));
        }

        public async UniTask OpenTransitionAsync()
        {
            int version = _overlayVersion;
            for (int frame = 0; frame < 300 && _transitionPanel == null && _blackPanel == null; frame++)
            {
                CheckOverlayVersion(version);
                await UniTask.Yield();
            }

            CheckOverlayVersion(version);
            if (_transitionPanel == null && _blackPanel == null)
                throw new InvalidOperationException("The loading transition panel did not become ready.");

            SetOverlayVisible(true);
            if (UnityEngine.Application.isPlaying)
                await UniTask.NextFrame();
            CheckOverlayVersion(version);
        }

        public UniTask CloseTransitionAsync()
        {
            _overlayVersion++;
            if (_overlayVisible)
                SetOverlayVisible(false);
            return UniTask.CompletedTask;
        }

        private void CheckOverlayVersion(int version)
        {
            if (version != _overlayVersion)
                throw new OperationCanceledException("The loading transition was closed before it became ready.");
        }

        private void SetOverlayVisible(bool visible)
        {
            _overlayVisible = visible;
            if (_transitionPanel != null)
            {
                _transitionPanel.EnableInClassList(VisibleTransitionClass, visible);
                if (visible)
                    _transitionPanel.BringToFront();
            }
            else if (_blackPanel != null && visible)
            {
                _blackPanel.style.display = DisplayStyle.Flex;
                _blackPanel.style.opacity = 1f;
            }
            else
            {
                Hide(_blackPanel);
            }
        }

        public async UniTask FadeIn(LoadingScreenType type)
        {
            int version = ++_transitionVersion;
            await WaitForPanel(type);
            if (version != _transitionVersion)
                return;

            VisualElement panel = GetPanel(type);
            Hide(GetPanel(type == LoadingScreenType.Black ? LoadingScreenType.Transparent : LoadingScreenType.Black));
            panel.style.display = DisplayStyle.Flex;
            await Fade(panel, panel.resolvedStyle.opacity, GetTargetOpacity(type), version);
        }

        public async UniTask FadeOut(LoadingScreenType type)
        {
            int version = ++_transitionVersion;
            await WaitForPanel(type);
            if (version != _transitionVersion)
                return;

            VisualElement panel = GetPanel(type);
            if (panel.style.display.value == DisplayStyle.None)
                return;

            await Fade(panel, panel.resolvedStyle.opacity, 0f, version);
            if (version == _transitionVersion)
                Hide(panel);
        }

        private static float GetTargetOpacity(LoadingScreenType type)
        {
            return type switch
            {
                LoadingScreenType.Black => 1f,
                LoadingScreenType.Transparent => TransparentPanelMaxOpacity,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        private VisualElement GetPanel(LoadingScreenType type)
        {
            return type switch
            {
                LoadingScreenType.Black => _blackPanel,
                LoadingScreenType.Transparent => _transparentPanel,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        private async UniTask WaitForPanel(LoadingScreenType type)
        {
            for (int frame = 0; frame < 300; frame++)
            {
                if (GetPanel(type) != null)
                    return;

                await UniTask.Yield();
            }

            throw new InvalidOperationException($"The loading panel for '{type}' did not become ready.");
        }

        private async UniTask Fade(VisualElement panel, float from, float to, int version)
        {
            float startTime = Time.realtimeSinceStartup;
            panel.style.opacity = from;

            while (version == _transitionVersion)
            {
                float progress = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / FadeDurationSeconds);
                panel.style.opacity = Mathf.Lerp(from, to, progress);
                if (progress >= 1f)
                    return;

                await UniTask.Yield();
            }
        }

        private static void Hide(VisualElement panel)
        {
            if (panel == null)
                return;

            panel.style.display = DisplayStyle.None;
            panel.style.opacity = 0f;
        }
    }
}
