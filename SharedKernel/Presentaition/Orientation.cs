using UnityEngine;
using UnityEngine.UIElements;

namespace Blackhole.SharedKernel.Presentaion
{
    [RequireComponent(typeof(PanelRenderer))]
    public abstract class Orientation : MonoBehaviour
    {
        private const string LandscapeClassName = "ori-landscape";
        private const string PortraitClassName = "ori-portrait";

        [SerializeField] private string orientationRootName = "Root";

        private PanelRenderer _panelRenderer;
        public VisualElement DocumentRoot => orientationRoot;
        private VisualElement _documentRoot;
        private VisualElement orientationRoot;

        protected bool isLandscape;
        protected virtual void Awake()
        {
            _panelRenderer = GetComponent<PanelRenderer>();
            _panelRenderer.RegisterUIReloadCallback(OnUIReloaded);
        }

        private void OnDisable()
        {
            _panelRenderer.UnregisterUIReloadCallback(OnUIReloaded);
            UnbindOrientationRoot();
        }

        protected virtual void OnUIReloaded(PanelRenderer panelRenderer, VisualElement root)
        {
            _documentRoot = root;
            BindOrientationRoot();
        }

        protected virtual void OnDestroy()
        {
            UnbindOrientationRoot();
        }

        private void BindOrientationRoot()
        {
            if (_documentRoot == null)
                return;

            orientationRoot = ResolveOrientationRoot();
            orientationRoot.RegisterCallback<GeometryChangedEvent>(OnOrientationRootGeometryChanged);
            orientationRoot.schedule.Execute(UpdateOrientationClass).ExecuteLater(0);
            UpdateOrientationClass();
        }

        private void UnbindOrientationRoot()
        {
            if (orientationRoot != null)
                orientationRoot.UnregisterCallback<GeometryChangedEvent>(OnOrientationRootGeometryChanged);

            orientationRoot = null;
        }

        private VisualElement ResolveOrientationRoot()
        {
            if (!string.IsNullOrWhiteSpace(orientationRootName))
            {
                VisualElement namedRoot = _documentRoot.Q<VisualElement>(orientationRootName);

                if (namedRoot != null)
                    return namedRoot;
            }

            return _documentRoot;
        }

        protected virtual void OnOrientationRootGeometryChanged(GeometryChangedEvent evt)
        {
            ApplyOrientationClass(evt.newRect.width, evt.newRect.height);
        }

        private void UpdateOrientationClass()
        {
            if (orientationRoot == null)
                return;

            ApplyOrientationClass(
                orientationRoot.resolvedStyle.width,
                orientationRoot.resolvedStyle.height);
        }

        private void ApplyOrientationClass(float width, float height)
        {
            if (orientationRoot == null || !IsValidDimension(width) || !IsValidDimension(height))
                return;

            bool landscape = width >= height;
            isLandscape = landscape;
            string activeClass = landscape ? LandscapeClassName : PortraitClassName;
            string inactiveClass = landscape ? PortraitClassName : LandscapeClassName;

            orientationRoot.RemoveFromClassList(inactiveClass);
            orientationRoot.AddToClassList(activeClass);
        }

        private static bool IsValidDimension(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
        }
    }
}