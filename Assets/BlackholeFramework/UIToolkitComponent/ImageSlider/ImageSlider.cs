using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace BigBang.UIToolkitComponent
{
    [UxmlElement]
    public partial class ImageSlider : BindableElement, INotifyValueChanged<float>
    {
        public const string ussClassName = "image-slider";
        public const string trackUssClassName = "image-slider__track";
        public const string fillUssClassName = "image-slider__fill";
        public const string fillImageUssClassName = "image-slider__fill-image";
        public const string handleUssClassName = "image-slider__handle";

        private const string defaultStyleSheetResourcePath = "ImageSliderUss";

        private static readonly CustomStyleProperty<Sprite> BackgroundSpriteProperty = new("--slider-background-sprite");
        private static readonly CustomStyleProperty<Sprite> FillSpriteProperty = new("--slider-fill-sprite");
        private static readonly CustomStyleProperty<Sprite> HandleSpriteProperty = new("--slider-handle-sprite");
        private static readonly CustomStyleProperty<Color> BackgroundColorProperty = new("--slider-background-color");
        private static readonly CustomStyleProperty<Color> FillColorProperty = new("--slider-fill-color");
        private static readonly CustomStyleProperty<Color> HandleColorProperty = new("--slider-handle-color");
        private static readonly CustomStyleProperty<float> FillBleedProperty = new("--slider-fill-bleed");

        private static StyleSheet cachedDefaultStyleSheet;

        private readonly VisualElement track;
        private readonly VisualElement fill;
        private readonly VisualElement fillImage;
        private readonly VisualElement handle;

        private float currentValue;
        private float currentLowValue = 0f;
        private float currentHighValue = 1f;
        private float currentStepSize = 0f;
        private bool currentWholeNumbers;

        private Sprite currentBackgroundSprite;
        private Sprite currentFillSprite;
        private Sprite currentHandleSprite;

        private Color currentBackgroundColor = new Color(0.22f, 0.22f, 0.22f, 1f);
        private Color currentFillColor = new Color(0.35f, 0.65f, 1f, 1f);
        private Color currentHandleColor = Color.white;
        private float currentFillBleed = 1f;

        [UxmlAttribute("value"), CreateProperty]
        public float value
        {
            get => currentValue;
            set => SetValue(value, true);
        }

        public void SetValueWithoutNotify(float newValue)
        {
            SetValue(newValue, false);
        }

        private void SetValue(float newValue, bool notify)
        {
            float previousValue = currentValue;
            float sanitizedValue = SanitizeValue(newValue);

            if (Mathf.Approximately(previousValue, sanitizedValue))
            {
                currentValue = sanitizedValue;
                RefreshVisuals();
                return;
            }

            currentValue = sanitizedValue;
            RefreshVisuals();

            if (!notify)
                return;

            using ChangeEvent<float> changeEvent = ChangeEvent<float>.GetPooled(previousValue, sanitizedValue);

            changeEvent.target = this;
            SendEvent(changeEvent);
        }

        [UxmlAttribute("low-value"), CreateProperty]
        public float lowValue
        {
            get => currentLowValue;
            set
            {
                currentLowValue = value;
                SetValueWithoutNotify(currentValue);
            }
        }

        [UxmlAttribute("high-value"), CreateProperty]
        public float highValue
        {
            get => currentHighValue;
            set
            {
                currentHighValue = value;
                SetValueWithoutNotify(currentValue);
            }
        }

        [UxmlAttribute("whole-numbers"), CreateProperty]
        public bool wholeNumbers
        {
            get => currentWholeNumbers;
            set
            {
                currentWholeNumbers = value;
                SetValueWithoutNotify(currentValue);
            }
        }

        [UxmlAttribute("step-size"), CreateProperty]
        public float stepSize
        {
            get => currentStepSize;
            set
            {
                currentStepSize = Mathf.Max(0f, value);
                SetValueWithoutNotify(currentValue);
            }
        }

        [UxmlAttribute("background-sprite")]
        public Sprite backgroundSprite
        {
            get => currentBackgroundSprite;
            set
            {
                currentBackgroundSprite = value;
                ApplyImages();
            }
        }

        [UxmlAttribute("fill-sprite")]
        public Sprite fillSprite
        {
            get => currentFillSprite;
            set
            {
                currentFillSprite = value;
                ApplyImages();
            }
        }

        [UxmlAttribute("handle-sprite")]
        public Sprite handleSprite
        {
            get => currentHandleSprite;
            set
            {
                currentHandleSprite = value;
                ApplyImages();
            }
        }

        [UxmlAttribute("background-color")]
        public Color backgroundColor
        {
            get => currentBackgroundColor;
            set
            {
                currentBackgroundColor = value;
                ApplyColors();
            }
        }

        [UxmlAttribute("fill-color")]
        public Color fillColor
        {
            get => currentFillColor;
            set
            {
                currentFillColor = value;
                ApplyColors();
            }
        }

        [UxmlAttribute("handle-color")]
        public Color handleColor
        {
            get => currentHandleColor;
            set
            {
                currentHandleColor = value;
                ApplyColors();
            }
        }

        public ImageSlider()
        {
            LoadDefaultStyleSheet();

            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;

            track = new VisualElement { name = "track" };
            fill = new VisualElement { name = "fill" };
            fillImage = new VisualElement { name = "fillImage" };
            handle = new VisualElement { name = "handle" };

            track.AddToClassList(trackUssClassName);
            fill.AddToClassList(fillUssClassName);
            fillImage.AddToClassList(fillImageUssClassName);
            handle.AddToClassList(handleUssClassName);

            track.pickingMode = PickingMode.Position;
            fill.pickingMode = PickingMode.Ignore;
            fillImage.pickingMode = PickingMode.Ignore;
            handle.pickingMode = PickingMode.Ignore;

            fill.Add(fillImage);
            track.Add(fill);
            Add(track);
            Add(handle);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

            ApplyImages();
            ApplyColors();
            RefreshVisuals();
        }

        private void LoadDefaultStyleSheet()
        {
            cachedDefaultStyleSheet ??= Resources.Load<StyleSheet>(defaultStyleSheetResourcePath);

            if (cachedDefaultStyleSheet == null)
            {
                Debug.LogWarning(
                    $"ImageSlider default USS was not found. Expected path: Assets/Resources/{defaultStyleSheetResourcePath}.uss"
                );

                return;
            }

            styleSheets.Add(cachedDefaultStyleSheet);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
                return;

            Focus();

            PointerCaptureHelper.CapturePointer(this, evt.pointerId);
            SetValueFromPointer(evt.position);

            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!PointerCaptureHelper.HasPointerCapture(this, evt.pointerId))
                return;

            SetValueFromPointer(evt.position);

            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!PointerCaptureHelper.HasPointerCapture(this, evt.pointerId))
                return;

            SetValueFromPointer(evt.position);
            PointerCaptureHelper.ReleasePointer(this, evt.pointerId);

            evt.StopPropagation();
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            if (PointerCaptureHelper.HasPointerCapture(this, evt.pointerId))
                PointerCaptureHelper.ReleasePointer(this, evt.pointerId);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            float range = Mathf.Abs(currentHighValue - currentLowValue);
            float delta = currentStepSize > 0f ? currentStepSize : range / 100f;

            if (currentWholeNumbers)
                delta = Mathf.Max(1f, Mathf.Round(delta));

            switch (evt.keyCode)
            {
                case KeyCode.LeftArrow:
                case KeyCode.DownArrow:
                    value -= delta;
                    evt.StopPropagation();
                    break;

                case KeyCode.RightArrow:
                case KeyCode.UpArrow:
                    value += delta;
                    evt.StopPropagation();
                    break;

                case KeyCode.Home:
                    value = GetMin();
                    evt.StopPropagation();
                    break;

                case KeyCode.End:
                    value = GetMax();
                    evt.StopPropagation();
                    break;
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            RefreshVisuals();
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            ICustomStyle customStyle = evt.customStyle;

            if (customStyle.TryGetValue(BackgroundSpriteProperty, out Sprite background))
                currentBackgroundSprite = background;

            if (customStyle.TryGetValue(FillSpriteProperty, out Sprite fillSprite))
                currentFillSprite = fillSprite;

            if (customStyle.TryGetValue(HandleSpriteProperty, out Sprite handleSprite))
                currentHandleSprite = handleSprite;

            if (customStyle.TryGetValue(BackgroundColorProperty, out Color backgroundColor))
                currentBackgroundColor = backgroundColor;

            if (customStyle.TryGetValue(FillColorProperty, out Color fillColor))
                currentFillColor = fillColor;

            if (customStyle.TryGetValue(HandleColorProperty, out Color handleColor))
                currentHandleColor = handleColor;

            if (customStyle.TryGetValue(FillBleedProperty, out float fillBleed))
                currentFillBleed = Mathf.Max(0f, fillBleed);

            ApplyImages();
            ApplyColors();
            RefreshVisuals();
        }

        private void SetValueFromPointer(Vector3 panelPosition)
        {
            float width = track.resolvedStyle.width;

            if (float.IsNaN(width) || width <= 0f)
                return;

            Vector2 localPosition = track.WorldToLocal(new Vector2(panelPosition.x, panelPosition.y));
            float normalized = Mathf.Clamp01(localPosition.x / width);

            float newValue = Mathf.Lerp(GetMin(), GetMax(), normalized);

            SetValue(newValue, true);
        }

        private void RefreshVisuals()
        {
            float normalized = GetNormalizedValue();
            SyncTrackRadius();

            float trackWidth = track.resolvedStyle.width;
            float handleWidth = handle.resolvedStyle.width;

            if (float.IsNaN(trackWidth) || trackWidth <= 0f)
                return;

            float fillWidth = trackWidth * normalized;
            float fillBleed = normalized > 0f ? currentFillBleed : 0f;

            fill.style.left = -fillBleed;
            fill.style.top = -fillBleed;
            fill.style.bottom = -fillBleed;
            fill.style.width = fillWidth + fillBleed * 2f;

            fillImage.style.left = fillBleed;
            fillImage.style.top = fillBleed;
            fillImage.style.bottom = fillBleed;
            fillImage.style.width = trackWidth;

            if (float.IsNaN(handleWidth) || handleWidth <= 0f)
                handleWidth = 0f;

            handle.style.left = trackWidth * normalized - handleWidth * 0.5f;
        }

        private void SyncTrackRadius()
        {
            track.style.borderTopLeftRadius = GetInnerRadius(
                resolvedStyle.borderTopLeftRadius,
                resolvedStyle.borderTopWidth,
                resolvedStyle.borderLeftWidth);

            track.style.borderTopRightRadius = GetInnerRadius(
                resolvedStyle.borderTopRightRadius,
                resolvedStyle.borderTopWidth,
                resolvedStyle.borderRightWidth);

            track.style.borderBottomRightRadius = GetInnerRadius(
                resolvedStyle.borderBottomRightRadius,
                resolvedStyle.borderBottomWidth,
                resolvedStyle.borderRightWidth);

            track.style.borderBottomLeftRadius = GetInnerRadius(
                resolvedStyle.borderBottomLeftRadius,
                resolvedStyle.borderBottomWidth,
                resolvedStyle.borderLeftWidth);
        }

        private static StyleLength GetInnerRadius(StyleLength outerRadius, float borderA, float borderB)
        {
            float radius = Mathf.Max(0f, outerRadius.value.value - Mathf.Max(borderA, borderB));
            return new StyleLength(new Length(radius, LengthUnit.Pixel));
        }

        private float GetNormalizedValue()
        {
            float min = GetMin();
            float max = GetMax();

            if (Mathf.Approximately(min, max))
                return 0f;

            return Mathf.Clamp01(Mathf.InverseLerp(min, max, currentValue));
        }

        private float SanitizeValue(float rawValue)
        {
            float min = GetMin();
            float max = GetMax();

            float result = Mathf.Clamp(rawValue, min, max);

            if (currentStepSize > 0f)
            {
                result = min + Mathf.Round((result - min) / currentStepSize) * currentStepSize;
            }

            if (currentWholeNumbers)
            {
                result = Mathf.Round(result);
            }

            return Mathf.Clamp(result, min, max);
        }

        private float GetMin()
        {
            return Mathf.Min(currentLowValue, currentHighValue);
        }

        private float GetMax()
        {
            return Mathf.Max(currentLowValue, currentHighValue);
        }

        private void ApplyImages()
        {
            ApplyBackground(track, currentBackgroundSprite);
            ApplyBackground(fillImage, currentFillSprite);
            ApplyBackground(handle, currentHandleSprite);
        }

        private void ApplyColors()
        {
            track.style.backgroundColor = currentBackgroundColor;
            fill.style.backgroundColor = Color.clear;
            fillImage.style.backgroundColor = currentFillColor;
            handle.style.backgroundColor = currentHandleColor;
        }

        private static void ApplyBackground(VisualElement element, Sprite sprite)
        {
            if (sprite != null)
            {
                element.style.backgroundImage = new StyleBackground(sprite);
                ApplySpriteSlices(element, sprite);
                return;
            }

            element.style.backgroundImage = StyleKeyword.None;
            ClearSpriteSlices(element);
        }

        private static void ApplySpriteSlices(VisualElement element, Sprite sprite)
        {
            Vector4 border = sprite.border;

            element.style.unitySliceLeft = Mathf.RoundToInt(border.x);
            element.style.unitySliceBottom = Mathf.RoundToInt(border.y);
            element.style.unitySliceRight = Mathf.RoundToInt(border.z);
            element.style.unitySliceTop = Mathf.RoundToInt(border.w);
        }

        private static void ClearSpriteSlices(VisualElement element)
        {
            element.style.unitySliceLeft = StyleKeyword.Null;
            element.style.unitySliceBottom = StyleKeyword.Null;
            element.style.unitySliceRight = StyleKeyword.Null;
            element.style.unitySliceTop = StyleKeyword.Null;
        }
    }
}
