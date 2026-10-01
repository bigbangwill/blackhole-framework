using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace BigBang.UIToolkitComponent
{
    public enum PanelTransitionType
    {
        FromTop,
        Fade,
        ScaleFade,
        ZoomFade,
        SlideLeft,
        SlideRight,
        SlideUp,
        SlideDown
    }

    [CreateAssetMenu(fileName = "PanelTransitionProfile", menuName = "BigBang/UI Toolkit/Panel Transition Profile")]
    public sealed class PanelTransitionProfileSO : ScriptableObject
    {
        private const string ResourcesProfilesPath = "PanelTransition/Profiles";
        private const string DefaultBaseClass = "panel-transition";
        private const string DefaultHiddenClass = "panel-hidden";
        private const string DefaultVisibleClass = "panel-visible";

        [Header("USS")]
        [SerializeField] private StyleSheet styleSheet = null;

        [Header("Classes")]
        [SerializeField] private string baseClass = DefaultBaseClass;
        [SerializeField] private string hiddenClass = DefaultHiddenClass;
        [SerializeField] private string visibleClass = DefaultVisibleClass;

        [Header("Timing")]
        [Tooltip("Fallback delay used when UI Toolkit does not send a TransitionEndEvent. Match this to the longest duration in the assigned USS file.")]
        [Min(0)]
        [SerializeField] private int closeDurationMs = 250;

        [SerializeField] private bool setDisplayNoneAfterClose = true;

        public StyleSheet StyleSheet => styleSheet;
        public string BaseClass => string.IsNullOrWhiteSpace(baseClass) ? DefaultBaseClass : baseClass;
        public string HiddenClass => string.IsNullOrWhiteSpace(hiddenClass) ? DefaultHiddenClass : hiddenClass;
        public string VisibleClass => string.IsNullOrWhiteSpace(visibleClass) ? DefaultVisibleClass : visibleClass;
        public int CloseDurationMs => Mathf.Max(0, closeDurationMs);
        public bool SetDisplayNoneAfterClose => setDisplayNoneAfterClose;

        public static PanelTransitionProfileSO Load(PanelTransitionType transitionType)
        {
            PanelTransitionProfileSO profile = Resources.Load<PanelTransitionProfileSO>($"{ResourcesProfilesPath}/{transitionType}");
            if (profile == null)
                throw new InvalidOperationException($"No panel transition profile was found for {transitionType} at Resources/{ResourcesProfilesPath}.");

            return profile;
        }

        public bool InstallStyleSheet(VisualElement target)
        {
            if (target == null || styleSheet == null || HasStyleSheet(target, styleSheet))
                return false;

            target.styleSheets.Add(styleSheet);
            return true;
        }

        public void UninstallStyleSheet(VisualElement target)
        {
            if (target != null && styleSheet != null)
                target.styleSheets.Remove(styleSheet);
        }

        private static bool HasStyleSheet(VisualElement target, StyleSheet candidate)
        {
            for (int i = 0; i < target.styleSheets.count; i++)
            {
                if (target.styleSheets[i] == candidate)
                    return true;
            }

            return false;
        }
    }
}
