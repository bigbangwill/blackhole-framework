using UnityEngine;
using UnityEngine.UIElements;

namespace BigBang.UIToolkitComponent
{
    public enum BigBangScrollAxis
    {
        Vertical,
        Horizontal
    }

    [UxmlElement("BigBangScrollView")]
    public partial class BigBangScrollView : VisualElement
    {
        public const string ussClassName = "bigbang-scroll-view";
        public const string viewportUssClassName = "bigbang-scroll-view__viewport";
        public const string contentUssClassName = "bigbang-scroll-view__content";
        public const string scrollbarUssClassName = "bigbang-scroll-view__scrollbar";
        public const string thumbUssClassName = "bigbang-scroll-view__thumb";

        public const string verticalUssClassName = "bigbang-scroll-view--vertical";
        public const string horizontalUssClassName = "bigbang-scroll-view--horizontal";
        public const string hiddenScrollbarUssClassName = "bigbang-scroll-view--hidden-scrollbar";
        public const string startFromBottomUssClassName = "bigbang-scroll-view--start-from-bottom";

        private const string defaultStyleSheetResourcePath = "BigBangScrollViewUss";

        private static StyleSheet cachedDefaultStyleSheet;

        private readonly VisualElement viewport;
        private readonly VisualElement content;
        private readonly VisualElement scrollbar;
        private readonly VisualElement thumb;

        private BigBangScrollAxis axis = BigBangScrollAxis.Vertical;

        private bool showScrollbar = true;
        private bool enableDragScroll = true;
        private bool wrapContent;
        private bool startFromBottom;
        private bool pendingStartFromBottom;

        private float contentPadding = 0f;
        private float contentGap = 8f;
        private float mouseWheelSpeed = 48f;

        private Color backgroundColor = new Color32(0, 0, 0, 0);
        private Color contentBackgroundColor = new Color32(0, 0, 0, 0);
        private Color scrollbarColor = new Color32(255, 255, 255, 24);
        private Color thumbColor = new Color32(255, 255, 255, 95);

        private Vector2 scrollOffset;

        private bool pointerPressed;
        private bool isDraggingContent;
        private bool suppressClickAfterDrag;
        private int activePointerId = -1;

        private Vector2 pointerStartPosition;
        private Vector2 lastPointerPosition;

        private bool isDraggingScrollbar;
        private int scrollbarPointerId = -1;

        private const float dragThreshold = 6f;
        private const float minThumbLength = 28f;

        private bool enableInertia = true;

        private float dragScrollSensitivity = 1.45f;
        private float inertiaDeceleration = 5200f;
        private float inertiaStopSpeed = 25f;
        private float maxInertiaVelocity = 5200f;

        private float axisVelocity;
        private float lastPointerTime;
        private float lastInertiaTime;

        private IVisualElementScheduledItem inertiaItem;

        public override VisualElement contentContainer => content;

        [UxmlAttribute("axis")]
        public BigBangScrollAxis Axis
        {
            get => axis;
            set
            {
                axis = value;
                ApplyAxis();
                ApplyContentLayout();
                ClampAndApplyScroll();
            }
        }

        [UxmlAttribute("show-scrollbar")]
        public bool ShowScrollbar
        {
            get => showScrollbar;
            set
            {
                showScrollbar = value;
                ApplyScrollbarVisibility();
                UpdateScrollbarVisual();
            }
        }

        [UxmlAttribute("enable-drag-scroll")]
        public bool EnableDragScroll
        {
            get => enableDragScroll;
            set => enableDragScroll = value;
        }

        [UxmlAttribute("wrap-content")]
        public bool WrapContent
        {
            get => wrapContent;
            set
            {
                wrapContent = value;
                ApplyContentLayout();
                ClampAndApplyScroll();
            }
        }

        [UxmlAttribute("start-from-bottom")]
        public bool StartFromBottom
        {
            get => startFromBottom;
            set
            {
                startFromBottom = value;
                pendingStartFromBottom = value;
                ApplyStartFromBottomLayout();

                if (value)
                    schedule.Execute(RefreshScrollAfterGeometryChange);
            }
        }

        [UxmlAttribute("content-padding")]
        public float ContentPadding
        {
            get => contentPadding;
            set
            {
                contentPadding = Mathf.Max(0f, value);
                ApplyContentPadding();
                ClampAndApplyScroll();
            }
        }

        [UxmlAttribute("content-gap")]
        public float ContentGap
        {
            get => contentGap;
            set
            {
                contentGap = Mathf.Max(0f, value);
                ApplyChildSpacing();
                ClampAndApplyScroll();
            }
        }

        [UxmlAttribute("mouse-wheel-speed")]
        public float MouseWheelSpeed
        {
            get => mouseWheelSpeed;
            set => mouseWheelSpeed = Mathf.Max(1f, value);
        }

        [UxmlAttribute("background-color")]
        public Color BackgroundColor
        {
            get => backgroundColor;
            set
            {
                backgroundColor = value;
                style.backgroundColor = backgroundColor;
            }
        }

        [UxmlAttribute("content-background-color")]
        public Color ContentBackgroundColor
        {
            get => contentBackgroundColor;
            set
            {
                contentBackgroundColor = value;
                content.style.backgroundColor = contentBackgroundColor;
            }
        }

        [UxmlAttribute("scrollbar-color")]
        public Color ScrollbarColor
        {
            get => scrollbarColor;
            set
            {
                scrollbarColor = value;
                scrollbar.style.backgroundColor = scrollbarColor;
            }
        }

        [UxmlAttribute("thumb-color")]
        public Color ThumbColor
        {
            get => thumbColor;
            set
            {
                thumbColor = value;
                thumb.style.backgroundColor = thumbColor;
            }
        }

        [UxmlAttribute("drag-scroll-sensitivity")]
        public float DragScrollSensitivity
        {
            get => dragScrollSensitivity;
            set => dragScrollSensitivity = Mathf.Max(0.1f, value);
        }

        [UxmlAttribute("enable-inertia")]
        public bool EnableInertia
        {
            get => enableInertia;
            set => enableInertia = value;
        }

        [UxmlAttribute("inertia-deceleration")]
        public float InertiaDeceleration
        {
            get => inertiaDeceleration;
            set => inertiaDeceleration = Mathf.Max(100f, value);
        }

        [UxmlAttribute("max-inertia-velocity")]
        public float MaxInertiaVelocity
        {
            get => maxInertiaVelocity;
            set => maxInertiaVelocity = Mathf.Max(100f, value);
        }

        public BigBangScrollView()
        {
            LoadDefaultStyleSheet();

            AddToClassList(ussClassName);

            viewport = new VisualElement { name = "viewport" };
            content = new VisualElement { name = "content" };
            scrollbar = new VisualElement { name = "scrollbar" };
            thumb = new VisualElement { name = "thumb" };

            viewport.AddToClassList(viewportUssClassName);
            content.AddToClassList(contentUssClassName);
            scrollbar.AddToClassList(scrollbarUssClassName);
            thumb.AddToClassList(thumbUssClassName);

            viewport.hierarchy.Add(content);
            scrollbar.hierarchy.Add(thumb);

            hierarchy.Add(viewport);
            hierarchy.Add(scrollbar);

            ApplyAllStyles();

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                pendingStartFromBottom = startFromBottom;
                ApplyAllStyles();
                schedule.Execute(RefreshScrollAfterGeometryChange);
            });
            RegisterCallback<GeometryChangedEvent>(_ => RefreshScrollAfterGeometryChange());

            viewport.RegisterCallback<GeometryChangedEvent>(_ => RefreshScrollAfterGeometryChange());
            content.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                ApplyChildSpacing();
                RefreshScrollAfterGeometryChange();
            });

            RegisterCallback<WheelEvent>(OnWheel, TrickleDown.TrickleDown);

            RegisterCallback<PointerDownEvent>(OnContentPointerDown, TrickleDown.TrickleDown);
            RegisterCallback<PointerMoveEvent>(OnContentPointerMove, TrickleDown.TrickleDown);
            RegisterCallback<PointerUpEvent>(OnContentPointerUp, TrickleDown.TrickleDown);
            RegisterCallback<PointerCancelEvent>(OnContentPointerCancel, TrickleDown.TrickleDown);
            RegisterCallback<ClickEvent>(OnContentClick, TrickleDown.TrickleDown);

            scrollbar.RegisterCallback<PointerDownEvent>(OnScrollbarPointerDown, TrickleDown.TrickleDown);
            scrollbar.RegisterCallback<PointerMoveEvent>(OnScrollbarPointerMove, TrickleDown.TrickleDown);
            scrollbar.RegisterCallback<PointerUpEvent>(OnScrollbarPointerUp, TrickleDown.TrickleDown);
            scrollbar.RegisterCallback<PointerCancelEvent>(OnScrollbarPointerCancel, TrickleDown.TrickleDown);
        }

        public void SetScrollOffset(Vector2 newOffset)
        {
            scrollOffset = ClampOffset(newOffset);
            ApplyScrollOffset();
            UpdateScrollbarVisual();
        }

        public Vector2 GetScrollOffset()
        {
            return scrollOffset;
        }

        public void ScrollToStart()
        {
            SetScrollOffset(Vector2.zero);
        }

        public void ScrollToEnd()
        {
            if (axis == BigBangScrollAxis.Vertical)
                SetScrollOffset(new Vector2(0f, GetMaxScrollOffset()));
            else
                SetScrollOffset(new Vector2(GetMaxScrollOffset(), 0f));
        }

        private void LoadDefaultStyleSheet()
        {
            cachedDefaultStyleSheet ??= Resources.Load<StyleSheet>(defaultStyleSheetResourcePath);

            if (cachedDefaultStyleSheet == null)
            {
                Debug.LogWarning(
                    $"BigBangScrollView default USS was not found. Expected path: Assets/Resources/{defaultStyleSheetResourcePath}.uss"
                );

                return;
            }

            styleSheets.Add(cachedDefaultStyleSheet);
        }

        private void ApplyAllStyles()
        {
            ApplyAxis();
            ApplyScrollbarVisibility();
            ApplyContentLayout();
            ApplyStartFromBottomLayout();
            ApplyContentPadding();
            ApplyChildSpacing();

            style.backgroundColor = backgroundColor;
            content.style.backgroundColor = contentBackgroundColor;
            scrollbar.style.backgroundColor = scrollbarColor;
            thumb.style.backgroundColor = thumbColor;

            ClampAndApplyScroll();
        }

        private void ApplyAxis()
        {
            RemoveFromClassList(verticalUssClassName);
            RemoveFromClassList(horizontalUssClassName);

            if (axis == BigBangScrollAxis.Vertical)
                AddToClassList(verticalUssClassName);
            else
                AddToClassList(horizontalUssClassName);
        }

        private void ApplyScrollbarVisibility()
        {
            if (showScrollbar)
            {
                RemoveFromClassList(hiddenScrollbarUssClassName);
                scrollbar.style.display = DisplayStyle.Flex;
            }
            else
            {
                AddToClassList(hiddenScrollbarUssClassName);
                scrollbar.style.display = DisplayStyle.None;
            }
        }

        private void ApplyStartFromBottomLayout()
        {
            EnableInClassList(startFromBottomUssClassName, startFromBottom);
        }

        private void ApplyContentLayout()
        {
            if (axis == BigBangScrollAxis.Horizontal)
            {
                content.style.flexDirection = FlexDirection.Row;
                content.style.flexWrap = Wrap.NoWrap;
            }
            else
            {
                content.style.flexDirection = wrapContent
                    ? FlexDirection.Row
                    : FlexDirection.Column;

                content.style.flexWrap = wrapContent
                    ? Wrap.Wrap
                    : Wrap.NoWrap;
            }

            ApplyChildSpacing();
        }

        private void ApplyContentPadding()
        {
            content.style.paddingLeft = contentPadding;
            content.style.paddingRight = contentPadding;
            content.style.paddingTop = contentPadding;
            content.style.paddingBottom = contentPadding;
        }

        private void ApplyChildSpacing()
        {
            int childCount = content.childCount;

            for (int i = 0; i < childCount; i++)
            {
                VisualElement child = content[i];

                child.style.marginLeft = 0;
                child.style.marginRight = 0;
                child.style.marginTop = 0;
                child.style.marginBottom = 0;

                bool isLastChild = i == childCount - 1;

                if (isLastChild)
                    continue;

                if (axis == BigBangScrollAxis.Horizontal)
                {
                    child.style.marginRight = contentGap;
                    continue;
                }

                if (wrapContent)
                {
                    child.style.marginRight = contentGap;
                    child.style.marginBottom = contentGap;
                }
                else
                {
                    child.style.marginBottom = contentGap;
                }
            }
        }

        private void OnWheel(WheelEvent evt)
        {
            float delta = evt.delta.y * mouseWheelSpeed;

            if (axis == BigBangScrollAxis.Vertical)
                SetScrollOffset(scrollOffset + new Vector2(0f, delta));
            else
                SetScrollOffset(scrollOffset + new Vector2(delta, 0f));

            evt.StopPropagation();
        }

        private void OnContentPointerDown(PointerDownEvent evt)
        {
            if (!enableDragScroll)
                return;

            if (evt.button != 0)
                return;

            suppressClickAfterDrag = false;

            if (IsPointerOnScrollbar(evt.position))
                return;

            StopInertia();

            pointerPressed = true;
            isDraggingContent = false;
            activePointerId = evt.pointerId;

            pointerStartPosition = evt.position;
            lastPointerPosition = evt.position;

            lastPointerTime = Time.unscaledTime;
            axisVelocity = 0f;
        }

        private void OnContentPointerMove(PointerMoveEvent evt)
        {
            if (!enableDragScroll)
                return;

            if (!pointerPressed)
                return;

            if (evt.pointerId != activePointerId)
                return;

            if ((evt.pressedButtons & 1) == 0)
            {
                ReleaseContentPointerIfNeeded(evt.pointerId);
                ResetContentDragState();
                StopInertia();
                return;
            }

            Vector2 currentPosition = evt.position;
            Vector2 totalDelta = currentPosition - pointerStartPosition;

            if (!isDraggingContent)
            {
                float axisDelta = axis == BigBangScrollAxis.Vertical
                    ? Mathf.Abs(totalDelta.y)
                    : Mathf.Abs(totalDelta.x);

                float crossAxisDelta = axis == BigBangScrollAxis.Vertical
                    ? Mathf.Abs(totalDelta.x)
                    : Mathf.Abs(totalDelta.y);

                if (axisDelta < dragThreshold || axisDelta < crossAxisDelta)
                    return;

                isDraggingContent = true;
                suppressClickAfterDrag = true;
                PointerCaptureHelper.CapturePointer(this, evt.pointerId);
            }

            Vector2 frameDelta = currentPosition - lastPointerPosition;

            float axisFrameDelta = axis == BigBangScrollAxis.Vertical
                ? frameDelta.y
                : frameDelta.x;

            float scrollDelta = -axisFrameDelta * dragScrollSensitivity;

            float now = Time.unscaledTime;
            float deltaTime = Mathf.Clamp(now - lastPointerTime, 0.001f, 0.05f);

            float instantVelocity = scrollDelta / deltaTime;

            axisVelocity = Mathf.Lerp(axisVelocity, instantVelocity, 0.65f);
            axisVelocity = Mathf.Clamp(axisVelocity, -maxInertiaVelocity, maxInertiaVelocity);

            ScrollByAxisDelta(scrollDelta);

            lastPointerPosition = currentPosition;
            lastPointerTime = now;

            evt.StopPropagation();
        }

        private void OnContentPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId != activePointerId)
                return;

            bool wasDragging = isDraggingContent;

            ReleaseContentPointerIfNeeded(evt.pointerId);
            ResetContentDragState();

            if (wasDragging)
            {
                StartInertia();
                schedule.Execute(() => suppressClickAfterDrag = false);
                evt.StopPropagation();
            }
        }

        private void OnContentPointerCancel(PointerCancelEvent evt)
        {
            if (evt.pointerId != activePointerId)
                return;

            ReleaseContentPointerIfNeeded(evt.pointerId);
            ResetContentDragState();
            StopInertia();
            suppressClickAfterDrag = false;

            evt.StopPropagation();
        }

        private void OnContentClick(ClickEvent evt)
        {
            if (!suppressClickAfterDrag)
                return;

            suppressClickAfterDrag = false;
            evt.PreventDefault();
            evt.StopImmediatePropagation();
        }

        private void OnScrollbarPointerDown(PointerDownEvent evt)
        {
            if (!showScrollbar)
                return;

            if (evt.button != 0)
                return;

            StopInertia();

            isDraggingScrollbar = true;
            scrollbarPointerId = evt.pointerId;

            PointerCaptureHelper.CapturePointer(scrollbar, evt.pointerId);
            SetOffsetFromScrollbarPointer(evt.position);

            evt.StopPropagation();
        }

        private void OnScrollbarPointerMove(PointerMoveEvent evt)
        {
            if (!isDraggingScrollbar)
                return;

            if (evt.pointerId != scrollbarPointerId)
                return;

            SetOffsetFromScrollbarPointer(evt.position);
            evt.StopPropagation();
        }

        private void OnScrollbarPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId != scrollbarPointerId)
                return;

            ReleaseScrollbarPointerIfNeeded(evt.pointerId);
            ResetScrollbarDragState();

            evt.StopPropagation();
        }

        private void OnScrollbarPointerCancel(PointerCancelEvent evt)
        {
            if (evt.pointerId != scrollbarPointerId)
                return;

            ReleaseScrollbarPointerIfNeeded(evt.pointerId);
            ResetScrollbarDragState();

            evt.StopPropagation();
        }

        private void SetOffsetFromScrollbarPointer(Vector2 panelPosition)
        {
            Vector2 local = scrollbar.WorldToLocal(panelPosition);

            float scrollbarLength = GetScrollbarLength();
            float thumbLength = GetThumbLength();
            float maxThumbTravel = Mathf.Max(0f, scrollbarLength - thumbLength);

            if (maxThumbTravel <= 0f)
                return;

            float pointerPosition = axis == BigBangScrollAxis.Vertical
                ? local.y - thumbLength * 0.5f
                : local.x - thumbLength * 0.5f;

            float normalized = Mathf.Clamp01(pointerPosition / maxThumbTravel);
            float maxOffset = GetMaxScrollOffset();

            if (axis == BigBangScrollAxis.Vertical)
                SetScrollOffset(new Vector2(0f, normalized * maxOffset));
            else
                SetScrollOffset(new Vector2(normalized * maxOffset, 0f));
        }

        private bool IsPointerOnScrollbar(Vector2 panelPosition)
        {
            if (!showScrollbar)
                return false;

            Vector2 local = scrollbar.WorldToLocal(panelPosition);
            Rect rect = new Rect(0f, 0f, scrollbar.resolvedStyle.width, scrollbar.resolvedStyle.height);

            return rect.Contains(local);
        }

        private void ReleaseContentPointerIfNeeded(int pointerId)
        {
            if (PointerCaptureHelper.HasPointerCapture(this, pointerId))
                PointerCaptureHelper.ReleasePointer(this, pointerId);
        }

        private void ReleaseScrollbarPointerIfNeeded(int pointerId)
        {
            if (PointerCaptureHelper.HasPointerCapture(scrollbar, pointerId))
                PointerCaptureHelper.ReleasePointer(scrollbar, pointerId);
        }

        private void ResetContentDragState()
        {
            pointerPressed = false;
            isDraggingContent = false;
            activePointerId = -1;
        }

        private void ResetScrollbarDragState()
        {
            isDraggingScrollbar = false;
            scrollbarPointerId = -1;
        }

        private void ClampAndApplyScroll()
        {
            scrollOffset = ClampOffset(scrollOffset);
            ApplyScrollOffset();
            UpdateScrollbarVisual();
        }

        private void RefreshScrollAfterGeometryChange()
        {
            if (TryApplyInitialBottomPosition())
                return;

            ClampAndApplyScroll();
        }

        private bool TryApplyInitialBottomPosition()
        {
            if (!startFromBottom || !pendingStartFromBottom || axis != BigBangScrollAxis.Vertical)
                return false;

            float maxOffset = GetMaxScrollOffset();

            if (maxOffset <= 0f)
                return false;

            pendingStartFromBottom = false;
            scrollOffset = new Vector2(0f, maxOffset);
            ApplyScrollOffset();
            UpdateScrollbarVisual();
            return true;
        }

        private Vector2 ClampOffset(Vector2 offset)
        {
            float maxOffset = GetMaxScrollOffset();

            if (axis == BigBangScrollAxis.Vertical)
            {
                return new Vector2(
                    0f,
                    Mathf.Clamp(offset.y, 0f, maxOffset)
                );
            }

            return new Vector2(
                Mathf.Clamp(offset.x, 0f, maxOffset),
                0f
            );
        }

        private void ApplyScrollOffset()
        {
            content.style.translate = new Translate(
                -scrollOffset.x,
                -scrollOffset.y,
                0f
            );
        }

        private void UpdateScrollbarVisual()
        {
            if (!showScrollbar)
                return;

            float viewportLength = GetViewportLength();
            float contentLength = GetContentLength();

            if (viewportLength <= 0f || contentLength <= viewportLength)
            {
                thumb.style.display = DisplayStyle.None;
                return;
            }

            thumb.style.display = DisplayStyle.Flex;

            float scrollbarLength = GetScrollbarLength();
            float thumbLength = GetThumbLength();
            float maxThumbTravel = Mathf.Max(0f, scrollbarLength - thumbLength);
            float maxOffset = GetMaxScrollOffset();

            float normalized = maxOffset <= 0f
                ? 0f
                : GetCurrentAxisOffset() / maxOffset;

            float thumbPosition = normalized * maxThumbTravel;

            if (axis == BigBangScrollAxis.Vertical)
            {
                thumb.style.height = thumbLength;
                thumb.style.width = Length.Percent(100);
                thumb.style.top = thumbPosition;
                thumb.style.left = 0;
            }
            else
            {
                thumb.style.width = thumbLength;
                thumb.style.height = Length.Percent(100);
                thumb.style.left = thumbPosition;
                thumb.style.top = 0;
            }
        }

        private float GetCurrentAxisOffset()
        {
            return axis == BigBangScrollAxis.Vertical
                ? scrollOffset.y
                : scrollOffset.x;
        }

        private float GetMaxScrollOffset()
        {
            float viewportLength = GetViewportLength();
            float contentLength = GetContentLength();

            return Mathf.Max(0f, contentLength - viewportLength);
        }

        private float GetViewportLength()
        {
            float value = axis == BigBangScrollAxis.Vertical
                ? viewport.resolvedStyle.height
                : viewport.resolvedStyle.width;

            if (float.IsNaN(value) || value <= 0f)
                return 0f;

            return value;
        }

        private float GetContentLength()
        {
            float max = 0f;
            int childCount = content.childCount;

            for (int i = 0; i < childCount; i++)
            {
                VisualElement child = content[i];

                if (axis == BigBangScrollAxis.Vertical)
                {
                    max = Mathf.Max(
                        max,
                        child.layout.yMax + child.resolvedStyle.marginBottom
                    );
                }
                else
                {
                    max = Mathf.Max(
                        max,
                        child.layout.xMax + child.resolvedStyle.marginRight
                    );
                }
            }

            if (axis == BigBangScrollAxis.Vertical)
                max += content.resolvedStyle.paddingBottom;
            else
                max += content.resolvedStyle.paddingRight;

            if (float.IsNaN(max) || max <= 0f)
                return 0f;

            return max;
        }

        private float GetScrollbarLength()
        {
            float value = axis == BigBangScrollAxis.Vertical
                ? scrollbar.resolvedStyle.height
                : scrollbar.resolvedStyle.width;

            if (float.IsNaN(value) || value <= 0f)
                return 0f;

            return value;
        }

        private float GetThumbLength()
        {
            float viewportLength = GetViewportLength();
            float contentLength = GetContentLength();
            float scrollbarLength = GetScrollbarLength();

            if (viewportLength <= 0f || contentLength <= 0f || scrollbarLength <= 0f)
                return minThumbLength;

            float ratio = Mathf.Clamp01(viewportLength / contentLength);
            return Mathf.Clamp(scrollbarLength * ratio, minThumbLength, scrollbarLength);
        }

        private void ScrollByAxisDelta(float delta)
        {
            if (axis == BigBangScrollAxis.Vertical)
            {
                SetScrollOffset(scrollOffset + new Vector2(0f, delta));
            }
            else
            {
                SetScrollOffset(scrollOffset + new Vector2(delta, 0f));
            }
        }

        private void StartInertia()
        {
            if (!enableInertia)
            {
                StopInertia();
                return;
            }

            if (Mathf.Abs(axisVelocity) < inertiaStopSpeed)
            {
                StopInertia();
                return;
            }

            axisVelocity = Mathf.Clamp(axisVelocity, -maxInertiaVelocity, maxInertiaVelocity);
            lastInertiaTime = Time.unscaledTime;

            inertiaItem?.Pause();
            inertiaItem = schedule.Execute(UpdateInertia).Every(16);
        }

        private void StopInertia()
        {
            inertiaItem?.Pause();
            inertiaItem = null;
            axisVelocity = 0f;
        }

        private void UpdateInertia()
        {
            float now = Time.unscaledTime;
            float deltaTime = Mathf.Clamp(now - lastInertiaTime, 0.001f, 0.05f);

            lastInertiaTime = now;

            float previousOffset = GetCurrentAxisOffset();

            ScrollByAxisDelta(axisVelocity * deltaTime);

            float currentOffset = GetCurrentAxisOffset();

            bool hitBoundary = Mathf.Approximately(previousOffset, currentOffset);

            if (hitBoundary)
            {
                StopInertia();
                return;
            }

            axisVelocity = Mathf.MoveTowards(
                axisVelocity,
                0f,
                inertiaDeceleration * deltaTime
            );

            if (Mathf.Abs(axisVelocity) < inertiaStopSpeed)
            {
                StopInertia();
            }
        }
    }
}
