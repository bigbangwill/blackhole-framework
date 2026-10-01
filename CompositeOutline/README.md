# Composite Outline

`CompositeSpriteOutline` renders the alpha union of multiple animated `SpriteRenderer` components and draws one outline behind the combined silhouette. Source sprites can keep independent materials, colors, transforms, and Animator-driven sprite changes.

## Use

1. Add **Blackhole/Rendering/Composite Sprite Outline** to the common parent of the sprites.
2. Add every contributing `SpriteRenderer` to **Source Renderers**. Adding the component automatically collects child sprite renderers as a starting point.
3. Set the mask resolution, outline color, pixel width, and padding. The material fields are optional; module defaults load automatically.

At runtime, `SetSourceRenderers`, `SetOutline`, and `SetTextureSize` can reconfigure the effect. One mask RenderTexture is allocated per enabled outline component.
