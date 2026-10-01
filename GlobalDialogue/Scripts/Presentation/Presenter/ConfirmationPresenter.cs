using Blackhole.GlobalDialogue.Application;
using Blackhole.GlobalDialogue.Contract;
using Blackhole.SharedKernel.Contract;
using Blackhole.SharedKernel.Presentaion;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Blackhole.GlobalDialogue.Presentation
{
    [RequireComponent(typeof(PanelRenderer))]
    public class ConfirmationPresenter : Orientation, IConfirmatinOutput
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void WebGLFullscreenInitialize();

        [DllImport("__Internal")]
        private static extern void WebGLFullscreenToggle();
#endif

        private PanelRenderer _panelRenderer;

        private readonly Queue<ConfirmationRequest> _requests = new();
        private ConfirmationRequest _activeRequest;
        private bool _acceptRequests;

        private VisualElement _confirmationVE;
        private VisualElement _yesOrNoButtonHolder;
        private VisualElement _confirmButtonHolder;

        private Label _detailDialogue;

        private Button _yesButton;
        private Button _noButton;
        private Button _confirmButton;
        private Button _fullScreenButton;

        private IScopedHandlerRegistery<IConfirmatinOutput> _handler;

        [Inject]
        public void Construct(
            IScopedHandlerRegistery<IConfirmatinOutput> handler)
        {
            _handler = handler;
        }

        protected override void Awake()
        {
            base.Awake();

            _handler.Register(this);
        }

        private void OnEnable()
        {
            _acceptRequests = true;
            _panelRenderer = GetComponent<PanelRenderer>();
            _panelRenderer.RegisterUIReloadCallback(OnUIReloaded);
        }

        private void OnDisable()
        {
            _acceptRequests = false;
            _panelRenderer.UnregisterUIReloadCallback(OnUIReloaded);
            UnbindButtons();
            CancelPendingRequests();
        }

        protected override void OnUIReloaded(
            PanelRenderer panelRenderer,
            VisualElement root)
        {
            UnbindButtons();
            base.OnUIReloaded(panelRenderer, root);

            _confirmationVE = DocumentRoot.Q<VisualElement>("ConfirmationPanelVE");

            _yesOrNoButtonHolder =
                _confirmationVE.Q<VisualElement>("YesOrNoButtonHolder");

            _confirmButtonHolder =
                _confirmationVE.Q<VisualElement>("ConfirmButtonHolder");

            _detailDialogue =
                _confirmationVE.Q<Label>("DetailLabel");

            _yesButton =
                _yesOrNoButtonHolder.Q<Button>("YesButton");

            _noButton =
                _yesOrNoButtonHolder.Q<Button>("NoButton");

            _confirmButton =
                _confirmButtonHolder.Q<Button>("ConfirmButton");

            _fullScreenButton =
                DocumentRoot.Q<Button>("FullScreenButton");

            _yesButton.RegisterCallback<ClickEvent>(OnYesClicked);
            _noButton.RegisterCallback<ClickEvent>(OnNoClicked);
            _confirmButton.RegisterCallback<ClickEvent>(OnConfirmClicked);
            _fullScreenButton.RegisterCallback<ClickEvent>(OnFullScreenClicked);

            if (_activeRequest != null)
                DisplayRequest(_activeRequest);
            else
            {
                Hide();
                ShowNextRequest();
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLFullscreenInitialize();
#endif
        }

        public UniTask<ConfirmationResult> ShowYesOrNoPanel(
            YesOrNoPanelSetting setting)
        {
            return EnqueueRequest(new ConfirmationRequest(setting));
        }

        public UniTask<ConfirmationResult> ShowConfirmPanel(
            ConfirmationPanelSetting setting)
        {
            return EnqueueRequest(new ConfirmationRequest(setting));
        }

        private UniTask<ConfirmationResult> EnqueueRequest(ConfirmationRequest request)
        {
            if (!_acceptRequests)
                return UniTask.FromCanceled<ConfirmationResult>();

            _requests.Enqueue(request);
            ShowNextRequest();
            return request.Completion.Task;
        }

        private void ShowNextRequest()
        {
            if (!_acceptRequests || _activeRequest != null || _confirmationVE == null || _requests.Count == 0)
                return;

            _activeRequest = _requests.Dequeue();
            DisplayRequest(_activeRequest);
        }

        private void DisplayRequest(ConfirmationRequest request)
        {
            if (request.IsYesOrNo)
            {
                _detailDialogue.text = request.YesOrNoSetting.Description;
                _yesButton.text = request.YesOrNoSetting.YesButtonDescription;
                _noButton.text = request.YesOrNoSetting.NoButtonDescription;
                _yesOrNoButtonHolder.style.display = DisplayStyle.Flex;
                _confirmButtonHolder.style.display = DisplayStyle.None;
            }
            else
            {
                _detailDialogue.text = request.ConfirmSetting.Description;
                _confirmButton.text = request.ConfirmSetting.ButtonDescription;
                _yesOrNoButtonHolder.style.display = DisplayStyle.None;
                _confirmButtonHolder.style.display = DisplayStyle.Flex;
            }

            _confirmationVE.style.display = DisplayStyle.Flex;
        }

        private void OnYesClicked(ClickEvent evt)
        {
            if (evt.button != 0)
                return;

            Complete(ConfirmationResult.Yes);
        }

        private void OnNoClicked(ClickEvent evt)
        {
            if (evt.button != 0)
                return;

            Complete(ConfirmationResult.No);
        }

        private void OnConfirmClicked(ClickEvent evt)
        {
            if (evt.button != 0)
                return;

            Complete(ConfirmationResult.Confirm);
        }

        private void Complete(ConfirmationResult result)
        {
            ConfirmationRequest request = _activeRequest;
            if (request == null)
                return;

            _activeRequest = null;
            Hide();

            // Advance before completing: a resumed caller may enqueue another request immediately.
            ShowNextRequest();
            request.Completion.TrySetResult(result);
        }

        private void UnbindButtons()
        {
            _yesButton?.UnregisterCallback<ClickEvent>(OnYesClicked);
            _noButton?.UnregisterCallback<ClickEvent>(OnNoClicked);
            _confirmButton?.UnregisterCallback<ClickEvent>(OnConfirmClicked);
            _fullScreenButton?.UnregisterCallback<ClickEvent>(OnFullScreenClicked);
        }

        private void CancelPendingRequests()
        {
            ConfirmationRequest active = _activeRequest;
            _activeRequest = null;
            ConfirmationRequest[] queued = _requests.ToArray();
            _requests.Clear();
            Hide();

            active?.Completion.TrySetCanceled();
            foreach (ConfirmationRequest request in queued)
                request.Completion.TrySetCanceled();
        }

        private void Hide()
        {
            if (_confirmationVE != null)
                _confirmationVE.style.display = DisplayStyle.None;
        }

        private void OnFullScreenClicked(ClickEvent evt)
        {
            if (evt.button != 0)
                return;
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLFullscreenToggle();
#else
            Screen.fullScreen = !Screen.fullScreen;
#endif
        }

        protected override void OnDestroy()
        {
            _acceptRequests = false;
            UnbindButtons();
            CancelPendingRequests();

            _handler.Unregister(this);

            base.OnDestroy();
        }

        private sealed class ConfirmationRequest
        {
            public readonly bool IsYesOrNo;
            public readonly YesOrNoPanelSetting YesOrNoSetting;
            public readonly ConfirmationPanelSetting ConfirmSetting;
            public readonly UniTaskCompletionSource<ConfirmationResult> Completion = new();

            public ConfirmationRequest(YesOrNoPanelSetting setting)
            {
                IsYesOrNo = true;
                YesOrNoSetting = setting;
            }

            public ConfirmationRequest(ConfirmationPanelSetting setting)
            {
                ConfirmSetting = setting;
            }
        }
    }
}
