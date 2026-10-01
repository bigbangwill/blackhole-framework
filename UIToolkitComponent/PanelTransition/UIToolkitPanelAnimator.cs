using System;
using UnityEngine.UIElements;

namespace BigBang.UIToolkitComponent
{
    public sealed class UIToolkitPanelAnimator : IDisposable
    {
        private readonly VisualElement panel;
        private readonly PanelTransitionProfileSO profile;
        private readonly bool installedStyleSheet;

        private int operationVersion;
        private bool isVisible;
        private bool isClosing;
        private bool isDisposed;

        public bool IsVisible => isVisible;
        public bool IsClosing => isClosing;

        public UIToolkitPanelAnimator(VisualElement panel, PanelTransitionType transitionType)
            : this(panel, PanelTransitionProfileSO.Load(transitionType))
        {
        }

        public UIToolkitPanelAnimator(VisualElement panel, PanelTransitionProfileSO profile)
        {
            this.panel = panel ?? throw new ArgumentNullException(nameof(panel));
            this.profile = profile ?? throw new ArgumentNullException(nameof(profile));

            installedStyleSheet = profile.InstallStyleSheet(panel);
            panel.AddToClassList(profile.BaseClass);
            ApplyClasses(false);
            panel.style.display = DisplayStyle.None;
            panel.pickingMode = PickingMode.Ignore;
            panel.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        }

        public void Show()
        {
            SetVisible(true, true);
        }

        public void Hide()
        {
            SetVisible(false, true);
        }

        public void ShowImmediately()
        {
            SetVisible(true, false);
        }

        public void HideImmediately()
        {
            SetVisible(false, false);
        }

        public void SetVisible(bool visible, bool animate = true)
        {
            ThrowIfDisposed();

            if (visible)
                ShowInternal(animate);
            else
                HideInternal(animate);
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            operationVersion++;
            panel.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
            panel.RemoveFromClassList(profile.BaseClass);
            panel.RemoveFromClassList(profile.HiddenClass);
            panel.RemoveFromClassList(profile.VisibleClass);

            if (installedStyleSheet)
                profile.UninstallStyleSheet(panel);
        }

        private void ShowInternal(bool animate)
        {
            if (isVisible && !isClosing)
                return;

            int currentOperation = ++operationVersion;
            isVisible = true;
            isClosing = false;
            panel.style.display = DisplayStyle.Flex;
            panel.pickingMode = PickingMode.Position;

            if (!animate)
            {
                ApplyClasses(true);
                return;
            }

            ApplyClasses(false);
            panel.schedule.Execute(() =>
            {
                if (isDisposed || currentOperation != operationVersion || !isVisible)
                    return;

                ApplyClasses(true);
            });
        }

        private void HideInternal(bool animate)
        {
            if (!isVisible && !isClosing)
                return;

            int currentOperation = ++operationVersion;
            isVisible = false;
            panel.pickingMode = PickingMode.Ignore;
            ApplyClasses(false);

            if (!animate || profile.CloseDurationMs == 0)
            {
                FinishClosing();
                return;
            }

            isClosing = true;
            panel.schedule.Execute(() =>
            {
                if (isDisposed || currentOperation != operationVersion || isVisible || !isClosing)
                    return;

                FinishClosing();
            }).ExecuteLater(profile.CloseDurationMs);
        }

        private void ApplyClasses(bool visible)
        {
            panel.EnableInClassList(profile.HiddenClass, !visible);
            panel.EnableInClassList(profile.VisibleClass, visible);
        }

        private void OnTransitionEnd(TransitionEndEvent evt)
        {
            if (evt.target != panel || !isClosing || isVisible)
                return;

            operationVersion++;
            FinishClosing();
        }

        private void FinishClosing()
        {
            if (profile.SetDisplayNoneAfterClose)
                panel.style.display = DisplayStyle.None;

            isClosing = false;
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(UIToolkitPanelAnimator));
        }
    }
}
