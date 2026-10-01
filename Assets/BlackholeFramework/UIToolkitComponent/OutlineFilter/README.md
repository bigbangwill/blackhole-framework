# UI Toolkit image outline filter

`UIOutlineFilter` is a custom UI Toolkit filter that applies an outline directly to one standard `VisualElement`. Transparent PNGs and SVG images are outlined from their rendered alpha. It does not add or duplicate visual elements.

## USS and UXML

```xml
<ui:VisualElement class="outlined-image">
    <ui:Image source="project://database/Assets/Images/Icon.svg" />
</ui:VisualElement>
```

```css
.outlined-image {
    filter: filter("/Assets/Packages/UIToolkitComponent/OutlineFilter/Resources/UIOutlineFilter.asset" 4px #ffcc00 4);
}
```

In UI Builder, select the element, open **Filter**, add a **Custom** function, and choose `UIOutlineFilter.asset`. The three parameters are **Width**, **Color**, and **Resolution**. Resolution accepts `1` through `6`; `4` is the high-quality default and `6` is the maximum-quality setting.

## C#

The same filter can be applied to any existing parent element:

```csharp
parent.SetImageOutline(new Color32(255, 204, 0, 255), 4f, 4f);
parent.ClearImageOutline();
```

Outline width is measured in UI points and clamped to 16. At resolution `4`, the shader uses 40 directions and 16 radial rings. At resolution `6`, it uses 56 directions and 24 rings. High settings intentionally prioritize smooth curved and diagonal edges over GPU cost.

Keep the filtering parent itself transparent. An opaque background on the parent becomes part of the captured alpha and the filter outlines that rectangle instead of the image silhouette. Multiple children are treated as one composited shape.
