using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Minigame.Runtime.Presentation.UI
{
    [UxmlElement("TimerGradientSlider")]
    public partial class TimerGradientSlider : VisualElement
    {
        public static readonly BindingId ValueProperty = nameof(Value);
        public static readonly BindingId MaxValueProperty = nameof(MaxValue);

        public const string RootClass = "timer-gradient-slider";
        public const string TrackClass = "timer-gradient-slider__track";
        public const string FillClass = "timer-gradient-slider__fill";
        public const string LabelClass = "timer-gradient-slider__label";

        private readonly VisualElement track;
        private readonly VisualElement fill;
        private readonly Label valueLabel;

        private float value;
        private float maxValue = 1f;

        private bool showText = true;
        private bool invertValue;

        private Color fullColor = new(0.2f, 0.85f, 0.35f);
        private Color middleColor = new(1f, 0.75f, 0.15f);
        private Color emptyColor = new(1f, 0.2f, 0.15f);

        [UxmlAttribute("value")]
        [CreateProperty]
        public float Value
        {
            get => value;
            set
            {
                this.value = value;
                NotifyPropertyChanged(ValueProperty);
                RefreshVisuals();
            }
        }

        [UxmlAttribute("max-value")]
        [CreateProperty]
        public float MaxValue
        {
            get => maxValue;
            set
            {
                maxValue = Mathf.Max(0.001f, value);
                NotifyPropertyChanged(MaxValueProperty);
                RefreshVisuals();
            }
        }

        [UxmlAttribute("show-text")]
        [CreateProperty]
        public bool ShowText
        {
            get => showText;
            set
            {
                showText = value;
                RefreshVisuals();
            }
        }

        [UxmlAttribute("invert-value")]
        [CreateProperty]
        public bool InvertValue
        {
            get => invertValue;
            set
            {
                invertValue = value;
                RefreshVisuals();
            }
        }

        [UxmlAttribute("full-color")]
        [CreateProperty]
        public Color FullColor
        {
            get => fullColor;
            set
            {
                fullColor = value;
                RefreshVisuals();
            }
        }

        [UxmlAttribute("middle-color")]
        [CreateProperty]
        public Color MiddleColor
        {
            get => middleColor;
            set
            {
                middleColor = value;
                RefreshVisuals();
            }
        }

        [UxmlAttribute("empty-color")]
        [CreateProperty]
        public Color EmptyColor
        {
            get => emptyColor;
            set
            {
                emptyColor = value;
                RefreshVisuals();
            }
        }

        public TimerGradientSlider()
        {
            AddToClassList(RootClass);

            track = new VisualElement { name = "Track" };
            track.AddToClassList(TrackClass);

            fill = new VisualElement { name = "Fill" };
            fill.AddToClassList(FillClass);

            valueLabel = new Label { name = "ValueLabel" };
            valueLabel.AddToClassList(LabelClass);

            track.Add(fill);

            Add(track);
            Add(valueLabel);

            RefreshVisuals();
        }

        public void SetTimer(float currentValue, float maxValue)
        {
            this.maxValue = Mathf.Max(0.001f, maxValue);
            value = currentValue;

            RefreshVisuals();
        }

        public void SetValue(float currentValue)
        {
            value = currentValue;
            RefreshVisuals();
        }

        public void SetMaxValue(float maxValue)
        {
            this.maxValue = Mathf.Max(0.001f, maxValue);
            RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            if (fill == null || valueLabel == null)
                return;

            float clampedValue = Mathf.Clamp(value, 0f, maxValue);
            float normalized = Mathf.Clamp01(clampedValue / maxValue);
            float visualValue = invertValue ? 1f - normalized : normalized;

            fill.style.width = Length.Percent(visualValue * 100f);

            Color color = EvaluateColor(visualValue);
            fill.style.backgroundColor = color;

            valueLabel.style.display = showText ? DisplayStyle.Flex : DisplayStyle.None;

            if (showText)
                valueLabel.text = $"{Mathf.CeilToInt(clampedValue)} / {Mathf.CeilToInt(maxValue)}";
        }

        private Color EvaluateColor(float normalizedValue)
        {
            if (normalizedValue <= 0.5f)
            {
                float t = normalizedValue / 0.5f;
                return Color.Lerp(emptyColor, middleColor, t);
            }

            float upperT = (normalizedValue - 0.5f) / 0.5f;
            return Color.Lerp(middleColor, fullColor, upperT);
        }
    }
}