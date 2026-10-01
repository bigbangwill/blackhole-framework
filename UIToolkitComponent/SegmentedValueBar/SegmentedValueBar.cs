using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace BigBang.UIToolkitComponent
{
    [UxmlElement("SegmentedValueBar")]
    public partial class SegmentedValueBar : VisualElement
    {
        public static readonly BindingId ValueProperty = nameof(Value);
        public static readonly BindingId MaxValueProperty = nameof(MaxValue);
        public static readonly BindingId SegmentCountProperty = nameof(SegmentCount);

        public const string RootClass = "segmented-value-bar";
        public const string SegmentClass = "segmented-value-bar__segment";
        public const string FilledSegmentClass = "segmented-value-bar__segment--filled";

        private const string defaultStyleSheetResourcePath = "SegmentedValueBarUSS";

        private static StyleSheet cachedDefaultStyleSheet;

        private readonly List<VisualElement> segments = new();

        private float value;
        private float maxValue = 100f;
        private int segmentCount = 10;
        private Color filledColor = new(0.04f, 0.67f, 0.57f, 1f);
        private Color emptyColor = new(0.46f, 0.49f, 0.5f, 0.78f);

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

        [UxmlAttribute("segment-count")]
        [CreateProperty]
        public int SegmentCount
        {
            get => segmentCount;
            set
            {
                segmentCount = Mathf.Max(1, value);
                NotifyPropertyChanged(SegmentCountProperty);
                RebuildSegments();
                RefreshVisuals();
            }
        }

        [UxmlAttribute("filled-color")]
        [CreateProperty]
        public Color FilledColor
        {
            get => filledColor;
            set
            {
                filledColor = value;
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

        public SegmentedValueBar()
        {
            LoadDefaultStyleSheet();
            AddToClassList(RootClass);

            pickingMode = PickingMode.Ignore;

            RebuildSegments();
            RefreshVisuals();
        }

        public void SetValue(float currentValue, float maximumValue)
        {
            maxValue = Mathf.Max(0.001f, maximumValue);
            value = currentValue;

            RefreshVisuals();
        }

        public void SetNormalizedValue(float normalizedValue)
        {
            value = Mathf.Clamp01(normalizedValue) * maxValue;
            RefreshVisuals();
        }

        private void LoadDefaultStyleSheet()
        {
            cachedDefaultStyleSheet ??= Resources.Load<StyleSheet>(defaultStyleSheetResourcePath);

            if (cachedDefaultStyleSheet == null)
            {
                Debug.LogWarning(
                    $"SegmentedValueBar default USS was not found. Expected path: Assets/Resources/{defaultStyleSheetResourcePath}.uss"
                );

                return;
            }

            styleSheets.Add(cachedDefaultStyleSheet);
        }

        private void RebuildSegments()
        {
            Clear();
            segments.Clear();

            for (int i = 0; i < segmentCount; i++)
            {
                VisualElement segment = new() { name = $"Segment{i + 1}" };
                segment.AddToClassList(SegmentClass);

                segments.Add(segment);
                Add(segment);
            }
        }

        private void RefreshVisuals()
        {
            if (segments.Count != segmentCount)
                RebuildSegments();

            float normalizedValue = Mathf.Clamp01(value / maxValue);
            int filledSegments = Mathf.Clamp(Mathf.CeilToInt(normalizedValue * segmentCount), 0, segmentCount);

            for (int i = 0; i < segments.Count; i++)
            {
                bool isFilled = i < filledSegments;
                VisualElement segment = segments[i];

                segment.EnableInClassList(FilledSegmentClass, isFilled);
                segment.style.backgroundColor = isFilled ? filledColor : emptyColor;
            }
        }
    }
}
