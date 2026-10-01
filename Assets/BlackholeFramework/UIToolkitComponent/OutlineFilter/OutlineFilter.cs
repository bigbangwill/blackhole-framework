using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BigBang.UIToolkitComponent
{
    public static class ImageOutlineFilter
    {
        public const float MaxOutlineWidth = 16f;
        public const float MinOutlineResolution = 1f;
        public const float MaxOutlineResolution = 6f;

        private const string DefinitionResourceName = "UIOutlineFilter";

        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineResolutionId = Shader.PropertyToID("_OutlineResolution");
        private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");

        private static FilterFunctionDefinition definition;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void ScheduleInitializeInEditor()
        {
            UnityEditor.EditorApplication.delayCall += () => TryInitialize(false);
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeAtRuntime()
        {
            TryInitialize();
        }

        public static void SetImageOutline(this VisualElement element, Color color, float width = 2f, float resolution = 4f)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (width <= 0f || color.a <= 0f)
            {
                element.ClearImageOutline();
                return;
            }

            if (!TryInitialize())
                return;

            FilterFunction outline = new(definition);
            outline.AddParameter(new FilterParameter(Mathf.Clamp(width, 0f, MaxOutlineWidth)));
            outline.AddParameter(new FilterParameter(color));
            outline.AddParameter(new FilterParameter(Mathf.Clamp(resolution, MinOutlineResolution, MaxOutlineResolution)));

            List<FilterFunction> filters = GetFiltersWithoutOutline(element);
            filters.Add(outline);
            element.style.filter = filters;
        }

        public static void ClearImageOutline(this VisualElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            List<FilterFunction> filters = GetFiltersWithoutOutline(element);
            element.style.filter = filters.Count == 0 ? new StyleList<FilterFunction>(StyleKeyword.None) : new StyleList<FilterFunction>(filters);
        }

        private static List<FilterFunction> GetFiltersWithoutOutline(VisualElement element)
        {
            List<FilterFunction> filters = new();

            foreach (FilterFunction filter in element.resolvedStyle.filter)
            {
                bool isImageOutline = filter.type == FilterFunctionType.Custom && filter.customDefinition == definition;
                if (!isImageOutline)
                    filters.Add(filter);
            }

            return filters;
        }

        private static bool TryInitialize(bool logErrors = true)
        {
            if (definition != null)
                return true;

            definition = Resources.Load<FilterFunctionDefinition>(DefinitionResourceName);
            if (definition == null)
            {
                if (logErrors)
                    Debug.LogError($"Could not load Resources/{DefinitionResourceName}.asset.");

                return false;
            }

            if (!InitializeDefinition(definition))
            {
                if (logErrors)
                    Debug.LogError("A UI Toolkit outline filter definition has no valid material pass.");

                definition = null;
                return false;
            }

            return true;
        }

        private static bool InitializeDefinition(FilterFunctionDefinition filterDefinition)
        {
            PostProcessingPass[] passes = filterDefinition.passes;
            if (passes == null || passes.Length == 0 || passes[0].material == null)
                return false;

            PostProcessingPass pass = passes[0];
            pass.computeRequiredReadMarginsCallback = ComputeMargins;
            pass.computeRequiredWriteMarginsCallback = ComputeMargins;
            pass.applySettingsCallback = ApplySettings;
            passes[0] = pass;
            filterDefinition.passes = passes;
            return true;
        }

        private static PostProcessingMargins ComputeMargins(FilterFunction filter)
        {
            float margin = Mathf.Ceil(Mathf.Clamp(filter.GetParameter(0).floatValue, 0f, MaxOutlineWidth)) + 1f;
            return new PostProcessingMargins { left = margin, top = margin, right = margin, bottom = margin };
        }

        private static void ApplySettings(MaterialPropertyBlock properties, FilterPassContext context)
        {
            float width = Mathf.Clamp(context.filterFunction.GetParameter(0).floatValue, 0f, MaxOutlineWidth);
            Color color = context.filterFunction.GetParameter(1).colorValue;
            float resolution = Mathf.Clamp(context.filterFunction.GetParameter(2).floatValue, MinOutlineResolution, MaxOutlineResolution);

            if (!context.readsGamma)
                color = color.linear;

            properties.SetFloat(OutlineWidthId, width * context.scaledPixelsPerPoint);
            properties.SetColor(OutlineColorId, color);
            properties.SetFloat(OutlineResolutionId, resolution);
        }
    }
}
